using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YawVR {
    public class Rainbow : MonoBehaviour {

        Coroutine coroutine;

        Color32[] colors = new Color32[129];
        byte[] bytes = new byte[390];

        double multiplier = 10;
        ushort counter = 0;


        [SerializeField]
        private float loopDelay = 0.02f;
        private WaitForSeconds delay;
        private void Awake() {
            delay = new WaitForSeconds(loopDelay);
        }

        public void StartRainbow() {
            StopRainbow();

            coroutine = StartCoroutine(RainbowCoroutine());
        }
        public void StopRainbow() {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        IEnumerator RainbowCoroutine() {
            while (true) {
                for (int i = 0; i < colors.Length; i++) {
                    colors[i].g = (byte)(Math.Sin(X(i)) * 255);
                    colors[i].r = (byte)(-Math.Sin(X(i)) * 255);
                    colors[i].b = (byte)(-Math.Cos(X(i)) * 255);
                  
                }
                counter++;

                YawController.Instance().SendLED(colors);
                yield return delay;
            }
        }

        private double X(int i) {
            return ((multiplier * ((i) + counter)) / 129);
        }
    }
}
