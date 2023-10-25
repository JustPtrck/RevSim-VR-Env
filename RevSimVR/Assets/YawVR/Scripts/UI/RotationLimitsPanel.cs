using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



namespace YawVR {


    /// <summary>
    /// UI class that handles the RotationLimits panel
    /// </summary>
    public class RotationLimitsPanel : MonoBehaviour {

        [SerializeField]
        private InputField yaw;
        [SerializeField]
        private InputField pitch;
        [SerializeField]
        private InputField roll;


        private YawController controller;
        private void Start() {
            controller = YawController.Instance();
        }

        public void OnChange() {
            try {
                controller.SetTiltLimits(
                    float.Parse(yaw.text),
                    float.Parse(pitch.text),
                    float.Parse(roll.text));


            }
            catch {
                Debug.LogError("rotation limits error!");
            }
        }

    }
}
