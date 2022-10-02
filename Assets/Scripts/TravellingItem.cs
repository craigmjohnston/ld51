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

        private Vector3 velocity;

        private void Update()
        {
            var t = (Time.time - this.startTime) / (this.distance / GameManager.Instance.ItemTravelSpeed);
            this.transform.position = Vector3.SmoothDamp(this.transform.position, this.end, ref this.velocity, 0.7f);

            if (Vector3.Distance(this.transform.position, this.end) < 0.05f)
            {
                this.onArrive?.Invoke();
                GameObject.Destroy(this.gameObject);
            }
        }

        public void Travel(Item item, Vector3 from, Vector3 to, Action onArrive)
        {
            this.item = item;

            this.startTime = Time.time;
            this.distance = Vector3.Distance(from, to);
            this.transform.position = from;

            this.start = from;
            this.end = to;

            this.onArrive = onArrive;

            this.spriteRenderer.sprite = item.Sprite;
            this.spriteRenderer.color = item.Color;

            AudioManager.PlayOnce("hit");
        }
    }
}