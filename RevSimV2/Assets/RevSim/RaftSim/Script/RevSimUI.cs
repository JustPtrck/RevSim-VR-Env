using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;
using YawVR;
using System.Net;
using UnityEngine.XR;
using UnityEditor.XR.LegacyInputHelpers;
public class RevSimUI : MonoBehaviour, YawControllerDelegate
{
    [Header("Tabs Large")]
    [SerializeField] private GameObject waveTab;
    [SerializeField] private GameObject collectableTab, preferencesTab, appTab, yawTab, observerTab;
    [Header("Tabs Small")]
    [SerializeField] private GameObject waveTabS;
    [SerializeField] private GameObject collectableTabS, preferencesTabS, appTabS, yawTabS, observerTabS;
    [Header("Yaw Rotation")]
    [SerializeField] private Transform yawTrackerTransform;
    [SerializeField] private Text yaw, pitch, roll;
    [Header("VR Preferences")]
    [SerializeField] private Camera camera; 
    [SerializeField] private Light lightSource; 
    [SerializeField] private Volume volume;
    private Vignette vg;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private CameraOffset VROffset;
    [SerializeField] private Slider FOVSlider, vignetteSlider, brightnessSlider, heightSlider, masterSlider;
    [SerializeField] private InputField FOVField, vignetteField, brightnessField, heightField, masterField; 
    private float FOV, vignette, brightness;
    [Header("Level Selection")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Slider timeSlider, levelSlider, modSlider;
    [SerializeField] private InputField timeField, levelField, modField;

    [Header("Spawn Settings")]
    [SerializeField] private CollectableSpawn collectableSpawn;
    [SerializeField] private Slider radiusSlider, horizontalSlider, verticalSlider, offsetSlider;
    [SerializeField] private InputField radiusField, horizontalField, verticalField, offsetField;

    [Header("App Settings")]
    [SerializeField] private Text closingText;
    
    private void Start()
    {
        volume.profile.TryGet(out vg);
        UpdateVignette();
        UpdateBrightness();
        UpdateFOV();
        UpdateLevelManagerValues();
        UpdateMasterVol();
        UpdateRadius();
        UpdateHorizontal();
        UpdateVertical();
        UpdateOffset();
        YawController.Instance().ControllerDelegate = this;
        RefreshLayout(YawController.Instance().State);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 angles = yawTrackerTransform.rotation.eulerAngles;
        yaw.text = FormatAngles(angles.y);
        pitch.text = FormatAngles(angles.x);
        roll.text = FormatAngles(angles.z);
    }

    private string FormatAngles(float angle)
    {
        if (angle > 180)
            return (angle - 360).ToString("0.000");
        else
            return angle.ToString("0.000");
    }

#region YAW
    [Header("Yaw Settings")]
    [SerializeField] private GameObject settingsPanel, deviceListItemPrefab;
    [SerializeField] private Button connectButton, startDevice, stopDevice;
    [SerializeField] private Text subTitle, errorText, IP, UDP, TCP;
    private int? udpPort = 50010;
    private int? tcpPort;
    private IPAddress ipAddress;
    private YawDevice selectedDevice;
    private List<IPAddress> foundIps = new List<IPAddress>();
    private Coroutine searchCoroutine;
    public void DeviceStoppedFromApp()
    {
        Debug.Log("DEVICE STOPPED FROM CONFIGAPP");
    }
    public void DeviceStartedFromApp()
    {
        Debug.Log("DEVICE STARTED FROM CONFIGAPP");
    }
    public void DidDisconnectFrom(YawDevice device)
    {
        ShowError("Device disconnected");
    }

    public void YawControllerStop()
    {
        if (YawController.Instance().State == ControllerState.Started) 
            YawController.Instance().StopDevice(true);
    }

    public void YawControllerStart() 
    {
        if (YawController.Instance().State == ControllerState.Connected) 
            YawController.Instance().StartDevice();
    }

    public void YawControllerConnect()
    {
        if (selectedDevice != null)
        {
            if (YawController.Instance().Device != null && SameDevice(YawController.Instance().Device, selectedDevice)) 
                return;
            YawController.Instance().ConnectToDevice(selectedDevice, null, (error) => {ShowError(error);});
        } 
        else if (YawController.Instance().State == ControllerState.Connected)
            YawControllerDisconnect();
    }

    public void YawControllerDisconnect()
    {
        if (YawController.Instance().State != ControllerState.Initial)
            YawController.Instance().DisconnectFromDevice(null, (error) => {ShowError(error);} );
    }

    private void ShowError(string error, int duration = 10)
    {
        if (errorText.text != "")
            StopCoroutine(ClearError(duration));
        errorText.text = error;
        StartCoroutine(ClearError(duration));
    }
    private IEnumerator ClearError(int duration)
    {
        yield return new WaitForSeconds(duration);
        errorText.text = "";
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
    void DeviceListItemPressed(YawDevice device)
    {
        if (device.Status != DeviceStatus.Available || YawController.Instance().State != ControllerState.Initial) 
            return;
        IP.text = device.IPAddress.ToString();
        UDP.text = device.UDPPort.ToString();
        TCP.text = device.TCPPort.ToString();
        selectedDevice = device;
        connectButton.interactable = true;
    }   
    public void DidFoundDevice(YawDevice device)
    {
        if (!foundIps.Contains(device.IPAddress)) 
        {
            Debug.Log("Found device: " + device.Name);
            foundIps.Add(device.IPAddress);

            GameObject go = Instantiate(deviceListItemPrefab, deviceListItemPrefab.transform.parent);
            go.SetActive(true);

            string buttonText = device.Name;
            if (device.Status != DeviceStatus.Available) buttonText += " - Reserved";

            go.GetComponentInChildren<Text>().text = buttonText;
            go.GetComponent<Button>().onClick.AddListener(delegate { DeviceListItemPressed(device); });
        }
    }
    private bool SameDevice(YawDevice device, YawDevice toDevice)
    {
        if (device.Id == toDevice.Id && device.TCPPort == toDevice.TCPPort && device.UDPPort == toDevice.UDPPort) 
            return true;
        return false;
    }
    public void ControllerStateChanged(ControllerState state)
    {
        RefreshLayout(state);
    }

    private void RefreshLayout(ControllerState state) {
        switch (state)
        {
            // TODO Revise this?
            case ControllerState.Initial:
                connectButton.interactable = false;
                connectButton.GetComponentInChildren<Text>().text = "Connect";
                subTitle.text = "No Device Connected";
                break;
            case ControllerState.Connecting:
                connectButton.interactable = false;
                connectButton.GetComponentInChildren<Text>().text = "Connecting...";
                break;
            case ControllerState.Connected:
                subTitle.text = "Active Device: " + YawController.Instance().Device.Name;
                connectButton.GetComponentInChildren<Text>().text = "Disconnect";
                connectButton.interactable = true;
                break;
            case ControllerState.Starting:
                break;
            case ControllerState.Started:
                break;
            case ControllerState.Stopping:
                break;
            case ControllerState.Disconnecting:
                connectButton.interactable = false;
                connectButton.GetComponentInChildren<Text>().text = "Disconnecting...";
                break;
        }
    }
    public void HideShowPanel() 
    {
        ClearList();
        if (!settingsPanel.activeInHierarchy) {
            settingsPanel.SetActive(true);
            searchCoroutine = StartCoroutine(SearchForDevices());
            if (YawController.Instance().State == ControllerState.Started) 
            {
                
            }
        }
        else {
            settingsPanel.SetActive(false);
            if (YawController.Instance().State == ControllerState.Connected)
            {
                
            }
            StopCoroutine(searchCoroutine);
        }
    }
    private void ClearList() 
    {
        foreach (Transform t in deviceListItemPrefab.transform.parent) 
            if (t.gameObject.activeSelf)
                Destroy(t.gameObject);
        foundIps.Clear();
    }


#endregion

#region LevelManager
    public void UpdateLevelManagerValues()
    {
        timeSlider.value = levelManager.values.Item1;
        timeField.text = levelManager.values.Item1.ToString("0");
        levelSlider.value = levelManager.values.Item2;
        levelField.text = levelManager.values.Item2.ToString();
        modSlider.value = levelManager.values.Item3;
        modField.text = levelManager.values.Item3.ToString();
    }

    public void SetTransitionTime(float value)
    {
        levelManager.SetTransitionTime(value);
        UpdateLevelManagerValues();
    }
    public void SetTransitionTime(string value) => SetTransitionTime(float.Parse(value));

    public void SetLevel(float value)
    {
        levelManager.LevelSelect(value);
        UpdateLevelManagerValues();
    }
    public void SetLevel(string value) => SetLevel(float.Parse(value));
    public void SetMod(float value)
    {
        levelManager.SetSteepnessMod(value);
        UpdateLevelManagerValues();
    }
    public void SetMod(string value) => SetMod(float.Parse(value));

#endregion

#region VR Preferences
    public void UpdateFOV()
    {
        FOVField.text = camera.fieldOfView.ToString("0");
        FOVSlider.value = camera.fieldOfView;
    }
    public void SetFOV(float value)
    {
        camera.fieldOfView = value;
        UpdateFOV();
    }
    public void SetFOV(string value) => SetFOV(float.Parse(value));

    public void UpdateVignette()
    {
        vignetteField.text = vg.intensity.value.ToString("0.000");
        vignetteSlider.value = vg.intensity.value;
    }
    public void SetVignette(float value)
    {
        vg.intensity.value = value;
        UpdateVignette();
    }
    public void SetVignette(string value) => SetVignette(float.Parse(value));

    public void UpdateBrightness()
    {
        brightnessField.text =  lightSource.intensity.ToString("0.000");
        brightnessSlider.value = lightSource.intensity;
    }
    public void SetBrightness(float value)
    {
        lightSource.intensity = value;
        UpdateBrightness();
    }
    public void SetBrightness(string value) => SetBrightness(float.Parse(value));

    public void UpdateHeight()
    {
        heightField.text =  VROffset.cameraYOffset.ToString("0.00");
        heightSlider.value = VROffset.cameraYOffset;
    }
    public void SetHeight(float value)
    {
        VROffset.cameraYOffset = value;
        UpdateHeight();
    }
    public void SetHeight(string value) => SetHeight(float.Parse(value));

#endregion

#region Collectible
    public void UpdateRadius()
    {
        radiusField.text =  collectableSpawn.radius.ToString("0.00");
        radiusSlider.value = collectableSpawn.radius;
    }
    public void SetRadius(float value)
    {
        collectableSpawn.radius = value;
        UpdateRadius();
    }
    public void SetRadius(string value) => SetRadius(float.Parse(value));

    public void UpdateHorizontal()
    {
        horizontalField.text =  collectableSpawn.horizontal.ToString("0.00");
        horizontalSlider.value = collectableSpawn.horizontal;
    }
    public void SetHorizontal(float value)
    {
        collectableSpawn.horizontal = value;
        UpdateHorizontal();
    }
    public void SetHorizontal(string value) => SetHorizontal(float.Parse(value));

    public void UpdateVertical()
    {
        verticalField.text =  collectableSpawn.vertical.ToString("0.00");
        verticalSlider.value = collectableSpawn.vertical;
    }
    public void SetVertical(float value)
    {
        collectableSpawn.vertical = value;
        UpdateVertical();
    }
    public void SetVertical(string value) => SetVertical(float.Parse(value));

    public void UpdateOffset()
    {
        offsetField.text =  collectableSpawn.offset.ToString("0.00");
        offsetSlider.value = collectableSpawn.offset;
    }
    public void SetOffset(float value)
    {
        collectableSpawn.offset = value;
        UpdateOffset();
    }
    public void SetOffset(string value) => SetOffset(float.Parse(value));
#endregion

#region Application
    public void UpdateMasterVol()
    {
        float value = 0f;
        audioMixer.GetFloat("masterVol", out value);
        masterField.text = value.ToString("0.0");
        masterSlider.value = value;
    }
    public void SetMasterVol(float value)
    {
        if (value < -80f) value = -80f;
        else if (value > 20f) value = 20f;
        audioMixer.SetFloat("masterVol", value);
        UpdateMasterVol();
    }
    public void SetMasterVol(string value) => SetMasterVol(float.Parse(value));

    public void MinimizeAllTabs()
    {
        waveTab.SetActive(false);
        waveTabS.SetActive(true);
        preferencesTab.SetActive(false);
        preferencesTabS.SetActive(true);
        collectableTab.SetActive(false);
        collectableTabS.SetActive(true);
        appTab.SetActive(false);
        appTabS.SetActive(true);
        yawTab.SetActive(false);
        // yawTabs.SetActive(true);
        observerTab.SetActive(false);
        // observerTabS.SetActive(true);
    }

    public void ExitApp()
    {
        StartCoroutine(ShutDown());
    }

    IEnumerator ShutDown()
    {
        Debug.Log("SHUTDOWN SEQUENCE");
        closingText.text = "Yaw to Origin...";
        Debug.Log("YAW to Origin");
        SetTransitionTime(5);
        SetLevel(-1);
        yield return new WaitForSeconds(5);
        Debug.Log("YAW Stopping");
        closingText.text = "Yaw Stopping...";
        YawControllerStop();
        yield return new WaitForSeconds(1);
        closingText.text = "Goodbye";
        Debug.Log("Goodbye");
        Application.Quit();
    }
#endregion

}
