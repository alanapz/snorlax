using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    class UserPermissionsList : DetailsList<UserPermission>
    {
        [JsonProperty("users", Required = Required.Always)]
        public override List<UserPermission> Results { get; set; } = null!;

    }
}
