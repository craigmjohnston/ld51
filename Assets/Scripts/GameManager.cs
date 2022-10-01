namespace Oatsbarley.LD51
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NaughtyAttributes;
    using Newtonsoft.Json;
    using Oatsbarley.LD51.Data;
    using UnityEngine;

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private bool autotickEnabled;
        [SerializeField] private float autotickInterval;
        [SerializeField] private float itemTravelSpeed;

        [SerializeField] private TextAsset itemsJson;
        [SerializeField] private TextAsset recipesJson;
        [SerializeField] private TextAsset[] levelsJson;

        [SerializeField] private ResourceNode resourceNodePrefab;
        [SerializeField] private ConsumerNode consumerNodePrefab;
        [SerializeField] private FactoryNode factoryPrefab;

        [SerializeField] private MainMenuManager mainMenuManager;

        private float lastTick = 0;

        private Dictionary<string, Item> items;
        private Dictionary<string, Recipe> recipes;
        private JsonSerializerSettings serializerSettings;
        private List<ResourceNode> firstResourceNodes;

        private static GameManager instance;

        public event Action Ticked;

        public static GameManager Instance => GameManager.instance;

        public float ItemTravelSpeed => this.itemTravelSpeed;

        private void Awake()
        {
            GameManager.instance = this;
            this.lastTick = Time.realtimeSinceStartup;
        }

        private void Start()
        {
            this.LoadDefinitions();
            this.RunLevel(this.levelsJson.First());
        }

        private void LoadDefinitions()
        {
            this.items = JsonConvert.DeserializeObject<Item[]>(this.itemsJson.text)?.ToDictionary(i => i.Tag);
            this.serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new TagJsonConverter<Item>(this.items)
                }
            };

            this.recipes = JsonConvert.DeserializeObject<Recipe[]>(this.recipesJson.text, this.serializerSettings)?.ToDictionary(r => r.Tag);

            this.serializerSettings.Converters.Add(new TagJsonConverter<Recipe>(this.recipes));
        }

        private void RunLevel(TextAsset levelJson)
        {
            var definition = JsonConvert.DeserializeObject<Level>(levelJson.text, this.serializerSettings);
            this.firstResourceNodes = new List<ResourceNode>();

            foreach (var levelNode in definition.InitialSpawn)
            {
                switch (levelNode.Type)
                {
                    case LevelNodeType.Resource:
                        var resourceNode = this.SpawnResource(levelNode.Item, new Vector2(levelNode.Position[0], levelNode.Position[1]));
                        resourceNode.Populated += this.OnFirstResourcePopulated;
                        this.firstResourceNodes.Add(resourceNode);
                        break;
                    case LevelNodeType.Consumer:
                        this.SpawnConsumer(levelNode.Item, new Vector2(levelNode.Position[0], levelNode.Position[1]));
                        break;
                    case LevelNodeType.Factory: // todo
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void OnFirstResourcePopulated()
        {
            foreach (var node in this.firstResourceNodes)
            {
                node.Populated -= this.OnFirstResourcePopulated;
            }

            this.firstResourceNodes = null;

            this.mainMenuManager.Hide();
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup - this.lastTick >= this.autotickInterval)
            {
                this.Tick();
            }
        }

        public void SpawnFactory()
        {
            var worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log($"Spawning factory at {worldPosition}");

            var factory = Instantiate(this.factoryPrefab);
            factory.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);

            // todo
            factory.SetRecipe(this.recipes["recipe_iron_bar"]);
        }

        public ResourceNode SpawnResource(Item resourceItem, Vector2 position)
        {
            var resource = Instantiate(this.resourceNodePrefab);
            resource.Init(resourceItem);
            // todo position

            return resource;
        }

        public void SpawnConsumer(Item neededItem, Vector2 position)
        {
            var consumer = Instantiate(this.consumerNodePrefab);
            consumer.Init(neededItem);
        }

        [Button()]
        public void Tick()
        {
            this.lastTick = Time.realtimeSinceStartup;
            this.Ticked?.Invoke();
        }
    }
}