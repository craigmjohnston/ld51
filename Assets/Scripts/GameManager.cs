namespace Oatsbarley.LD51
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NaughtyAttributes;
    using Newtonsoft.Json;
    using Oatsbarley.LD51.Data;
    using UnityEngine;
    using UnityEngine.SceneManagement;
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
        [SerializeField] private Sprite[] itemSprites;

        [SerializeField] private MainMenuManager mainMenuManager;
        [SerializeField] private CanvasGroup gameUiCanvasGroup;
        [SerializeField] private SpawnToolbar spawnToolbar;

        private float lastTick = 0;
        private Dictionary<string, Item> items;
        private Dictionary<string, Recipe> recipes;
        private JsonSerializerSettings serializerSettings;
        // private List<ResourceNode> firstResourceNodes;
        private List<SpawnSegment> spawnSegments;
        private float startTime;
        private bool isPaused = false;
        private bool isOnEndGameScreen = false;
        private List<GameObject> nodes = new List<GameObject>();

        public bool IsPlaying { get; private set; } = false;
        public float ItemTravelSpeed => this.itemTravelSpeed;

        public IReadOnlyCollection<Item> Items => this.items.Values;

        public event Action Ticked;

        private void Awake()
        {
            GameManager.instance = this;
            this.gameUiCanvasGroup.alpha = 0;
            this.gameUiCanvasGroup.interactable = false;
            this.gameUiCanvasGroup.blocksRaycasts = false;
        }

        private IEnumerator Start()
        {
            this.LoadDefinitions();

            yield return new WaitForSeconds(1);
            this.InitGame();
        }

        private void InitGame()
        {
            this.IsPlaying = false;
            this.lastTick = Time.time;
            this.RunLevel(this.levelsJson.First());
        }

        private void LoadDefinitions()
        {
            this.items = JsonConvert.DeserializeObject<Item[]>(this.itemsJson.text)?.ToDictionary(i => i.Tag);
            foreach (var item in this.items.Values)
            {
                var sprite = this.itemSprites.FirstOrDefault(s => s.name == item.SpriteName);
                if (sprite == null)
                {
                    continue;
                }

                item.Sprite = sprite;

                if (!string.IsNullOrWhiteSpace(item.ColourHex))
                {
                    ColorUtility.TryParseHtmlString(item.ColourHex, out item.Color);
                }
            }

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
            // this.firstResourceNodes = new List<ResourceNode>();

            foreach (var levelNode in definition.InitialSpawn)
            {
                this.spawnToolbar.Add(levelNode);
                // this.SpawnNode(levelNode);
            }

            // this.firstResourceNodes = FindObjectsOfType<ResourceNode>().ToList();
            // foreach (var resourceNode in this.firstResourceNodes)
            // {
            //     resourceNode.Populated += this.OnFirstResourcePopulated;
            // }

            this.spawnSegments = definition.Spawns.ToList();
            float total = -8; // start negative to make the first spawns come earlier than the normal interval
            foreach (var segment in this.spawnSegments)
            {
                total += 10; //segment.Time;
                segment.Time = total;
            }
        }

        [Button()]
        private void DebugWinGame()
        {
            this.LostGame("You've completed capitalism.", true);
        }

        public void OnFirstResourcePopulated()
        {
            // foreach (var node in this.firstResourceNodes)
            // {
            //     node.Populated -= this.OnFirstResourcePopulated;
            // }
            //
            // this.firstResourceNodes = null;

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
                if (this.isOnEndGameScreen)
                {
                    if (Input.GetKeyUp(KeyCode.Mouse0))
                    {
                        this.EndGameScreenClicked();
                    }
                }

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
            else
            {
                if (this.spawnToolbar.CurrentItems == 0)
                {
                    this.LostGame("You've completed capitalism.", true);
                }
            }
        }

        private void SpawnSegment(SpawnSegment segment)
        {
            foreach (var node in segment.Nodes)
            {
                this.spawnToolbar.Add(node);

                // this.SpawnNode(node);
            }

            if (this.spawnToolbar.CurrentItems > this.spawnToolbar.MaxItems)
            {
                this.LostGame("Your inbox filled up. (Maximum 7 items).");
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

            SpawnNode(node, new Vector2(position[0], position[1]));
        }

        public void SpawnNode(LevelNode node, Vector2 position)
        {
            switch (node.Type)
            {
                case LevelNodeType.Resource:
                    this.SpawnResource(node.Item, position);
                    break;
                case LevelNodeType.Consumer:
                    this.SpawnConsumer(node.Item, position);
                    break;
                case LevelNodeType.Factory:
                    this.SpawnFactory(node, position);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AudioManager.PlayOnce("resource_populate");

            if (!this.IsPlaying)
            {
                this.OnFirstResourcePopulated();
            }
        }

        public void OnBackgroundDoubleClicked()
        {
            // if (!this.IsPlaying)
            // {
            //     return;
            // }
            //
            // var worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            // this.SpawnFactory(worldPosition);
        }

        public void SpawnFactory(LevelNode node, Vector2 position)
        {
            Debug.Log($"Spawning factory at {position}");

            var factory = Instantiate(this.factoryPrefab);
            factory.transform.position = new Vector3(position.x, position.y, 0);

            factory.SetMode(node.FactoryMode);

            this.nodes.Add(factory.gameObject);
        }

        public ResourceNode SpawnResource(Item resourceItem, Vector2 position)
        {
            var resource = Instantiate(this.resourceNodePrefab);
            resource.Init(resourceItem);
            resource.transform.position = position;

            this.nodes.Add(resource.gameObject);

            return resource;
        }

        public void SpawnConsumer(Item neededItem, Vector2 position)
        {
            var consumer = Instantiate(this.consumerNodePrefab);
            consumer.Init(neededItem);
            consumer.transform.position = position;

            this.nodes.Add(consumer.gameObject);
        }

        [Button()]
        public void Tick()
        {
            this.lastTick = Time.time;
            this.Ticked?.Invoke();
        }

        public void LostGame(string reason, bool actuallyWon = false)
        {
            this.IsPlaying = false;
            Time.timeScale = 0;

            this.mainMenuManager.ShowEndGame(() =>
            {
                this.spawnToolbar.gameObject.SetActive(false);
                this.gameUiCanvasGroup.alpha = 0;
                this.gameUiCanvasGroup.interactable = false;
                this.gameUiCanvasGroup.blocksRaycasts = false;
                this.isOnEndGameScreen = true;
            }, reason, actuallyWon);
        }

        public void EndGameScreenClicked()
        {
            this.isOnEndGameScreen = false;
            this.Reload();
        }

        private void Reload()
        {
            this.Unload();

            this.mainMenuManager.Show(() =>
            {
                this.spawnToolbar.gameObject.SetActive(true);
                this.InitGame();
                Time.timeScale = 1f;
            });

            // this.mainMenuManager.ShowFrontCover(() =>
            // {
            //     var scene = SceneManager.GetActiveScene().name;
            //     SceneManager.LoadScene(scene, LoadSceneMode.Single);
            // });
        }

        private void Unload()
        {
            var toDestroy = this.nodes.ToList();
            foreach (var node in toDestroy)
            {
                GameObject.Destroy(node);
            }

            this.nodes.Clear();
            this.spawnToolbar.Clear();
            this.spawnSegments.Clear();
            this.isPaused = false;

            var travellingObjects = FindObjectsOfType<TravellingItem>();
            foreach (var travelling in travellingObjects)
            {
                GameObject.Destroy(travelling.gameObject);
            }
        }
    }
}