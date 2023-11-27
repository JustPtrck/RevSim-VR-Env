using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YawVR {
    public class ColorTransition : MonoBehaviour {
        [SerializeField]
        private float time = 1f;

      

        [SerializeField]
        private Color[] colors;


        private WaitForSeconds delay;
        private Coroutine cor;

        private void Awake() {
            delay = new WaitForSeconds(time);
        }

        public void StopEffect() {
            if (cor != null) StopCoroutine(cor);
        }
        public void StartEffect() {
            StopEffect();

            cor = StartCoroutine(EffectCoroutine());
        }

        private IEnumerator EffectCoroutine() {

            //float effectT = 0;
            float t = 0;
            int index = 0;
            int nextIndex;
            nextIndex = index + 1;
            while (true) {
              
                Color toSet = Color.Lerp(colors[index], colors[nextIndex], t / time);
                YawController.Instance().SendLED(toSet);
                t += Time.deltaTime;
                if (t >= time) {               
                    t = 0;
                    index = nextIndex;
                    nextIndex = index + 1;
                    if (nextIndex >= colors.Length) nextIndex = 0;
                }
                yield return null;
            }
        }
    }
}
