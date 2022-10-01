namespace Oatsbarley.LD51
{
    using System;
    using Oatsbarley.LD51.Data;
    using UnityEngine;

    public class TravellingItem : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Item item;
        private Vector3 start;
        private Vector3 end;
        private float distance;
        private float startTime;
        private Action onArrive;

        private void Update()
        {
            var t = (Time.time - this.startTime) / (this.distance / GameManager.Instance.ItemTravelSpeed);
            this.transform.position = Vector3.Lerp(this.start, this.end, t);

            if (t >= 1)
            {
                this.onArrive?.Invoke();
                GameObject.Destroy(this.gameObject);
            }
        }

        public void Travel(Item item, Vector3 from, Vector3 to, Action onArrive)
        {
            this.item = item;
            // todo this.spriteRenderer.sprite = null;

            this.startTime = Time.time;
            this.distance = Vector3.Distance(from, to);
            this.transform.position = from;

            this.start = from;
            this.end = to;

            this.onArrive = onArrive;
        }
    }
}