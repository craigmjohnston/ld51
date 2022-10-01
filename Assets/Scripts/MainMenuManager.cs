namespace Oatsbarley.LD51
{
    using System;
    using System.Collections;
    using UnityEngine;

    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration;
        [SerializeField] private CanvasGroup frontCover;
        [SerializeField] private CanvasGroup backCover;

        private void Awake()
        {
            this.canvasGroup.alpha = 1;
            this.frontCover.alpha = 1;
            this.backCover.alpha = 1;
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1);
            yield return this.StartCoroutine(this.Fade(this.frontCover, 0, this.fadeDuration));
            yield return new WaitForSeconds(1);
            this.StartCoroutine(this.Fade(this.backCover, 0, this.fadeDuration));
        }

        public void Hide()
        {
            this.StartCoroutine(this.Fade(this.canvasGroup, 0, this.fadeDuration));
        }

        private IEnumerator Fade(CanvasGroup canvasGroup, float to, float duration, Action onFinished = null)
        {
            float initial = canvasGroup.alpha;
            float start = Time.realtimeSinceStartup;
            while ((Time.realtimeSinceStartup - start) < duration)
            {
                yield return new WaitForEndOfFrame();
                float t = (Time.realtimeSinceStartup - start) / this.fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(initial, to, t);
            }

            canvasGroup.alpha = to;
            onFinished?.Invoke();
        }
    }
}