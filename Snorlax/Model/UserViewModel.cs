using System.Collections.Generic;
using Shared.App.Utils;

namespace Snorlax.Model
{
    public class UserViewModel
    {
        public UserViewModel(User user)
        {
            this.User = Require.NonNull(user);
        }

        public User User { get; }

        public List<Issue> UnassignedIssues { get; set; } = new List<Issue>();

        public List<Issue> OverdueIssues { get; set; } = new List<Issue>();

        public string OverdueIssuesUrl(string serverUrl) => $"{serverUrl}/issues?assignees={User.Key}&resolved=false";
    }
}
