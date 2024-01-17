using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YawVR;
using System.Net;

/// <summary>
/// This Class can be used as a base for RevSim UI scripts.
/// Inherit this class in the UI script to make sure all YawController needs are met. 
/// </summary>
public class RevSimUI_Yaw : MonoBehaviour, YawControllerDelegate
{
    [Header("Yaw Settings Panel")]
    [SerializeField] private GameObject yawSettingsPanel;
    [SerializeField] private GameObject  yawDeviceListItem;
    [SerializeField] private Button connectYawButton;
    [SerializeField] private List<Button> startYawButton = new List<Button>(), stopYawButton = new List<Button>();
    [SerializeField] private Text yawConnectedDeviceText, yawErrorText, yawIPText, yawUDPText, yawTCPText;
    
    private Text connectText;
    private int? udpPort = 50010;
    private int? tcpPort;
    private IPAddress ipAddress;
    private YawDevice selectedDevice;
    private List<IPAddress> foundIps = new List<IPAddress>();
    private Coroutine searchCoroutine;

    /// <summary>
    /// Make sure to add:
    /// override void Start()
    /// { base.Start(); ...}
    /// In the inheriting Class
    /// </summary>
    private void Start()
    {
        // References Text in each Button
        connectText = connectYawButton.GetComponentInChildren<Text>();
        // Sets the delegates of YawController to this class
        YawController.Instance().ControllerDelegate = this;
        YawUI_UpdateUI(YawController.Instance().State);

    }


#region Callable UI Methods
    /// <summary>
    /// This method toggles the YawSettingsPanel active or unactive.
    /// When activating the DiscoverDevices method of YawController is called.
    /// </summary>
    public void YawUI_TogglePanel() 
    {
        YawUI_ClearList();
        if (!yawSettingsPanel.activeInHierarchy) {
            yawSettingsPanel.SetActive(true);
            searchCoroutine = StartCoroutine(SearchForDevices());
            if (YawController.Instance().State == ControllerState.Started) 
            {
                // TODO Do something here?
            }
        }
        else {
            yawSettingsPanel.SetActive(false);
            if (YawController.Instance().State == ControllerState.Connected)
            {
                // TODO Do something here?
            }
            StopCoroutine(searchCoroutine);
        }
    }

    /// <summary>
    /// This method calls the StopDevice method of YawController.
    /// This disengages the motors on the connected YAW2.
    /// </summary>
    public void YawUI_StopDevice()
    {
        // BUG This is not working is it?
        if (YawController.Instance().State == ControllerState.Started) 
            YawController.Instance().StopDevice(true, null, (error) => {YawUI_DisplayError(error);});
    }

    /// <summary>
    /// This method calls the StartDevice method of YawController. 
    /// This engages the YAW2 and its motors when connected!!!
    /// </summary>
    public void YawUI_StartDevice() 
    {
        if (YawController.Instance().State == ControllerState.Connected) 
            YawController.Instance().StartDevice();
    }

    /// <summary>
    /// This method calls the ConnectToDevice method of YawController.
    /// This connects to the selected YAW device.
    /// </summary>
    public void YawUI_ConnectDevice()
    {
        if (selectedDevice != null)
        {
            if (YawController.Instance().Device != null && YawUI_CheckDevice(YawController.Instance().Device, selectedDevice))
            {
                if (YawController.Instance().State == ControllerState.Connected)
                    YawUI_DisconnectDevice();
                return;
            }
            YawController.Instance().ConnectToDevice(selectedDevice, null, (error) => {YawUI_DisplayError(error);});
        }
        else if (YawController.Instance().State == ControllerState.Connected)
            YawUI_DisconnectDevice();
    }

    /// <summary>
    /// This calls the DisconnectFromDevice method in YawController.
    /// This disconnects from the currently connected device.
    /// TODO Make sure this is SAFE in every situation!!!
    /// </summary>
    public void YawUI_DisconnectDevice()
    {
        if (YawController.Instance().State != ControllerState.Initial)
            YawController.Instance().DisconnectFromDevice(null, (error) => {YawUI_DisplayError(error);} );
    }

#endregion

#region Supporting Methods and IEnumerators
    /// <summary>
    /// Updates the YawUI.
    /// </summary>
    /// <param name="state">State of connected YAW device.</param>
    private void YawUI_UpdateUI(ControllerState state) {
        switch (state)
        {
            // TODO Add functionality?
            case ControllerState.Initial:           // Initial State; No device connected
                connectYawButton.interactable = true;
                SetButtonsInteractable(false, startYawButton);
                SetButtonsInteractable(false, stopYawButton);
                connectText.text = "Connect";
                yawConnectedDeviceText.text = "No Device Connected";
                ipAddress = null;
                yawIPText.text = "";
                tcpPort = null;
                yawTCPText.text = "";
                break;
            case ControllerState.Connecting:        // Connecting State; Controller is connecting to device
                connectYawButton.interactable = false;
                SetButtonsInteractable(false, startYawButton);
                SetButtonsInteractable(false, stopYawButton);
                connectText.text = "Connecting...";
                break;
            case ControllerState.Connected:         // Connected State; Controller is connected; Not in drive
                yawConnectedDeviceText.text = "Active Device: " + YawController.Instance().Device.Name;
                connectText.text = "Disconnect";
                connectYawButton.interactable = true;
                SetButtonsInteractable(true, startYawButton);
                SetButtonsInteractable(false, stopYawButton);
                break;
            case ControllerState.Starting:          // Starting State; Controller is starting motor drivers
                connectYawButton.interactable = false;
                SetButtonsInteractable(false, startYawButton);
                SetButtonsInteractable(false, stopYawButton);
                break;
            case ControllerState.Started:           // Started State; Controller and Device are in drive/operation state 
                connectYawButton.interactable = false;
                SetButtonsInteractable(false, startYawButton);
                SetButtonsInteractable(true, stopYawButton);
                break;
            case ControllerState.Stopping:          // Stopping State; Controller is stopping motor drivers
                connectYawButton.interactable = false;
                SetButtonsInteractable(false, startYawButton);
                SetButtonsInteractable(false, stopYawButton);
                break;
            case ControllerState.Disconnecting:     // Disconnecting State; Controller is disconnecting from Device
                connectYawButton.interactable = false;
                SetButtonsInteractable(false, startYawButton);
                SetButtonsInteractable(false, stopYawButton);
                connectText.text = "Disconnecting...";
                break;
        }
    }

