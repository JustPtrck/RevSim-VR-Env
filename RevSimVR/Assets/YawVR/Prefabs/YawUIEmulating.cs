using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using YawVR;
using TMPro;
using System;

public class YawUIEmulating : MonoBehaviour, YawControllerDelegate
{
    [SerializeField] private GameObject yaw1Model;
    [SerializeField] private GameObject yaw2Model;
    [SerializeField] private GameObject parkButton;
    [SerializeField] private GameObject MCCalibrateButton;
    [SerializeField] private GameObject disconnectButton;
    [SerializeField] private GameObject connectButton;
    [SerializeField] private GameObject rememberButton;
    [SerializeField] private GameObject batteryIcon;
    [SerializeField] private TextMeshProUGUI batteryHoverText;
    [SerializeField] private GameObject heatIcon;
    [SerializeField] private TextMeshProUGUI heatHoverText;
    [SerializeField] private GameObject advancedPanel;
    [SerializeField] private GameObject advancedButton;
    [SerializeField] private GameObject buttonSample;
    [SerializeField] private MotionCompensation motionCompensation;
    [SerializeField] private TMP_InputField ipAddressInputField;
    [SerializeField] private TMP_InputField udpPortInputField;
    [SerializeField] private TMP_InputField tcpPortInputField;
    [SerializeField] private Text errorText;

    bool connected = false;

    private Image batteryImage;
    private Image heatImage;
    [SerializeField] private float TemperatureAmount;

    [Space]
    [Header("Limits and Multipliers")]
    [SerializeField] private Slider yawMultiplier;
    [SerializeField] private Slider pitchMultiplier;
    [SerializeField] private Slider rollMultiplier;
    [SerializeField] private Slider yawLimit;
    [SerializeField] private Slider pitchForwardLimit;
    [SerializeField] private Slider pitchBackwardLimit;
    [SerializeField] private Slider rollLimit;
    [SerializeField] private float yawMultiplierDef, pitchMultiplierDef, rollMultiplierDef, yawLimitDef, pitchForwardLimitDef, pitchBackwardLimitDef, rollLimitDef;

    [Header("LightMode")]
    [SerializeField] private Button changeLedColorBTN;
    [SerializeField] private Button rainbowBTN;
    [SerializeField] private Button transitionBTN;
    [SerializeField] private Button blinkingBTN;

    [Header("Motion Comp Modes")]
    [SerializeField] private Button deviceModeBTN;
    [SerializeField] private Button controllerModeBTN;
    [SerializeField] private Button gameDataModeBTN;

    [Space]
    [SerializeField] private Button calibrateButton;
    [SerializeField] private Button DefaultButton;

    int yawversion=0;

    private int? udpPort = 50010;
    private int? tcpPort;
    private IPAddress ipAddress;
    private Coroutine searchCoroutine = null;
    private List<IPAddress> foundIps = new List<IPAddress>();
    private List<YawDevice> foundDevices = new List<YawDevice>();
    private YawDevice selectedDevice = null;
    private bool m_bFirstConnected = false;
    private bool m_bIsInit = false;

    private void Start()
    {
        YawController.Instance().ControllerDelegate = this;

        batteryImage = batteryIcon.GetComponent<Image>();
        heatImage = heatIcon.GetComponent<Image>();
        m_bIsInit = true;
        Disconnect();
        m_bIsInit = false;
        advancedPanel.SetActive(false);

        udpPortInputField.text = udpPort.ToString();
        udpPortInputField.onValueChanged.AddListener(delegate { UDPPortInputFieldTextDidChange(udpPortInputField); });
        tcpPortInputField.text = tcpPort.ToString();
        tcpPortInputField.onValueChanged.AddListener(delegate { TCPPortInputFieldTextDidChange(tcpPortInputField); });
        ipAddressInputField.onValueChanged.AddListener(delegate { IPAddressInputFieldTextDidChange(ipAddressInputField); });

        //if (false == rememberButton.GetComponent<Toggle>().isOn) 
        //{
        //    Disconnect();
        //}
        rememberButton.GetComponent<Toggle>().onValueChanged.AddListener((value) => { RememberButton_OnValueChanged(value); });

        searchCoroutine = StartCoroutine(SearchForDevices());
        RefreshLayout(YawController.Instance().State);

        
    }

