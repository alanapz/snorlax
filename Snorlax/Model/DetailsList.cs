using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    public abstract class DetailsList<T>
    {
        [JsonProperty("paging", Required = Required.Always)]
        public Paging Paging { get; set; } = null!;

        public abstract List<T> Results { get; set; }
    }
}
