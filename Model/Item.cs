using System;
using System.Text.Json.Serialization;

namespace Model
{
    public class Item
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public bool IsCompleted { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
    }
}