    void RememberButton_OnValueChanged(bool value)
    {
        YawController.Instance().IsRememberDevice = value;

        if (true == value)
        {
            YawController.Instance().SavedUdpPort = udpPort;
            YawController.Instance().SavedTcpPort = tcpPort;
            YawController.Instance().SavedIpAddress = ipAddressInputField.text;
        }
        else 
        {
            YawController.Instance().SavedUdpPort = 0;
            YawController.Instance().SavedTcpPort = 0;
            YawController.Instance().SavedIpAddress = "";
        }
    }

    private void OnDisable()
    {
        StopCoroutine(searchCoroutine);
        udpPortInputField.onValueChanged.RemoveAllListeners();
        tcpPortInputField.onValueChanged.RemoveAllListeners();
        ipAddressInputField.onValueChanged.RemoveAllListeners();
        rememberButton.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
    }

    private void Update()
    {
        if (false == connected && false == m_bFirstConnected && true == YawController.Instance().IsRememberDevice)
        {
            foreach (YawDevice device in foundDevices) 
            {
                if (device.IPAddress.ToString() == YawController.Instance().SavedIpAddress) 
                {
                    selectYaw(device);
                    Connect();
                    rememberButton.GetComponent<Toggle>().isOn = true;
                    m_bFirstConnected = true;
                    return;
                }
            }
            
        }

        // battery
        batteryImage.fillAmount = m_fBatteryPercent;
        batteryHoverText.text = ((int)(100.0f * m_fBatteryPercent)).ToString() + "%";

        // temperature
        float fTemperaturePercent = Mathf.InverseLerp(0.0f, 65.0f, (float)m_nTemperature);
        heatImage.fillAmount = fTemperaturePercent;
        heatHoverText.text = m_nTemperature.ToString() + "°C";
    }

    public void ControllerStateChanged(ControllerState state)
    {
        RefreshLayout(state);
    }

    private void RefreshLayout(ControllerState state)
    {
        switch (state)
        {
            case ControllerState.Initial:
                //connectButton.GetComponent<Button>().interactable = false;
                //connectButton.GetComponentInChildren<TMP_Text>().text = "Connect";
                //setupTitleLabel.text = "Set target YAW device";
                //disconnectButton.gameObject.SetActive(false);
                ipAddress = null;
                ipAddressInputField.text = "";
                tcpPort = null;
                tcpPortInputField.text = "";


                break;
            case ControllerState.Connecting:
                //connectButton.GetComponent<Button>().interactable = false;
                //connectButton.GetComponentInChildren<Text>().text = "Connecting...";
                break;
            case ControllerState.Connected:
                //connectButton.GetComponent<Button>().interactable = false;
                //disconnectButton.GetComponentInChildren<Text>().text = "Disconnect";
                //setupTitleLabel.text = "Active device: " + YawController.Instance().Device.Name;
                //disconnectButton.gameObject.SetActive(true);
                //disconnectButton.GetComponent<Button>().interactable = true;
                //connectButton.GetComponentInChildren<Text>().text = "Connect";
                break;
            case ControllerState.Starting:
                break;
            case ControllerState.Started:
                break;
            case ControllerState.Stopping:
                break;
            case ControllerState.Disconnecting:
                //connectButton.GetComponent<Button>().interactable = false;
                //disconnectButton.GetComponent<Button>().interactable = false;
                //disconnectButton.GetComponentInChildren<Text>().text = "Disconnecting...";
                break;
        }
    }

