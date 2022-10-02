namespace Oatsbarley.LD51
{
    using System.Linq;
    using Oatsbarley.LD51.Data;
    using TMPro;
    using UnityEngine;

    public class ConnectorLine : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private TextMeshPro textComponent;
        [SerializeField] private float textAngleOffset;

        public void Connect(ConnectorWidget from, ConnectorWidget to, Item item)
        {
            var fromPosition = from.transform.position;
            var toPosition = to.transform.position;

            this.lineRenderer.SetPositions(new []
            {
                this.transform.InverseTransformPoint(fromPosition),
                this.transform.InverseTransformPoint(toPosition)
            });

            var angle = Mathf.Rad2Deg * (Mathf.Atan2(toPosition.y - fromPosition.y, toPosition.x - fromPosition.x));
            if (angle < 0)
            {
                angle += 180;
            }

            this.textComponent.transform.localRotation = Quaternion.Euler(0, 0, angle - this.textAngleOffset);
            this.textComponent.transform.position = fromPosition - (fromPosition - toPosition) / 2f;

            this.textComponent.transform.Translate(Vector3.up * 0.15f, Space.Self);

            this.textComponent.text = "Iron";// item.Name;
        }

        public void SetPositions(params Vector3[] positions)
        {
            this.lineRenderer.SetPositions(positions.Select(p => this.transform.InverseTransformPoint(p)).ToArray());
        }
    }
}