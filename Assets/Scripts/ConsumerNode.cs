namespace Oatsbarley.LD51
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Oatsbarley.LD51.Data;
    using Oatsbarley.LD51.Interfaces;
    using TMPro;
    using UnityEngine;

    public class ConsumerNode : MonoBehaviour, IReceiver
    {
        [SerializeField] private SpriteRenderer backgroundSprite;
        [SerializeField] private SpriteRenderer foregroundSprite;

        [SerializeField] private TextMeshPro textComponent;

        // [SerializeField] private Item neededItem;
        [SerializeField] private Connector connector;

        [SerializeField] private GameObject countdownGameObject;
        [SerializeField] private TextMeshPro countdownTextComponent;
        [SerializeField] private float receiveTimeout;
        [SerializeField] private float countdownDuration;

        private float lastReceivedTime;

        private Item neededItem;

        public void Init(Item neededItem)
        {
            this.neededItem = neededItem;
        }

        private void Start()
        {
            this.connector.SetReceiver(this);
            this.connector.SetInputs(1);

            this.textComponent.text = this.neededItem.Value.ToString();
            this.backgroundSprite.sprite = this.neededItem.Sprite;
            this.foregroundSprite.sprite = this.neededItem.Sprite;

            this.lastReceivedTime = Time.time;
            this.countdownGameObject.SetActive(false);
        }

        private void Update()
        {
            if (!GameManager.Instance.IsPlaying)
            {
                return;
            }

            // countdown

            if (!this.countdownGameObject.activeSelf)
            {
                if (Time.time - this.lastReceivedTime >= (this.receiveTimeout - this.countdownDuration))
                {
                    this.countdownGameObject.SetActive(true);
                }
            }
            else
            {
                if (Time.time - this.lastReceivedTime < (this.receiveTimeout - this.countdownDuration))
                {
                    this.countdownGameObject.SetActive(false);
                }
                else
                {
                    var countdown = Mathf.CeilToInt(this.receiveTimeout - (Time.time - this.lastReceivedTime));
                    this.countdownTextComponent.text = Mathf.Max(countdown, 0).ToString();

                    if (countdown <= 0)
                    {
                        GameManager.Instance.LostGame("One of your customers got too impatient.");
                    }
                }
            }
        }

        public bool CanReceive(Item item, int inputIndex)
        {
            if (item != this.neededItem)
            {
                return false;
            }

            return true;
        }

        public void Receive(Item item, int inputIndex)
        {
            Debug.Log($"Received {item.Name}");

            this.lastReceivedTime = Time.time;
            if (this.countdownGameObject.activeSelf)
            {
                this.countdownGameObject.SetActive(false);
            }
        }

        // public Item[] FilterValidItems(IEnumerable<Item> items)
        // {
        //     return items.Where(i => i == this.neededItem).ToArray();
        // }
    }
}