    private void SetButtonsInteractable(bool interactable, List<Button> buttons)
    {
        foreach(Button button in buttons)
            button.interactable = interactable;
    }

    /// <summary>
    /// Clears the devicelist
    /// </summary>
    private void YawUI_ClearList() 
    {
        foreach (Transform t in yawDeviceListItem.transform.parent) 
            if (t.gameObject.activeSelf)
                Destroy(t.gameObject);
        foundIps.Clear();
    }

    /// <summary>
    /// This method compares two YAW Devices and returns if they are on the same port or not.
    /// </summary>
    /// <param name="device">Current Device</param>
    /// <param name="toDevice">New Device</param>
    /// <returns>TRUE if the devices are the same</returns>
    private bool YawUI_CheckDevice(YawDevice device, YawDevice toDevice)
    {
        if (device.Id == toDevice.Id && device.TCPPort == toDevice.TCPPort && device.UDPPort == toDevice.UDPPort) 
            return true;
        return false;
    }

    /// <summary>
    /// Displays an ERROR message to YawErrorText for a duration of seconds.
    /// </summary>
    /// <param name="error">The ERROR message to be displayed</param>
    /// <param name="duration">The duration (in seconds) the ERROR message is displayed. </param>
    private void YawUI_DisplayError(string error, int duration = 10)
    {
        if (yawErrorText.text != "")
            StopCoroutine(ClearError(duration));
        yawErrorText.text = error;
        StartCoroutine(ClearError(duration));
    }

    /// <summary>
    /// Update the Yaw IP address, UDP and TCP ports in the YawSettingsPanel 
    /// </summary>
    /// <param name="device">The Device of which data to display</param>
    private void YawUI_DeviceListItemPressed(YawDevice device)
    {
        if (device.Status != DeviceStatus.Available || YawController.Instance().State != ControllerState.Initial) 
            return;
        yawIPText.text = device.IPAddress.ToString();
        yawUDPText.text = device.UDPPort.ToString();
        yawTCPText.text = device.TCPPort.ToString();
        selectedDevice = device;
        connectYawButton.interactable = true;
    }   

    private IEnumerator ClearError(int duration)
    {
        yield return new WaitForSeconds(duration);
        yawErrorText.text = "";
    }

    private IEnumerator SearchForDevices()
    {
        Debug.Log("started searching for devices");
        while (true)
        {
            if (udpPort != null && udpPort > 1024)
                YawController.Instance().DiscoverDevices(udpPort.Value);
            yield return new WaitForSeconds(0.5f);
        }
    }

#endregion

#region YawController Delegate Methods
    public void DidFoundDevice(YawDevice device)
    {
        if (!foundIps.Contains(device.IPAddress)) 
        {
            Debug.Log("Found device: " + device.Name);
            foundIps.Add(device.IPAddress);

            GameObject go = Instantiate(yawDeviceListItem, yawDeviceListItem.transform.parent);
            go.SetActive(true);

            string buttonText = device.Name;
            if (device.Status != DeviceStatus.Available) buttonText += " - Reserved";

            go.GetComponentInChildren<Text>().text = buttonText;
            go.GetComponent<Button>().onClick.AddListener(delegate { YawUI_DeviceListItemPressed(device); });
        }
    }

    public void DeviceStoppedFromApp() => Debug.Log("DEVICE STOPPED FROM CONFIGAPP");
    public void DeviceStartedFromApp() => Debug.Log("DEVICE STARTED FROM CONFIGAPP");
    public void DidDisconnectFrom(YawDevice device) => YawUI_DisplayError("Device disconnected");
    public void ControllerStateChanged(ControllerState state) => YawUI_UpdateUI(state);

#endregion

}
