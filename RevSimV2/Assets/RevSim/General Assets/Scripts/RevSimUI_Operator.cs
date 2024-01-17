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

public class RevSimUI_Operator : MonoBehaviour
{
    [Header("Navigation Bar Setup")]
    [SerializeField] private CanvasGroup UITransparancy;
    [SerializeField] private Button revSimLogoButton;
    [SerializeField] private Button tab1Button, tab2Button, tab3Button;
    [SerializeField] private GameObject appTab, tab1, tab2, tab3; 

    [Header("Application Panel")]
    [SerializeField] private RevSimUI_Yaw yawControllerUI;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider UIAlphaSlider, masterSlider;
    [SerializeField] private InputField UIAlphaField, masterField;
    [SerializeField] private GameObject exitAppPanel; 
    [SerializeField] private Text exitAppText;

    [Header("Yaw Tracker Panel")]
    [SerializeField] private GameObject yawTrackerPanel;
    [SerializeField] private Transform yawTrackerTransform;
    [SerializeField] private Text yaw, pitch, roll;

    [Header("VR Settings")]
    [SerializeField] private Camera camera; 
    [SerializeField] private Light lightSource; 
    [SerializeField] private Volume volume;
    private Vignette vg;
    [SerializeField] private CameraOffset VROffset;
    [SerializeField] private Slider FOVSlider, vignetteSlider, brightnessSlider, heightSlider;
    [SerializeField] private InputField FOVField, vignetteField, brightnessField, heightField; 

    [Header("Info Bar")]
    [SerializeField] private Text buildVersionText;


    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGet(out vg);
        UpdateAllUIValues();
        buildVersionText.text = Application.productName + "-v" + Application.version + "-UE" + Application.unityVersion;
    }

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

    public void UpdateAllUIValues()
    {
        UpdateUITrans();
        UpdateMasterVol();
        UpdateBrightness();
        UpdateFOV();
        UpdateHeight();
        UpdateVignette();
    }


#region UI Methods

    public enum Tabs{
        Application,
        tab1,
        tab2,
        tab3
    };
    /// <summary>
    /// Toggles settings tabs active.
    /// Only one tab can be opened simultaniously
    /// </summary>
    /// <param name="tab">Select which tab to toggle</param>
    public void ToggleTab(int tab)
    {
        switch(tab)
        {
            case -1:
                appTab.SetActive(false);
                tab1.SetActive(false);
                tab2.SetActive(false);
                tab3.SetActive(false);
                break;
            case 0:
                if (!appTab.activeInHierarchy)
                    appTab.SetActive(true);
                else
                    appTab.SetActive(false);
                tab1.SetActive(false);
                tab2.SetActive(false);
                tab3.SetActive(false);
                break;
            case 1:
                if (!tab1.activeInHierarchy)
                    tab1.SetActive(true);
                else
                    tab1.SetActive(false);
                appTab.SetActive(false);
                tab2.SetActive(false);
                tab3.SetActive(false);
                break;
            case 2:
                if (!tab2.activeInHierarchy)
                    tab2.SetActive(true);
                else
                    tab2.SetActive(false);
                appTab.SetActive(false);
                tab1.SetActive(false);
                tab3.SetActive(false);
                break;
            case 3:
                if (!tab3.activeInHierarchy)
                    tab3.SetActive(true);
                else
                    tab3.SetActive(false);
                appTab.SetActive(false);
                tab1.SetActive(false);
                tab2.SetActive(false);
                break;
        }
    }
    public void ExitApp()
    {
        exitAppPanel.SetActive(true);
        StartCoroutine(ShutDown());
    }
    IEnumerator ShutDown()
    {
        Debug.Log("SHUTDOWN SEQUENCE");
        exitAppText.text = "Yaw to Origin...";
        Debug.Log("YAW to Origin");
        yield return new WaitForSeconds(5);
        Debug.Log("YAW Stopping");
        exitAppText.text = "Yaw Stopping...";
        yawControllerUI.YawUI_StopDevice();
        yield return new WaitForSeconds(1);
        exitAppText.text = "Goodbye";
        Debug.Log("Goodbye");
        Application.Quit();
    }

#endregion


    public void UpdateUITrans()
    {
        UIAlphaField.text =  UITransparancy.alpha.ToString("0.00");
        UIAlphaSlider.value = UITransparancy.alpha;
    }
    public void SetUITrans(float value)
    {
        if (value <= 0)
            value = 0.001f;
        UITransparancy.alpha = value;
        UpdateUITrans();
    }
    public void SetUITrans(string value) => SetUITrans(float.Parse(value));

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

}
