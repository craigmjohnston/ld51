namespace Oatsbarley.LD51
{
    using System;
    using Oatsbarley.LD51.Data;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class SpawnToolbarItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image backgroundSprite;
        [SerializeField] private Image foregroundSprite;
        [SerializeField] private TextMeshProUGUI textComponent;

        private bool isDragging = false;
        private Vector2 dragStart;
        private SpawnToolbar toolbar;

        private string factoryAdditionColour = "#7CCC5E";
        private string factoryDivisionColour = "#E00E2A";

        public LevelNode Node { get; private set; }

        public void SetNode(LevelNode node, SpawnToolbar toolbar)
        {
            this.Node = node;
            this.toolbar = toolbar;

            switch (node.Type)
            {
                case LevelNodeType.Resource:
                    this.foregroundSprite.sprite = node.Item.Sprite;
                    this.textComponent.text = node.Item.Value.ToString();
                    break;
                case LevelNodeType.Consumer:
                    this.backgroundSprite.sprite = node.Item.Sprite;
                    this.foregroundSprite.sprite = node.Item.Sprite;
                    this.textComponent.text = node.Item.Value.ToString();
                    break;
                case LevelNodeType.Factory:
                    ColorUtility.TryParseHtmlString(
                        node.FactoryMode == FactoryMode.Addition
                            ? this.factoryAdditionColour
                            : this.factoryDivisionColour, out Color color);
                    this.backgroundSprite.color = color;
                    this.textComponent.text = node.FactoryMode == FactoryMode.Addition ? "+" : "÷";
                    this.textComponent.fontStyle = node.FactoryMode == FactoryMode.Addition ? FontStyles.Bold : FontStyles.Normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // todo visual
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            this.toolbar.Remove(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var worldPosition = (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameManager.Instance.SpawnNode(this.Node, worldPosition);
            GameObject.Destroy(this.gameObject);
        }
    }
}