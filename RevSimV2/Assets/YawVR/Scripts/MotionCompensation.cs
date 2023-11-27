using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace YawVR {
    /// <summary>
    /// Cancels the camera's rotation based on IMU data
    /// </summary>
    public class MotionCompensation : MonoBehaviour 
    {
        [Header("Controllers")]
        [SerializeField] private YawController yawController = null;
        [SerializeField] private Transform leftController = null;
        [SerializeField] private Transform rightController = null;

        [Header("Select Controller")]
        [SerializeField]
        private enumYawPitchRollDevice device = enumYawPitchRollDevice.YawVRController;

        [Header("Offset (Camera)")]
        [SerializeField] public bool IsApplyToOffset = true;
        [SerializeField] private Transform cameraOffsetTransform = null;
        [SerializeField] [Range(0f, 0.9f)] private float smoothing = 0.7f;

        public enumYawPitchRollDevice GetDevice() 
        {
            return device;
        }

        public Transform GetOpenXRControllerTransform() 
        {
            if (device == enumYawPitchRollDevice.LeftController) { return leftController; }
            else if (device == enumYawPitchRollDevice.RightController) { return rightController; }
            return null;
        }

        public enum enumYawPitchRollDevice 
        {
            YawVRController,
            LeftController,
            RightController
        }

        private Vector3 m_v3BaseToHead;
        private Quaternion quat1 = new Quaternion();
        private Quaternion quat2 = new Quaternion();
        private Vector3 v3ToHead = new Vector3();

        public void SetHead(Vector3 v3Head) 
        {
            m_v3BaseToHead = new Vector3(v3Head.x, v3Head.y, v3Head.z);
        }

        public Vector3 GetHead() 
        {
            Vector3 ret = new Vector3(v3ToHead.x, v3ToHead.y, v3ToHead.z);
            return ret;
        }

        public void SetRotate(Quaternion quat) 
        {
            this.quat1 = new Quaternion(quat.x, quat.y, quat.z, quat.w);
        }

        public Quaternion GetRotate() 
        {
            Quaternion ret = new Quaternion(quat1.x, quat1.y, quat1.z, quat1.w);
            return ret;
        }

        private void Start()
        {
            if (null != cameraOffsetTransform) 
            {
                SetRotate(cameraOffsetTransform.rotation);
                SetHead(cameraOffsetTransform.localPosition);
            }
        }

        private void FixedUpdate() 
        {
            float yaw = 0.0f;
            float pitch = 0.0f;
            float roll = 0.0f;
            if (device == enumYawPitchRollDevice.YawVRController)
            {
                if (YawController.Instance().State == ControllerState.Started || YawController.Instance().State == ControllerState.Connected) 
                {
                    yaw = -yawController.Device.ActualPosition.yaw;
                    pitch = yawController.Device.ActualPosition.pitch;
                    roll = yawController.Device.ActualPosition.roll;
                }
            }
            else if (device == enumYawPitchRollDevice.LeftController) 
            {
                yaw = -leftController.localEulerAngles.y;
                pitch = -leftController.localEulerAngles.x;
                roll = -leftController.localEulerAngles.z;
            }
            else if (device == enumYawPitchRollDevice.RightController)
            {
                yaw = -rightController.localEulerAngles.y;
                pitch = -rightController.localEulerAngles.x;
                roll = -rightController.localEulerAngles.z;
            }

            quat1 = Quaternion.Slerp(quat1, Quaternion.Euler(new Vector3(pitch, yaw, roll)), 1f - smoothing);
            quat2 = Quaternion.Slerp(quat2, Quaternion.Euler(new Vector3(-pitch, -yaw, -roll)), 1f - smoothing);
            v3ToHead = quat2 * m_v3BaseToHead;

            if (cameraOffsetTransform != null && true == IsApplyToOffset)
            {
                cameraOffsetTransform.localRotation = quat1;
                cameraOffsetTransform.localPosition = v3ToHead;
            }
        }
    }
}
