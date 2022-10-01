namespace Oatsbarley.LD51.Data
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class TagJsonConverter<T> : JsonConverter
    {
        private Dictionary<string, T> itemDefinitions;

        public TagJsonConverter(Dictionary<string, T> itemDefinitions)
        {
            this.itemDefinitions = itemDefinitions;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var tag = JToken.ReadFrom(reader).Value<string>();
            if (string.IsNullOrWhiteSpace(tag))
            {
                return null;
            }

            if (!this.itemDefinitions.TryGetValue(tag, out T item))
            {
                return null;
            }

            return item;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Item).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}