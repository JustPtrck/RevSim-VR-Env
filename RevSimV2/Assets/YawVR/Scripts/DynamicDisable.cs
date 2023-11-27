using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YawVR {

    public class DynamicDisable : MonoBehaviour {
        [SerializeField]
        private bool yaw1;
        [SerializeField]
        private bool yaw2;


        void Start() {
            //Debug.Log(string.Format("{0} - Registering for OnConnected event", gameObject.name));
            YawController.OnConnectReceivers.Add(OnConnected);

        }
        void OnEnable() {
            //OnConnected();
        }
        private void OnConnected() {
            if (YawController.Instance() != null) {
                switch (YawController.Instance().Device.type) {
                    case DeviceType.YAW1:
                        gameObject.SetActive(yaw1);
                        break;
                    case DeviceType.YAW2:
                        gameObject.SetActive(yaw2);
                        break;


                }
            }
        }
    }
}
