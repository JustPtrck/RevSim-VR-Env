using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace YawVR {

    /// <summary>
    /// YawTracker class. The gameobject's orientation is sent to the simulator
    /// </summary>
    public class YawTracker : MonoBehaviour {


        private YawController yawController;

        private void Awake() {
            yawController = transform.parent.GetComponent<YawController>();
        }

        /// <summary>
        /// Sets the YawTracker's orientation, multiplier and limits are applied here
        /// </summary>
        public void SetRotation(Vector3 rot) {

            rot = SignedVector(rot);
            rot = ApplyMultipliers(rot);
            rot = ApplyLimits(rot);
           
            transform.eulerAngles = rot;
        }

        private Vector3 ApplyMultipliers(Vector3 rot) {
            rot.x = rot.x * yawController.RotationMultiplier.x;
            rot.y = rot.y * yawController.RotationMultiplier.y;
            rot.z = rot.z * yawController.RotationMultiplier.z;
            return rot;
        }
        private Vector3 ApplyLimits(Vector3 rot) {
            float pitchLimit = yawController.Limits.pitch;
            if (pitchLimit != -1)  rot.x = Mathf.Clamp(rot.x, -pitchLimit, pitchLimit);

            float yawLimit = yawController.Limits.yaw;
            if (yawLimit != -1) rot.y = Mathf.Clamp(rot.y, -yawLimit, yawLimit);

            float rollLimit = yawController.Limits.roll;
            if (rollLimit != -1) rot.z = Mathf.Clamp(rot.z, -rollLimit, rollLimit);
            return rot;
        }
        private Vector3 SignedVector(Vector3 v) {

            if (v.x >= 180) v.x -= 360;
            if (v.y >= 180) v.y -= 360;
            if (v.z >= 180) v.z -= 360;
            

            return v;
        }
    }
}
