namespace Oatsbarley.LD51
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DG.Tweening;
    using Oatsbarley.LD51.Data;
    using UnityEngine;

    // list the coming spawns in a bar, then the player can pick from several
    // if a spawn is on the bar for too long, it'll expire and the player gets a debuff of some sort

    public class SpawnToolbar : MonoBehaviour
    {
        [SerializeField] private SpawnToolbarItem factoryItemPrefab;
        [SerializeField] private SpawnToolbarItem consumerItemPrefab;
        [SerializeField] private SpawnToolbarItem resourceItemPrefab;
        [SerializeField] private float itemHeight;
        [SerializeField] private float itemSpacing;

        private List<SpawnToolbarItem> items = new List<SpawnToolbarItem>();

        public int MaxItems => 7;
        public int CurrentItems => this.items.Count;

        public void Clear()
        {
            foreach (var item in this.items.ToList())
            {
                GameObject.Destroy(item.gameObject);
            }

            this.items.Clear();
        }

        public void Add(LevelNode node)
        {
            SpawnToolbarItem item;
            switch (node.Type)
            {
                case LevelNodeType.Resource:
                    item = Instantiate(this.resourceItemPrefab, this.transform);
                    break;
                case LevelNodeType.Consumer:
                    item = Instantiate(this.consumerItemPrefab, this.transform);
                    break;
                case LevelNodeType.Factory:
                    item = Instantiate(this.factoryItemPrefab, this.transform);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            item.SetNode(node, this);

            // position
            item.transform.position = new Vector3(item.transform.position.x, -20, item.transform.position.z);
            this.MoveItemToIndex(item, this.items.Count);

            this.items.Add(item);
        }

        private void MoveItemToIndex(SpawnToolbarItem item, int index)
        {
            var position = 480 - 40 - this.itemHeight - this.itemSpacing - (index * this.itemHeight + (index - 1) * this.itemSpacing);
            item.transform.DOMoveY(position, 0.8f, false);
        }

        // removes an item from the toolbar (usually due to dragging it off) allows other items to shift up
        public void Remove(SpawnToolbarItem item)
        {
            var index = this.items.IndexOf(item);
            this.items.RemoveAt(index);

            for (int i = index; i < this.items.Count; i++)
            {
                this.MoveItemToIndex(this.items[i], i);
            }
        }
    }
}