    void UDPPortInputFieldTextDidChange(TMP_InputField inputField)
    {
        //   availableDevices.Clear();
        //  LayoutDeviceButtons(availableDevices);
        int portNumber;
        if (int.TryParse(inputField.text, out portNumber))
        {
            //TODO: - Error - not a port number
            udpPort = portNumber;
        }
        else
        {
            selectedDevice = null;
            udpPort = null;
        }
        SetDeviceFromPortAndIp();
    }

    void TCPPortInputFieldTextDidChange(TMP_InputField inputField)
    {
        int portNumber;
        if (int.TryParse(inputField.text, out portNumber))
        {
            tcpPort = portNumber;
        }
        else
        {
            //TODO: - Error - not a port number
            selectedDevice = null;
            tcpPort = null;
        }
        SetDeviceFromPortAndIp();
    }

    void IPAddressInputFieldTextDidChange(TMP_InputField inputField)
    {
        IPAddress ipFromString;
        if (IPAddress.TryParse(inputField.text, out ipFromString))
        {
            this.ipAddress = ipFromString;
        }
        else
        {
            //TODO: - Error - not an ip address
            selectedDevice = null;
            ipAddress = null;
        }
        SetDeviceFromPortAndIp();
    }

    void SetDeviceFromPortAndIp()
    {
        if (ipAddress != null && udpPort != null && tcpPort != null)
        {
            //string hostName = Dns.GetHostEntry(this.ipAddress).HostName;
            selectedDevice = new YawDevice(ipAddress, YawVR.DeviceType.YAW1, tcpPort.Value, udpPort.Value, "Manually set device", "Manually set device", DeviceStatus.Unknown); //TODO: - status
            connectButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            connectButton.GetComponent<Button>().interactable = false;
        }
    }


    public void selectYaw(YawDevice device)
    {
        if (!connected)
        {
            if (false == DeviceListItemPressed(device)) 
            {
                return;
            }

            if (device.type == YawVR.DeviceType.YAW1)
            {
                yawversion = 1;
                yaw1Model.SetActive(true);
                yaw2Model.SetActive(false);
                //connectButton.SetActive(true);
            }
            else if (device.type == YawVR.DeviceType.YAW2) 
            {
                yawversion = 2;
                yaw1Model.SetActive(false);
                yaw2Model.SetActive(true);
                //connectButton.SetActive(true);
            }

            ;
        }
        
    }

    bool DeviceListItemPressed(YawDevice device)
    {
        if (device.Status != DeviceStatus.Available || YawController.Instance().State != ControllerState.Initial)
        {
            return false;
        }
        ipAddressInputField.text = device.IPAddress.ToString();
        udpPortInputField.text = device.UDPPort.ToString();
        tcpPortInputField.text = device.TCPPort.ToString();
        selectedDevice = device;

        return true;
    }

    bool ConnectButtonPressed()
    {
        if (selectedDevice != null)
        {
            if (YawController.Instance().Device != null && SameDevice(YawController.Instance().Device, selectedDevice))
            {
                return false;
            }

            m_fBatteryPercent = 0.0f;
            m_nTemperature = 0;

            YawController.Instance().ConnectToDevice(
                selectedDevice,
                null,
               (error) =>
               {
                   ShowError(error);
               });

            return true;
        }

        return false;
    }

    private void ShowError(string error, int duration = 10)
    {
        if (errorText.text != "")
        {
            StopCoroutine(ClearError(duration));
        }
        errorText.text = error;
        StartCoroutine(ClearError(duration));

        Debug.Log("ERROR!: " + error);
        Disconnect();
    }

    private IEnumerator ClearError(int duration)
    {
        yield return new WaitForSeconds(duration);
        errorText.text = "";
    }

    private bool SameDevice(YawDevice device, YawDevice toDevice)
    {
        if (device.Id == toDevice.Id && device.TCPPort == toDevice.TCPPort && device.UDPPort == toDevice.UDPPort) return true;
        return false;
    }

