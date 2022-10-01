namespace Oatsbarley.LD51
{
    using System.Collections;
    using UnityEngine;

    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration;

        public void Hide()
        {
            this.StartCoroutine(this.HideCoroutine());
        }

        private IEnumerator HideCoroutine()
        {
            float start = Time.realtimeSinceStartup;
            while ((Time.realtimeSinceStartup - start) < this.fadeDuration)
            {
                yield return new WaitForEndOfFrame();
                this.canvasGroup.alpha = 1 - ((Time.realtimeSinceStartup - start) / this.fadeDuration);
            }

            this.canvasGroup.alpha = 0;
        }
    }
}