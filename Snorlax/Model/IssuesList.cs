using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    class IssuesList : DetailsList<Issue>
    {
        [JsonProperty("issues", Required = Required.Always)]
        public override List<Issue> Results { get; set; } = null!;

    }
}