    bool DisconnectButtonPressed()
    {
        try
        {
            if (YawController.Instance().State != ControllerState.Initial)
            {
                YawController.Instance().DisconnectFromDevice(
                    null,
                    (error) =>
                    {
                        ShowError(error);
                    });
                return true;
            }
        }
        catch (Exception ex) 
        {
            ex.ToString();
            return false;
        }

        return false;
    }

    public void ParkDevice()
    {
        if (YawController.Instance().State == ControllerState.Started)
        {
            YawController.Instance().StopDevice(true);
        }
    }

    public void StartDevice()
    {
        if (YawController.Instance().State == ControllerState.Connected)
        {
            YawController.Instance().StartDevice();
        }
    }

    public void Connect()
    {
        bool isOn = rememberButton.GetComponent<Toggle>().isOn;
        RememberButton_OnValueChanged(isOn);

        ConnectButtonPressed();
        //if (false == ConnectButtonPressed()) 
        //{
        //    return;
        //}

        //if (yawversion == 1)
        //{
        //    batteryIcon.SetActive(true);
        //}
        //
        //MCCalibrateButton.SetActive(true);
        //disconnectButton.SetActive(true);
        //rememberButton.SetActive(true);
        //heatIcon.SetActive(true);
        //parkButton.SetActive(true);
        ////connectButton.SetActive(false);
        //
        //EnableSlidersAndColors();
        //connected = true;
        //yawversion = 0;

        //Invoke("StartDevice", 2.0f);
    }

    public void Disconnect()
    {
        if (false == m_bIsInit) 
        {
            RememberButton_OnValueChanged(false);
        }

        DisconnectButtonPressed();

        MCCalibrateButton.SetActive(false);
        disconnectButton.SetActive(false);
        rememberButton.SetActive(false);
        heatIcon.SetActive(false);
        //connectButton.SetActive(false);
        yaw1Model.SetActive(false);
        batteryIcon.SetActive(false);
        yaw2Model.SetActive(false);
        parkButton.SetActive(false);
        
        connected = false;
        DisableSlidersAndButtons();
    }

    public void advanceToggle()
    {
        advancedPanel.SetActive(!advancedPanel.activeSelf);
    }

    public void SetDefaultValue()
    {
        yawMultiplier.value = yawMultiplierDef;
        pitchMultiplier.value = pitchMultiplierDef;
        rollMultiplier.value = rollMultiplierDef;
        yawLimit.value = yawLimitDef;
        pitchForwardLimit.value = pitchForwardLimitDef;
        pitchBackwardLimit.value = pitchBackwardLimitDef;
        rollLimit.value = rollLimitDef; 
    }

    private void DisableSlidersAndButtons()
    {
        yawMultiplier.interactable = false;
        pitchMultiplier.interactable = false;
        rollMultiplier.interactable = false;
        yawLimit.interactable = false;
        pitchForwardLimit.interactable = false;
        pitchBackwardLimit.interactable = false;
        rollLimit.interactable = false;
        changeLedColorBTN.interactable = false;
        rainbowBTN.interactable = false;
        transitionBTN.interactable = false;
        blinkingBTN.interactable = false;
        calibrateButton.interactable = false;
        DefaultButton.interactable = false;
        deviceModeBTN.interactable = false;
        controllerModeBTN.interactable = false;
        gameDataModeBTN.interactable = false;
    }

    private void EnableSlidersAndColors()
    {
        yawMultiplier.interactable = true;
        pitchMultiplier.interactable = true;
        rollMultiplier.interactable = true;
        yawLimit.interactable = true;
        pitchForwardLimit.interactable = true;
        pitchBackwardLimit.interactable = true;
        rollLimit.interactable = true;
        changeLedColorBTN.interactable = true;
        rainbowBTN.interactable = true;
        transitionBTN.interactable = true;
        blinkingBTN.interactable = true;
        calibrateButton.interactable = true;
        DefaultButton.interactable = true;
        deviceModeBTN.interactable = true;
        controllerModeBTN.interactable = true;
        gameDataModeBTN.interactable = true;
    }

