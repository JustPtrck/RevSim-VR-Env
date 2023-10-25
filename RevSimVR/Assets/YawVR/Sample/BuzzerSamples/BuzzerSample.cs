using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace YawVR {
    public class BuzzerSample : MonoBehaviour {
        [SerializeField]
        private Buzzer buzzerData;
     

        public void OneShot(float time) {
            GetBuzzer().SetBuzzerAmps(buzzerData.right_amp, buzzerData.center_amp, buzzerData.left_amp);
            GetBuzzer().SetHz(buzzerData.hz);
            GetBuzzer().isOn = true;

            Invoke("DisableBuzzer", time);


        }

        private void DisableBuzzer() {
          //  Debug.Log("disabled");
            GetBuzzer().isOn = false;
        }

        private Buzzer GetBuzzer() {
            return YawController.Instance().Buzzer;
        }
    }
}
