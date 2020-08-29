using System.Collections.Generic;
using Shared.App.Utils;

namespace Snorlax.Model
{
    public class ProjectViewModel
    {
        public ProjectViewModel(Project project)
        {
            this.Project = Require.NonNull(project);
        }

        public Project Project { get; }

        public List<Issue> UnassignedIssues { get; set; } = new List<Issue>();

        public string UnassignedIssuesUrl(string serverUrl) => $"{serverUrl}/project/issues?id={Project.Key}&assigned=false&resolved=false";

    }
}
