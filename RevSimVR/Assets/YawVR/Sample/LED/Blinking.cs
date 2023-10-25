using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YawVR {
    public class Blinking : MonoBehaviour {
        [SerializeField]
        private Color color;


        private Coroutine cor;

        [SerializeField]
        private float blinkDelay = 1f;

        private WaitForSeconds delay;


        private void Awake() {
            delay = new WaitForSeconds(blinkDelay);
        }

        public void StartBlinking() {

            StopBlinking();
            cor = StartCoroutine(BlinkingCoroutine());
        }
        public void StopBlinking() {
            if (cor != null) StopCoroutine(cor);
        }
        private IEnumerator BlinkingCoroutine() {
            bool b = false;
            while (true) {
                Color toSet = b ? color : Color.black;

                b = !b;

                YawController.Instance().SendLED(toSet);
                yield return delay;
            }
        }
    }
}
