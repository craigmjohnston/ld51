namespace Oatsbarley.LD51.Data
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Level
    {
        public string Name { get; set; }
        // spawned immediately
        public LevelNode[] InitialSpawn { get; set; }

        public SpawnSegment[] Spawns { get; set; }
    }

    public class SpawnSegment
    {
        public float Time { get; set; }
        public LevelNode[] Nodes { get; set; }
    }

    public class LevelNode
    {
        public LevelNodeType Type { get; set; }
        public Item Item { get; set; }
        public float[] Position { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public FactoryMode FactoryMode { get; set; }
    }

    public enum LevelNodeType
    {
        Resource,
        Consumer,
        Factory
    }
}