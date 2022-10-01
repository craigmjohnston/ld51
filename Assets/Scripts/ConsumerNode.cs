namespace Oatsbarley.LD51
{
    using System;
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
            this.textComponent.text = this.neededItem.Name.ToUpper();

            this.lastReceivedTime = Time.realtimeSinceStartup;
            this.countdownGameObject.SetActive(false);
        }

        private void Update()
        {
            if (!this.countdownGameObject.activeSelf)
            {
                if (Time.realtimeSinceStartup - this.lastReceivedTime >= (this.receiveTimeout - this.countdownDuration))
                {
                    this.countdownGameObject.SetActive(true);
                }
            }
            else
            {
                if (Time.realtimeSinceStartup - this.lastReceivedTime < (this.receiveTimeout - this.countdownDuration))
                {
                    this.countdownGameObject.SetActive(false);
                }
                else
                {
                    var countdown = Mathf.CeilToInt(this.receiveTimeout - (Time.realtimeSinceStartup - this.lastReceivedTime));
                    this.countdownTextComponent.text = Mathf.Max(countdown, 0).ToString();

                    if (countdown <= 0)
                    {
                        Debug.LogError("GAME LOST");
                    }
                }
            }
        }

        public bool CanReceive(Item item)
        {
            if (item != this.neededItem)
            {
                return false;
            }

            return true;
        }

        public void Receive(Item item)
        {
            Debug.Log($"Received {item.Name}");

            this.lastReceivedTime = Time.realtimeSinceStartup;
            if (this.countdownGameObject.activeSelf)
            {
                this.countdownGameObject.SetActive(false);
            }
        }
    }
}