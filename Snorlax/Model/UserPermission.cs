using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    public class UserPermission
    {
        [JsonProperty("login", Required = Required.Always)]
        public string UserKey { get; set; } = null!;

        [JsonProperty("permissions", Required = Required.Always)]
        public List<string> Permissions { get; set; } = null!;
    }
}
