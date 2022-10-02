namespace Oatsbarley.LD51
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Oatsbarley.LD51.Data;
    using Oatsbarley.LD51.Interfaces;
    using UnityEngine;

    // todo give production hints from producers, use to determine if connection is valid

    public struct Connection
    {
        public Connector Connector { get; set; }
        public int InputIndex { get; set; }
        public int OutputIndex { get; set; }

        public ConnectorLine LineRenderer { get; set; }

        public ConnectorWidget Widget { get; set; }
    }

    public class Connector : MonoBehaviour
    {
        [SerializeField] private ConnectorLine lineRendererPrefab;
        [SerializeField] private TravellingItem travellingItemPrefab;
        [SerializeField] private ConnectorWidget widgetPrefab;

        public int maxInputs;
        public int maxOutputs;

        // node that receives anything sent to this connector
        private IReceiver receiver;
        private IProducer producer;

        private List<Connection> inputs = new List<Connection>();
        private List<Connection> outputs = new List<Connection>();

        private List<ConnectorLine> lineRenderers = new List<ConnectorLine>();

        private Dictionary<Item, Connection> lastOutputUsed = new Dictionary<Item, Connection>();

        private List<ConnectorWidget> outputWidgets = new List<ConnectorWidget>();
        private List<ConnectorWidget> inputWidgets = new List<ConnectorWidget>();

        private Item[] possibleProducts;

        private ConnectorLine potentialLine;
        private bool isDrawingPotential;
        private ConnectorWidget potentialWidget;

        private void Start()
        {
            this.potentialLine = Instantiate(this.lineRendererPrefab, this.transform);
            this.potentialLine.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!this.isDrawingPotential)
            {
                return;
            }

            this.potentialLine.SetPositions(
                (Vector3) ((Vector2) this.potentialWidget.transform.position) + Vector3.forward * this.transform.position.z,
                (Vector3) ((Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition)) + Vector3.forward * this.transform.position.z);
        }

        public void SetReceiver(IReceiver receiver)
        {
            this.receiver = receiver;
        }

        public void SetProducer(IProducer producer)
        {
            this.producer = producer;
        }

        public void DrawPotential(ConnectorWidget widget)
        {
            this.isDrawingPotential = true;
            this.potentialWidget = widget;
            this.potentialLine.gameObject.SetActive(true);
        }

        public void StopDrawingPotential()
        {
            this.isDrawingPotential = false;
            this.potentialWidget = null;
            this.potentialLine.gameObject.SetActive(false);
        }

        public void SetOutputs(int outputs)
        {
            var toReturn = this.outputWidgets.ToList();
            foreach (var returnWidget in toReturn)
            {
                this.ReturnWidget(this.outputWidgets, returnWidget);
            }

            var spacing = 0.35f;

            var origin = this.transform.position;
            var height = outputs * 0.25f + (outputs - 1) * spacing;
            var minY = origin.y - (height - 0.25f) / 2f;
            var x = origin.x + 1f;

            for (var i = 0; i < outputs; i++)
            {
                var widget = this.GetWidget(this.outputWidgets);
                widget.SetIsInput(false);
                widget.transform.position = new Vector3(x, minY + i * (spacing + 0.25f), origin.z);
            }

            foreach (var output in this.outputs)
            {
                output.Connector.DisconnectAllInputs(this);
                this.ReturnLineRenderer(output.LineRenderer);
            }

            this.outputs.Clear();
        }

        public void SetInputs(int inputs)
        {
            var toReturn = this.inputWidgets.ToList();
            foreach (var returnWidget in toReturn)
            {
                this.ReturnWidget(this.inputWidgets, returnWidget);
            }

            var spacing = 0.35f;

            var origin = this.transform.position;
            var height = inputs * 0.25f + (inputs - 1) * spacing;
            var minY = origin.y - (height - 0.25f) / 2f;
            var x = origin.x - 1f;

            for (var i = 0; i < inputs; i++)
            {
                var widget = this.GetWidget(this.inputWidgets);
                widget.SetIsInput(true);
                widget.transform.position = new Vector3(x, minY + i * (spacing + 0.25f), origin.z);
            }

            foreach (var input in this.inputs)
            {
                input.Connector.DisconnectAllOutputs(this);
            }

            this.inputs.Clear();
        }

        public void UpdateConnectionPositions()
        {
            this.UpdateOutputConnectionPositions();

            foreach (var input in this.inputs)
            {
                input.Connector.UpdateOutputConnectionPosition(input.OutputIndex);
            }
        }

        public void UpdateOutputConnectionPositions()
        {
            foreach (var output in this.outputs)
            {
                output.LineRenderer.SetPositions(
                    output.Widget.transform.position,
                    output.Connector.GetInputWidgetPosition(output.InputIndex));
            }
        }

        public void UpdateOutputConnectionPosition(int index)
        {
            var output = this.outputs.First(o => o.OutputIndex == index);
            output.LineRenderer.SetPositions(
                output.Widget.transform.position,
                output.Connector.GetInputWidgetPosition(output.InputIndex));
        }

        public void DisconnectAllInputs(Connector other)
        {
            var inputsToRemove = this.inputs.Where(i => i.Connector == other).ToList();
            foreach (var input in inputsToRemove)
            {
                this.inputs.Remove(input);
            }
        }

        public void DisconnectAllOutputs(Connector other)
        {
            var outputsToRemove = this.outputs.Where(o => o.Connector == other).ToList();

            foreach (var output in outputsToRemove)
            {
                this.outputs.Remove(output);
                this.ReturnLineRenderer(output.LineRenderer);
            }
        }

        private ConnectorWidget GetWidget(List<ConnectorWidget> list)
        {
            var widget = Instantiate(this.widgetPrefab, this.transform);
            widget.Connector = this;

            list.Add(widget);
            return widget;
        }

        private void ReturnWidget(List<ConnectorWidget> list, ConnectorWidget widget)
        {
            GameObject.Destroy(widget.gameObject);
            list.Remove(widget);
        }

        public bool ConnectTo(ConnectorWidget ownWidget, ConnectorWidget widget)
        {
            if (this.outputs.Count >= this.maxOutputs)
            {
                Debug.LogError($"This node cannot connect any more outputs.", this);
                return false;
            }

            var other = widget.Connector;
            if (other == this)
            {
                Debug.LogError("Can't connect to itself.", this);
                return false;
            }

            var outputIndex = this.outputWidgets.IndexOf(ownWidget);
            if (outputIndex == -1)
            {
                return false;
            }

            if (!other.TryConnectFrom(this, widget, outputIndex, out int inputIndex))
            {
                return false;
            }

            // make connection
            var lineRenderer = this.GetLineRenderer();
            lineRenderer.Connect(ownWidget, widget, null);

            this.outputs.Add(new Connection
            {
                Connector = other,
                InputIndex = inputIndex,
                OutputIndex = outputIndex,
                LineRenderer = lineRenderer,
                Widget = ownWidget
            });

            return true;
        }

        private bool TryConnectFrom(Connector other, ConnectorWidget widget, int outputIndex, out int inputIndex)
        {
            if (this.inputs.Count >= this.maxInputs)
            {
                Debug.LogError($"This node cannot receive any more inputs.", this);
                inputIndex = -1;
                return false;
            }

            inputIndex = this.inputWidgets.IndexOf(widget);

            // make connection
            this.inputs.Add(new Connection
            {
                Connector = other,
                InputIndex = inputIndex,
                OutputIndex = outputIndex,
                Widget = widget
            });

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

            var validOutputs = this.outputs.Where(o => o.Connector.CanReceive(item, o)).ToArray();
            int index = 0;
            if (this.lastOutputUsed.TryGetValue(item, out Connection lastUsed))
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
            var outputWidget = this.outputWidgets[output.OutputIndex];

            this.lastOutputUsed[item] = output;

            var travellingItem = Instantiate(this.travellingItemPrefab);
            travellingItem.Travel(item, outputWidget.transform.position, output.Connector.GetInputWidgetPosition(output.InputIndex), () =>
            {
                output.Connector.Receive(item, output);
            });

            return true;
        }

        public void Receive(Item item, Connection connection)
        {
            if (!this.CanReceive(item, connection))
            {
                Debug.LogError("Hit Receive even though CanReceive is null", this);
            }

            if (this.receiver == null)
            {
                Debug.LogError("Receiver is null", this);
                return;
            }

            this.receiver.Receive(item, connection.InputIndex);
        }

        public bool CanReceive(Item item, Connection connection)
        {
            return this.receiver.CanReceive(item, connection.InputIndex);
        }

        public Vector2 GetInputWidgetPosition(int index)
        {
            return this.inputWidgets[index].transform.position;
        }

        // public Item[] GetPossibleItems()
        // {
        //     throw new NotSupportedException();
        //
        //     // return this.producer.GetPossibleItems()
        // }
        //
        // public Item[] FilterValidItems(Item[] items)
        // {
        //     throw new NotSupportedException();
        // }
    }
}