using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ToDoWeb.Models
{
    public class Item
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public bool IsDone { get; set; }
        public string Description { get; set; }
    }
}
