namespace Oatsbarley.LD51
{
    using Oatsbarley.LD51.Data;
    using TMPro;
    using UnityEngine;

    public class ConnectorWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textComponent;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color inputColour;
        [SerializeField] private Color outputColour;

        private bool isInput;
        private bool isDragging = false;

        public Connector Connector { get; set; }

        private static ConnectorWidget currentConnector;

        public void SetIsInput(bool isInput)
        {
            this.isInput = isInput;
            this.spriteRenderer.color = isInput ? this.inputColour : this.outputColour;
        }

        public void SetItem(Item item)
        {
            this.textComponent.text = $"{item.Value}";
        }

        private void OnMouseEnter()
        {
            ConnectorWidget.currentConnector = this;
        }

        private void OnMouseDown()
        {
            this.isDragging = true;
            this.Connector.DrawPotential(this);
        }

        private void OnMouseUp()
        {
            this.Connector.StopDrawingPotential();
            this.isDragging = false;

            if (ConnectorWidget.currentConnector == this)
            {
                return;
            }

            if (this.Connector.ConnectTo(this, ConnectorWidget.currentConnector))
            {
                Debug.Log("Connection made.");
            }
        }
    }
}