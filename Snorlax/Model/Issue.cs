using Newtonsoft.Json;
using System;

namespace Snorlax.Model
{
    [JsonObject]
    [Serializable]
    public class Issue
    {
        [JsonProperty("key", Required = Required.Always)]
        public string IssueId { get; set; } = null!;

        [JsonProperty("project", Required = Required.Always)]
        public string Project { get; set; } = null!;

        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonProperty("assignee")]
        public string? AssignedTo { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("severity")]
        public string? Severity { get; set; }

        [JsonProperty("component")]
        public string? Component { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        public int DaysOverdue() => (int) DateTime.Now.Subtract(CreationDate).TotalDays;

        public string IssueUrl(string serverUrl) => $"{serverUrl}/project/issues?id={Project}&open={IssueId}";
    }
}
