namespace Oatsbarley.LD51
{
    using System;
    using System.Collections;
    using TMPro;
    using UnityEngine;

    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration;
        [SerializeField] private CanvasGroup frontCover;
        [SerializeField] private CanvasGroup backCover;

        [SerializeField] private CanvasGroup endGameBack;
        [SerializeField] private CanvasGroup endGameGroup;

        [SerializeField] private TextMeshProUGUI endGameTitleText;
        [SerializeField] private TextMeshProUGUI endGameReasonText;
        [SerializeField] private TextMeshProUGUI endGameFunText;

        private string defaultEndGameFunText;

        private void Awake()
        {
            this.defaultEndGameFunText = this.endGameFunText.text;
        }

        private IEnumerator Start()
        {
            this.canvasGroup.alpha = 1;
            this.frontCover.alpha = 1;
            this.backCover.alpha = 1;

            yield return new WaitForSeconds(1);
            yield return this.StartCoroutine(this.Fade(this.frontCover, 0, this.fadeDuration));
            yield return new WaitForSeconds(1);
            this.StartCoroutine(this.Fade(this.backCover, 0, this.fadeDuration));

            this.endGameBack.alpha = 0;
            this.endGameGroup.alpha = 0;
        }

        public void Show(Action onFinished)
        {
            this.StartCoroutine(this.RunShow(onFinished));
        }

        private IEnumerator RunShow(Action onFinished)
        {
            // intentionally doing this at the same time and not yielding
            this.StartCoroutine(this.Fade(this.endGameBack, 0f, this.fadeDuration));
            this.StartCoroutine(this.Fade(this.endGameGroup, 0f, this.fadeDuration));
            this.endGameGroup.interactable = false;
            this.endGameGroup.blocksRaycasts = false;

            yield return this.StartCoroutine(this.Fade(this.backCover, 1f, this.fadeDuration));
            yield return new WaitForSecondsRealtime(1);
            yield return this.StartCoroutine(this.Fade(this.canvasGroup, 1f, this.fadeDuration));
            onFinished?.Invoke();
        }

        public void ShowFrontCover(Action onFinished)
        {
            this.StartCoroutine(this.Fade(this.frontCover, 1f, 0.8f, onFinished));
        }

        public void ShowEndGame(Action onFinished, string reason, bool actuallyWon = false)
        {
            this.endGameReasonText.text = reason;
            this.endGameTitleText.text = actuallyWon ? "GOOD JOB" : "OH WELL";
            this.endGameFunText.text = actuallyWon ? "Thanks for playing!" : this.defaultEndGameFunText;
            this.StartCoroutine(this.EndGame(onFinished));
        }

        private IEnumerator EndGame(Action onFinished)
        {
            yield return this.StartCoroutine(this.Fade(this.endGameBack, 1f, 0.8f));
            yield return new WaitForSecondsRealtime(1);
            yield return this.StartCoroutine(this.Fade(this.endGameGroup, 1f, 0.8f, onFinished));

            this.endGameGroup.interactable = true;
            this.endGameGroup.blocksRaycasts = true;
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