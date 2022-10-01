namespace Oatsbarley.LD51
{
    using UnityEngine;
    using UnityEngine.Events;

    public class DoubleClickTrigger : MonoBehaviour
    {
        [SerializeField] private float doubleClickInterval;
        [SerializeField] private UnityEvent handler;

        private float lastMouseUp = -1000;

        private void OnMouseUpAsButton()
        {
            if (Time.realtimeSinceStartup - this.lastMouseUp <= this.doubleClickInterval)
            {
                // double clicked
                this.OnDoubleClick();
                return;
            }

            this.lastMouseUp = Time.realtimeSinceStartup;
        }

        private void OnDoubleClick()
        {
            this.handler.Invoke();
        }
    }
}