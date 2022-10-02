namespace Oatsbarley.LD51
{
    using System;
    using NaughtyAttributes;
    using Oatsbarley.LD51.Data;
    using Oatsbarley.LD51.Interfaces;
    using TMPro;
    using UnityEngine;

    public class ResourceNode : MonoBehaviour, IProducer
    {
        [SerializeField] private Sprite unpopulatedSprite;
        [SerializeField] private Sprite populatedSprite;
        [SerializeField] private Color unpopulatedColor;
        [SerializeField] private Color populatedColor;

        [SerializeField] private SpriteRenderer backgroundSprite;
        [SerializeField] private SpriteRenderer foregroundSprite;

        [SerializeField] private TextMeshPro textComponent;

        [SerializeField] private Connector connector;

        public event Action Populated;

        private bool isExhausting = false;
        private int remainingBeforeExhaustion = 0;
        private Item resourceItem;

        public bool IsPopulated { get; private set; }

        public void Init(Item resourceItem)
        {
            this.resourceItem = resourceItem;
        }

        private void Start()
        {
            this.connector.SetProducer(this);

            this.textComponent.text = this.resourceItem.Value.ToString();
            this.foregroundSprite.sprite = this.resourceItem.Sprite;

            GameManager.Instance.Ticked += this.OnTick;

            this.Populate();
        }

        private void OnMouseUpAsButton()
        {
            if (this.IsPopulated)
            {
                return;
            }

            this.Populate();
        }

        [Button()]
        public void Populate()
        {
            // this.backgroundSprite.sprite = this.populatedSprite;
            // this.foregroundSprite.sprite = this.populatedSprite;
            this.backgroundSprite.color = this.populatedColor;

            this.connector.SetOutputs(1);

            this.IsPopulated = true;
            this.Populated?.Invoke();
        }

        [Button()]
        public void Unpopulate()
        {
            // this.backgroundSprite.sprite = this.unpopulatedSprite;
            // this.foregroundSprite.sprite = this.unpopulatedSprite;
            this.backgroundSprite.color = this.unpopulatedColor;

            this.connector.SetOutputs(0);

            this.IsPopulated = false;
        }

        [Button()]
        public void OnTick()
        {
            if (this.IsPopulated)
            {
                this.Generate();
            }
        }

        [Button()]
        public void Generate()
        {
            // todo
            Debug.Log($"Generating {this.resourceItem.Name}");

            this.connector.Send(this.resourceItem);

            if (this.isExhausting)
            {
                this.remainingBeforeExhaustion -= 1;
                if (this.remainingBeforeExhaustion == 0)
                {
                    this.Exhaust();
                }
            }
        }

        [Button()]
        public void BeginExhaustion()
        {
            if (this.isExhausting)
            {
                return;
            }

            this.isExhausting = true;
            this.remainingBeforeExhaustion = 10;
        }

        private void Exhaust()
        {
            // todo
            GameObject.Destroy(this.gameObject);
            GameManager.Instance.Ticked -= this.OnTick;
        }

        // public Item[] GetPossibleItems(IEnumerable<Item> inputs)
        // {
        //     // there aren't ever any inputs to resource nodes
        //
        //     return new[] { this.resourceItem };
        // }
    }
}