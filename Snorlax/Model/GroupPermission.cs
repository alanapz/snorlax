using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    public class GroupPermission
    {
        [JsonProperty("name", Required = Required.Always)]
        public string GroupKey { get; set; } = null!;

        [JsonProperty("permissions", Required = Required.Always)]
        public List<string> Permissions { get; set; } = null!;
    }
}
