using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    class GroupPermissionsList : DetailsList<GroupPermission>
    {
        [JsonProperty("groups", Required = Required.Always)]
        public override List<GroupPermission> Results { get; set; } = null!;

    }
}
