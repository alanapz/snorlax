using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    public class UsersList : DetailsList<User>
    {
        [JsonProperty("users", Required = Required.Always)]
        public override List<User> Results { get; set; } = null!;

    }
}
