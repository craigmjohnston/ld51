namespace Oatsbarley.LD51
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NaughtyAttributes;
    using Newtonsoft.Json;
    using Oatsbarley.LD51.Data;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance => GameManager.instance;

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
        [SerializeField] private CanvasGroup gameUiCanvasGroup;

        private float lastTick = 0;
        private Dictionary<string, Item> items;
        private Dictionary<string, Recipe> recipes;
        private JsonSerializerSettings serializerSettings;
        private List<ResourceNode> firstResourceNodes;
        private List<SpawnSegment> spawnSegments;
        private float startTime;
        private bool isPaused = false;

        public bool IsPlaying { get; private set; } = false;
        public float ItemTravelSpeed => this.itemTravelSpeed;

        public event Action Ticked;

        private void Awake()
        {
            GameManager.instance = this;
            this.lastTick = Time.time;
            this.gameUiCanvasGroup.alpha = 0;
            this.gameUiCanvasGroup.interactable = false;
            this.gameUiCanvasGroup.blocksRaycasts = false;
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
            this.IsPlaying = false;

            var definition = JsonConvert.DeserializeObject<Level>(levelJson.text, this.serializerSettings);
            this.firstResourceNodes = new List<ResourceNode>();

            foreach (var levelNode in definition.InitialSpawn)
            {
                this.SpawnNode(levelNode);
            }

            this.firstResourceNodes = FindObjectsOfType<ResourceNode>().ToList();
            foreach (var resourceNode in this.firstResourceNodes)
            {
                resourceNode.Populated += this.OnFirstResourcePopulated;
            }

            this.spawnSegments = definition.Spawns.ToList();
            float total = -5;
            foreach (var segment in this.spawnSegments)
            {
                total += 10; //segment.Time;
                segment.Time = total;
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

            this.startTime = Time.time;
            this.IsPlaying = true;
            this.gameUiCanvasGroup.alpha = 1;
            this.gameUiCanvasGroup.interactable = true;
            this.gameUiCanvasGroup.blocksRaycasts = true;
        }

        [Button()]
        public void TogglePause()
        {
            Time.timeScale = this.isPaused ? 1 : 0;
            this.isPaused = !this.isPaused;
        }

        private void Update()
        {
            if (!this.IsPlaying)
            {
                return;
            }

            // gameplay

            if (Time.time - this.lastTick >= this.autotickInterval)
            {
                this.Tick();
            }

            if (this.spawnSegments.Any())
            {
                if (Time.time - this.startTime >= this.spawnSegments.First().Time)
                {
                    var segment = this.spawnSegments.First();
                    this.spawnSegments.RemoveAt(0);
                    this.SpawnSegment(segment);
                }
            }
        }

        private void SpawnSegment(SpawnSegment segment)
        {
            foreach (var node in segment.Nodes)
            {
                this.SpawnNode(node);
            }
        }

        private float[] GetNewNodePosition()
        {
            return new float[]
            {
                Random.value * 10 - 5,
                Random.value * 10 - 5
            };
        }

        private void SpawnNode(LevelNode node)
        {
            var position = node.Position;
            if (position == null)
            {
                position = this.GetNewNodePosition();
            }

            switch (node.Type)
            {
                case LevelNodeType.Resource:
                    this.SpawnResource(node.Item, new Vector2(position[0], position[1]));
                    break;
                case LevelNodeType.Consumer:
                    this.SpawnConsumer(node.Item, new Vector2(position[0], position[1]));
                    break;
                case LevelNodeType.Factory: // todo
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnBackgroundDoubleClicked()
        {
            if (!this.IsPlaying)
            {
                return;
            }

            this.SpawnFactory();
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
            resource.transform.position = position;

            return resource;
        }

        public void SpawnConsumer(Item neededItem, Vector2 position)
        {
            var consumer = Instantiate(this.consumerNodePrefab);
            consumer.Init(neededItem);
            consumer.transform.position = position;
        }

        [Button()]
        public void Tick()
        {
            this.lastTick = Time.time;
            this.Ticked?.Invoke();
        }
    }
}