using System.Collections.Generic;
using System.Linq;

namespace Snorlax.Model
{
    public class SummaryViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();

        public List<ProjectViewModel> Projects { get; set; } = new List<ProjectViewModel>();

        public int NumberOfOverdueIssues => Users.SelectMany(u => u.OverdueIssues).Count();

        public int NumberOfUnassignedIssues => Projects.SelectMany(p => p.UnassignedIssues).Count();

    }
}
