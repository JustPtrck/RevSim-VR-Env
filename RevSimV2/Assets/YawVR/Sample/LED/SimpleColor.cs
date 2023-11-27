using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace YawVR {
    public class SimpleColor : MonoBehaviour {
        [SerializeField]
        private Color32 color;
        public void SetSimpleColor(Color c) {
            YawController.Instance().SendLED(c);
        }
        public void SetSimpleColor() {
            YawController.Instance().SendLED(color);
        }
    }
}
