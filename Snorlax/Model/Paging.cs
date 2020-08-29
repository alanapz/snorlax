using Newtonsoft.Json;
using System;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    public class Paging
    {
        [JsonProperty("pageIndex", Required = Required.Always)]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize", Required = Required.Always)]
        public int PageSize { get; set; }

        [JsonProperty("total", Required = Required.Always)]
        public int Total { get; set; }
    }
}
