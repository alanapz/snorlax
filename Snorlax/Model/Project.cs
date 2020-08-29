using System;
using Newtonsoft.Json;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    public class Project
    {
        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; } = null!;

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; } = null!;
    }
}
