using System;
using Newtonsoft.Json;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    public class User
    {
        [JsonProperty("login", Required = Required.Always)]
        public string Key { get; set; } = null!;

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; } = null!;

        [JsonProperty("email", Required = Required.Always)]
        public string EmailAddress { get; set; } = null!;

        [JsonProperty("groups", Required = Required.Always)]
        public string[] Groups { get; set; } = null!;

    }
}
