namespace Oatsbarley.LD51
{
    using System.Collections.Generic;
    using System.Linq;
    using Oatsbarley.LD51.Data;
    using Oatsbarley.LD51.Interfaces;
    using TMPro;
    using UnityEngine;

    public class FactoryNode : MonoBehaviour, IReceiver
    {
        [SerializeField] private SpriteRenderer backgroundSprite;
        [SerializeField] private SpriteRenderer foregroundSprite;

        [SerializeField] private TextMeshPro textComponent;
        [SerializeField] private Connector connector;

        // [SerializeField] private Recipe recipe;

        private Recipe recipe;
        private List<Item> itemBuffer = new List<Item>();

        public void SetRecipe(Recipe recipe)
        {
            this.recipe = recipe;
        }

        private void Start()
        {
            this.connector.SetReceiver(this);
            GameManager.Instance.Ticked += this.OnTick;
        }

        private void OnTick()
        {
            var remaining = this.itemBuffer.ToList();
            foreach (var recipeInput in this.recipe.Inputs)
            {
                if (!remaining.Remove(recipeInput))
                {
                    return;
                }
            }

            Debug.Log("Ready to create item");
            this.Generate();
        }

        public bool CanReceive(Item item)
        {
            return this.recipe.Inputs.Any(i => i == item);
        }

        public void Receive(Item item)
        {
            this.itemBuffer.Add(item);
        }

        private void Generate()
        {
            var remaining = this.itemBuffer.ToList();
            foreach (var recipeInput in this.recipe.Inputs)
            {
                if (!remaining.Remove(recipeInput))
                {
                    return;
                }
            }

            this.itemBuffer = remaining;

            foreach (var recipeOutput in this.recipe.Outputs)
            {
                this.connector.Send(recipeOutput);
            }
        }
    }
}