namespace Oatsbarley.LD51
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Oatsbarley.LD51.Data;
    using Oatsbarley.LD51.Interfaces;
    using UnityEngine;

    // todo give production hints from producers, use to determine if connection is valid

    public class Connector : MonoBehaviour
    {
        public int maxInputs;
        public int maxOutputs;

        // node that receives anything sent to this connector
        private IReceiver receiver;

        [SerializeField] private ConnectorLine lineRendererPrefab;
        [SerializeField] private TravellingItem travellingItemPrefab;

        private List<Connector> inputs = new List<Connector>();
        private List<Connector> outputs = new List<Connector>();

        private List<ConnectorLine> lineRenderers = new List<ConnectorLine>();

        private static Connector currentConnector;
        private bool isDragging = false;

        private Dictionary<Item, Connector> lastOutputUsed = new Dictionary<Item, Connector>();

        public void SetReceiver(IReceiver receiver)
        {
            this.receiver = receiver;
        }

        private void OnMouseEnter()
        {
            Connector.currentConnector = this;
        }

        private void OnMouseDown()
        {
            this.isDragging = true;
        }

        private void OnMouseUp()
        {
            this.isDragging = false;
            if (Connector.currentConnector == this)
            {
                return;
            }

            if (this.ConnectTo(Connector.currentConnector))
            {
                Debug.Log("Connection made.");
            }
        }

        private bool ConnectTo(Connector other)
        {
            if (this.outputs.Count >= this.maxOutputs)
            {
                Debug.LogError($"This node cannot connect any more outputs.", this);
                return false;
            }

            if (this.outputs.Contains(other))
            {
                Debug.LogError("This node is already connected to that node.", this);
                return false;
            }

            if (!other.ConnectFrom(this))
            {
                return false;
            }

            this.outputs.Add(other);

            var lineRenderer = this.GetLineRenderer();
            lineRenderer.Connect(this, other, null);

            return true;
        }

        public bool ConnectFrom(Connector other)
        {
            if (this.inputs.Count >= this.maxInputs)
            {
                Debug.LogError($"This node cannot receive any more inputs.", this);
                return false;
            }

            this.inputs.Add(other);

            return true;
        }

        private ConnectorLine GetLineRenderer()
        {
            var renderer = Instantiate(this.lineRendererPrefab, this.transform);
            this.lineRenderers.Add(renderer);
            return renderer;
        }

        private void ReturnLineRenderer(ConnectorLine renderer)
        {
            this.lineRenderers.Remove(renderer);
            GameObject.Destroy(renderer.gameObject);
        }

        public bool Send(Item item)
        {
            if (!this.outputs.Any())
            {
                return false;
            }

            var validOutputs = this.outputs.Where(o => o.CanReceive(item)).ToArray();
            int index = 0;
            if (this.lastOutputUsed.TryGetValue(item, out Connector lastUsed))
            {
                index = Array.IndexOf(validOutputs, lastUsed);
                if (index == -1 || index == validOutputs.Length - 1)
                {
                    index = 0;
                }
                else
                {
                    index += 1;
                }
            }

            var output = validOutputs[index];
            this.lastOutputUsed[item] = output;

            var travellingItem = Instantiate(this.travellingItemPrefab);
            travellingItem.Travel(item, this.transform.position, output.transform.position, () =>
            {
                output.Receive(item, this);
            });

            return true;
        }

        public void Receive(Item item, Connector sender)
        {
            if (this.receiver == null)
            {
                Debug.LogError("Receiver is null", this);
                return;
            }

            this.receiver.Receive(item);

            // todo check can receive
        }

        public bool CanReceive(Item item)
        {
            return this.receiver.CanReceive(item);
        }
    }
}