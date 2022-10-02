namespace Oatsbarley.LD51
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DG.Tweening;
    using Oatsbarley.LD51.Data;
    using Oatsbarley.LD51.Interfaces;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public enum FactoryMode
    {
        Addition,
        Division
    }

    public class FactoryNode : MonoBehaviour, IReceiver, IProducer, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private SpriteRenderer backgroundSprite;
        [SerializeField] private SpriteRenderer foregroundSprite;

        [SerializeField] private Color additionColour;
        [SerializeField] private Color divisionColour;

        [SerializeField] private TextMeshPro textComponent;
        [SerializeField] private Connector connector;

        private Recipe recipe;

        private Item input1Buffer;
        private Item input2Buffer;

        private FactoryMode mode = FactoryMode.Addition;
        // private Vector2 mouseOffset;
        // private bool isDragging;

        private void Start()
        {
            this.connector.SetReceiver(this);
            this.connector.SetProducer(this);
            GameManager.Instance.Ticked += this.OnTick;
        }

        // private void OnMouseDown()
        // {
        //     this.mouseOffset = this.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // }
        //
        // private void OnMouseDrag()
        // {
        //     this.isDragging = true;
        //
        //     var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     this.transform.position = (Vector3)(this.mouseOffset + (Vector2) mouseWorldPos) + Vector3.forward * this.transform.position.z;
        //
        //     this.connector.UpdateConnectionPositions();
        // }

        private void OnMouseUpAsButton()
        {
            // if (this.isDragging)
            // {
            //     this.isDragging = false;
            //     return;
            // }

            // this.SetMode(this.mode == FactoryMode.Addition ? FactoryMode.Division : FactoryMode.Addition);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Debug.Log("Click");
            //
            // if (eventData.button == PointerEventData.InputButton.Right)
            // {
            //     this.SetMode(this.mode == FactoryMode.Addition ? FactoryMode.Division : FactoryMode.Addition);
            // }
        }

        private void OnTick()
        {
            if (this.mode == FactoryMode.Addition && (this.input1Buffer == null || this.input2Buffer == null))
            {
                return;
            }

            if (this.mode == FactoryMode.Division && this.input1Buffer == null)
            {
                return;
            }

            Debug.Log("Ready to create item");
            this.Generate();
        }

        public void SetMode(FactoryMode mode)
        {
            this.mode = mode;

            this.input1Buffer = null;
            this.input2Buffer = null;

            switch (mode)
            {
                case FactoryMode.Addition:
                    this.textComponent.text = "+";
                    this.backgroundSprite.color = this.additionColour;
                    this.connector.SetOutputs(1);
                    this.connector.SetInputs(2);
                    break;
                case FactoryMode.Division:
                    this.textComponent.text = "÷";
                    this.backgroundSprite.color = this.divisionColour;
                    this.connector.SetOutputs(2);
                    this.connector.SetInputs(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public bool CanReceive(Item item, int inputIndex)
        {
            return true;
        }

        public void Receive(Item item, int inputIndex)
        {
            switch (this.mode)
            {
                case FactoryMode.Division:
                case FactoryMode.Addition when inputIndex == 0:
                    this.input1Buffer = item;
                    break;
                case FactoryMode.Addition:
                    this.input2Buffer = item;
                    break;
            }
        }

        // public Item[] FilterValidItems(IEnumerable<Item> items)
        // {
        //     return items.ToArray();
        // }

        private void Generate()
        {
            if (this.mode == FactoryMode.Addition)
            {
                var value = this.input1Buffer.Value + this.input2Buffer.Value;
                var item = GameManager.Instance.Items.FirstOrDefault(i => i.Value == value);

                if (item == null)
                {
                    Error("RESULT TOO LARGE");
                }
                else
                {
                    this.connector.Send(item);
                }
            }
            else if (this.mode == FactoryMode.Division)
            {
                var value = this.input1Buffer.Value;
                if (value == 1)
                {
                    Error("CAN'T DIVIDE 1 BY 2");
                }
                else
                {
                    var result = this.Divide(this.input1Buffer);
                    this.connector.Send(result[0]);
                }
            }

            this.input1Buffer = null;
            this.input2Buffer = null;
        }

        private void Error(string message)
        {
            Debug.LogError(message);
            // this.backgroundSprite.transform.DOShakeRotation(1f, 90f);
            // this.foregroundSprite.transform.DOShakeRotation(1f, 90f);
            // this.textComponent.transform.DOShakeRotation(1f, 90f);

            this.transform.DOShakeRotation(1f, 5f, randomnessMode:ShakeRandomnessMode.Harmonic);
        }

        // public Item[] GetPossibleItems(IEnumerable<Item> inputs)
        // {
        //     if (!inputs.Any())
        //     {
        //         return Array.Empty<Item>();
        //     }
        //
        //     switch (this.mode)
        //     {
        //         case FactoryMode.Addition:
        //             if (inputs.Count() != 2)
        //             {
        //                 return Array.Empty<Item>();
        //             }
        //
        //             var item1 = inputs.First();
        //             var item2 = inputs.Skip(1).First();
        //
        //             if (item1.Value + item2.Value > GameManager.Instance.Items.Max(i => i.Value))
        //             {
        //                 return Array.Empty<Item>();
        //             }
        //
        //             return new [] { this.Add(item1, item2) };
        //         case FactoryMode.Division:
        //             var item = inputs.First();
        //             if (item.Value == 1)
        //             {
        //                 return Array.Empty<Item>();
        //             }
        //
        //             return this.Divide(item);
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // }

        private Item[] Divide(Item item)
        {
            int value = item.Value;
            var remainder = value % 2;
            var value1 = value / 2;

            var output1 = GameManager.Instance.Items.First(i => i.Value == value1);
            var items = new List<Item>
            {
                output1
            };

            if (remainder == 0) // both values are the same
            {
                items.Add(output1);
            }
            else
            {
                var output2 = GameManager.Instance.Items.First(i => i.Value == remainder);
                items.Add(output2);
            }

            return items.ToArray();
        }

        private Item Add(Item item1, Item item2)
        {
            var value = item1.Value + item2.Value;
            var item = GameManager.Instance.Items.FirstOrDefault(i => i.Value == value);

            return item;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }
    }
}