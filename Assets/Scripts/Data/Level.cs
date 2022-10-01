namespace Oatsbarley.LD51.Data
{
    public class Level
    {
        public string Name { get; set; }
        // spawned immediately
        public LevelNode[] InitialSpawn { get; set; }
    }

    public class LevelNode
    {
        public LevelNodeType Type { get; set; }
        public Item Item { get; set; }
        public float[] Position { get; set; }
    }

    public enum LevelNodeType
    {
        Resource,
        Consumer,
        Factory
    }
}