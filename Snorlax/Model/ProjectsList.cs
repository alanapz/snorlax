using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    public class ProjectsList : DetailsList<Project>
    {
        [JsonProperty("components", Required = Required.Always)]
        public override List<Project> Results { get; set; } = null!;

    }
}