    void YawControllerDelegate.ControllerStateChanged(ControllerState state)
    {
        Debug.Log("ControllerStateChanged: " + state.ToString());
        switch (state) 
        {
            case (ControllerState.Connected): 
                {
                    if (yawversion == 1)
                    {
                        batteryIcon.SetActive(true);
                    }
                    
                    MCCalibrateButton.SetActive(true);
                    disconnectButton.SetActive(true);
                    rememberButton.SetActive(true);
                    heatIcon.SetActive(true);
                    parkButton.SetActive(true);
                    //connectButton.SetActive(false);
                    
                    EnableSlidersAndColors();
                    connected = true;
                    yawversion = 0;
                    break; 
                }
        }
    }

    void YawControllerDelegate.DidFoundDevice(YawDevice device)
    {
        if (!foundIps.Contains(device.IPAddress))
        {
            Debug.Log("Found device: " + device.Name);
            foundIps.Add(device.IPAddress);
            foundDevices.Add(device);

            GameObject go = Instantiate(buttonSample, buttonSample.transform.parent);
            go.SetActive(true);

            string buttonText = device.Name;
            if (device.Status != DeviceStatus.Available) buttonText += " - Reserved";

            go.GetComponentInChildren<Text>().text = buttonText;
            if (device.type == YawVR.DeviceType.YAW1)
            {
                go.GetComponent<Button>().onClick.AddListener(delegate { selectYaw(device); });
            }
            else if (device.type == YawVR.DeviceType.YAW2)
            {
                go.GetComponent<Button>().onClick.AddListener(delegate { selectYaw(device); });
            }
        }
    }

    void YawControllerDelegate.DidDisconnectFrom(YawDevice device)
    {
        ShowError("Device disconnected");
    }

    void YawControllerDelegate.DeviceStoppedFromApp()
    {
        Debug.Log("DEVICE STOPPED FROM CONFIGAPP");
    }

    void YawControllerDelegate.DeviceStartedFromApp()
    {
        Debug.Log("DEVICE STARTED FROM CONFIGAPP");
    }

    private IEnumerator SearchForDevices()
    {
        Debug.Log("started searching for devices");
        while (true)
        {
            if (udpPort != null && udpPort > 1024)
            {
                YawController.Instance().DiscoverDevices(udpPort.Value);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private float m_fBatteryPercent = 0.0f;
    public void SetBatteryPercent(float fBatteryPercent)
    {
        m_fBatteryPercent = fBatteryPercent;
    }


    private int m_nTemperature = 0;
    public void SetTemperature(int nTemperature)
    {
        m_nTemperature = nTemperature;
    }

    public void MotionCompensationMode_BindYawVRController() 
    {
        motionCompensation.SetDevice(MotionCompensation.enumYawPitchRollDevice.YawVRController);
    }

    public void MotionCompensationMode_BindLeftOrRightController()
    {
        if (MotionCompensation.enumYawPitchRollDevice.YawVRController == motionCompensation.GetDevice())
        {
            motionCompensation.SetDevice(MotionCompensation.enumYawPitchRollDevice.LeftController);
        }
        else if (MotionCompensation.enumYawPitchRollDevice.LeftController == motionCompensation.GetDevice())
        {
            motionCompensation.SetDevice(MotionCompensation.enumYawPitchRollDevice.RightController);
        }
        else if (MotionCompensation.enumYawPitchRollDevice.RightController == motionCompensation.GetDevice()) 
        {
            motionCompensation.SetDevice(MotionCompensation.enumYawPitchRollDevice.LeftController);
        }
    }

    public void CalibrateDevice()
    {
        if (YawController.Instance().State != ControllerState.Initial)
        {
            YawController.Instance().CalibrateDevice(true);
        }
    }
}
