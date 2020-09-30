using log4net;
using RazorLight;
using RazorLight.Extensions;
using Shared.App.Utils;
using Snorlax.Model;
using Snorlax.Net;
using Snorlax.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Snorlax
{
    delegate void ErrorHandler(Exception error);

    class Processor
    {
        public string? SonarUrl { get; set; }

        public string? Token { get; set; }

        public DateTime MaxCreationDate { get; set; }

        public HashSet<string> ProjectFilter { get; } = new HashSet<string>();

        public HashSet<string> IgnoredProjects { get; } = new HashSet<string>();

        public HashSet<string> UserFilter { get; } = new HashSet<string>();

        public HashSet<string> IgnoredUsers { get; } = new HashSet<string>();

        public string? SmtpServer { get; set; }

        public string? SmtpSenderAddress { get; set; }

        public bool SmtpSslEnabled { get; set; }

        public string? SmtpUsername { get; set; }

        public string? SmtpPassword { get; set; }

        public HashSet<string> RecipientFilter { get; } = new HashSet<string>();

        public HashSet<string> IgnoredRecipients { get; } = new HashSet<string>();

        public HashSet<string> SummaryRecipients { get; } = new HashSet<string>();

        public HashSet<string> AdminGroups { get; } = new HashSet<string>();

        public ErrorHandler? ErrorHandler { get; set; }

        private ILog Logger { get; } = LogManager.GetLogger(typeof(Processor));

        private const int PAGE_SIZE = 100;

        public void Process()
        {
            try
            {
                ProcessAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Logger.Error("Couldn't launch processor", e);
                ErrorHandler?.Invoke(e);
            }
        }

        public async Task ProcessAsync()
        {
            Require.NonNullNonEmpty(SonarUrl, "SonarUrl");
            Require.NonNullNonEmpty(Token, "Token");
            Require.NonNullNonEmpty(SmtpServer, "SmtpServer");
            Require.NonNullNonEmpty(SmtpSenderAddress, "SmtpServerAddress");
            if (SmtpUsername != null && SmtpPassword != null)
            {
                Require.NonNullNonEmpty(SmtpUsername, "SmtpUsername");
                Require.NonNullNonEmpty(SmtpPassword, "SmtpPassword");
            }

            Logger.Info($"Sonar URL: {SonarUrl}");
            Logger.Info($"Max Issue Date: {MaxCreationDate}");
            Logger.Info($"Project filter: {ProjectFilter.ToPrettyString()}, ignored projects: {IgnoredProjects.ToPrettyString()}");
            Logger.Info($"User filter: {UserFilter.ToPrettyString()}, ignored users: {IgnoredUsers.ToPrettyString()}");
            Logger.Info($"SMTP server: {SmtpServer} ({(SmtpSslEnabled ? "SSL enabled" : "SSL disabled")})");
            Logger.Info($"SMTP sender address: {SmtpSenderAddress}");
            Logger.Info($"SMTP authentication: {(SmtpUsername != null && SmtpPassword != null ? $"Yes (username: {SmtpUsername})" : "None")}");
            Logger.Info($"Recipient filter: {RecipientFilter.ToPrettyString()}, ignored recipients: {IgnoredRecipients.ToPrettyString()}");
            Logger.Info($"Summary recipients: {SummaryRecipients.ToPrettyString()}");

            SummaryViewModel summary = await GenerateSummary().OnAnyThread();
            
            Logger.Info($"Processing complete, {summary.NumberOfOverdueIssues} issues overdue, {summary.NumberOfUnassignedIssues} issues unassigned, {summary.Users.Count} users to notify, {summary.Projects.Count} projects to notify");

            RazorLightEngine razor = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Startup)) // exception without this (or another project type)
                .UseMemoryCachingProvider()
                .Build();

            SmtpClient smtpClient = new SmtpClient(SmtpServer!);
            smtpClient.EnableSsl = SmtpSslEnabled;
            if (SmtpUsername != null && SmtpPassword != null)
            {
                smtpClient.Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
            }

            await NotifyUser(summary, razor, smtpClient).OnAnyThread();
            await NotifySummary(summary, razor, smtpClient).OnAnyThread();
        }

        private async Task NotifyUser(SummaryViewModel summary, RazorLightEngine razor, SmtpClient smtpClient)
        {
            if (!summary.Users.Any())
            {
                Logger.Warn("Not sending user notification, no users to notify");
                return;
            }

            string subjectTemplate = File.ReadAllText(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Views", "UserSubject", "Template.cshtml"));
            string bodyTemplate = File.ReadAllText(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Views", "UserBody", "Template.cshtml"));

            ExpandoObject viewBag = new { SonarUrl }.ToExpando();

            foreach (UserViewModel model in summary.Users)
            {
                if (IgnoredRecipients.Contains(model.User.Key) || (RecipientFilter.Any() && !RecipientFilter.Contains(model.User.Key)))
                {
                    Logger.Info($"Not sending mail to: {model.User.Key}, recipient ignored");
                    continue;
                }

                await DoSendMessage(
                    smtpClient: smtpClient,
                    recipient: model.User.EmailAddress,
                    subject: (await razor.CompileRenderStringAsync("user-subject", subjectTemplate, model, viewBag).OnAnyThread()).Trim(),
                    message: (await razor.CompileRenderStringAsync("user-body", bodyTemplate, model, viewBag).OnAnyThread()).Trim()).OnAnyThread();
            }
        }

        private async Task NotifySummary(SummaryViewModel summary, RazorLightEngine razor, SmtpClient smtpClient)
        {
            if (!SummaryRecipients.Any())
            {
                Logger.Warn("Not sending summary, no recipients defined");
                return;
            }

            string subjectTemplate = File.ReadAllText(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Views", "SummarySubject", "Template.cshtml"));
            string bodyTemplate = File.ReadAllText(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Views", "SummaryBody", "Template.cshtml"));

            ExpandoObject viewBag = new { SonarUrl }.ToExpando();

            Logger.Info($"Preparing to send summary mail to: {SummaryRecipients.ToPrettyString()}");

            foreach(string summaryRecipient in SummaryRecipients)
            {
                await DoSendMessage(
                    smtpClient: smtpClient,
                    recipient: summaryRecipient,
                    subject: (await razor.CompileRenderStringAsync("summary-subject", subjectTemplate, summary, viewBag).OnAnyThread()).Trim(),
                    message: (await razor.CompileRenderStringAsync("summary-body", bodyTemplate, summary, viewBag).OnAnyThread()).Trim()).OnAnyThread();
            }
        }

        private async Task DoSendMessage(SmtpClient smtpClient, string recipient, string subject, string message)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(SmtpSenderAddress);
            mailMessage.To.Add(recipient);
            mailMessage.Subject = subject;
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = true;

            Logger.Info($"Sending message '{subject}' to: {recipient}");

            await smtpClient.SendMailAsync(mailMessage).OnAnyThread();
        }

        private async Task<SummaryViewModel> GenerateSummary()
        {
            List<Project> projects = await ListProjectsAsync().OnAnyThread();
            Logger.Info($"Retrieved {projects.Count} projects");

            if (ProjectFilter.Any() || IgnoredProjects.Any())
            {
                if (ProjectFilter.Any())
                {
                    projects.RemoveAll(project => !ProjectFilter.Contains(project.Key));
                }
                projects.RemoveAll(project => IgnoredProjects.Contains(project.Key));
                Logger.Info($"Filtered ignored projects, {projects.Count} remaining");
            }

            List<User> users = await ListUsersAsync().OnAnyThread();
            Logger.Info($"Retrieved {users.Count} users");

            if (UserFilter.Any() || IgnoredUsers.Any())
            {
                if (UserFilter.Any())
                {
                    users.RemoveAll(user => !UserFilter.Contains(user.Key!));
                }
                users.RemoveAll(user => IgnoredUsers.Contains(user.Key));
                Logger.Info($"Filtered ignored users, {users.Count} remaining");
            }

            if (!projects.Any() || !users.Any())
            {
                Logger.Warn("Nothing to do");
                return new SummaryViewModel();
            }

            Dictionary<string, UserViewModel> userDetails = new Dictionary<string, UserViewModel>();

            foreach (User user in users)
            {
                userDetails.Add(user.Key, new UserViewModel(user)
                {
                    OverdueIssues = await GetOverdueIssuesForUserAsync(user.Key).OnAnyThread()
                });
            }

            Dictionary<string, HashSet<User>> projectUserMappings = new Dictionary<string, HashSet<User>>(); // User => project

            foreach (Project project in projects)
            {
                foreach (User user in await GetUsersForProjectAsync(project.Key, users).OnAnyThread())
                {
                    projectUserMappings.GetOrCreate(project.Key, () => new HashSet<User>()).Add(user);
                }
            }

            foreach (Project project in new List<Project>(projects))
            {
                if (!projectUserMappings.ContainsKey(project.Key) || projectUserMappings[project.Key].Count == 0)
                {
                    Logger.Warn($"Ignoring project: {project.Key}, project has no users");
                    projects.Remove(project);
                }
            }

            Dictionary<string, ProjectViewModel> projectDetails = new Dictionary<string, ProjectViewModel>();

            foreach (Project project in projects)
            {
                projectDetails.Add(project.Key, new ProjectViewModel(project));
            }

            foreach (Issue issue in await GetUnassignedIssuesForProjectAsync(projects).OnAnyThread())
            {
                foreach (User user in projectUserMappings[issue.Project])
                {
                    userDetails[user.Key].UnassignedIssues.Add(issue);
                }

                projectDetails[issue.Project].UnassignedIssues.Add(issue);
            }

            userDetails.RemoveIf(entry => !entry.Value.OverdueIssues.Any() && !entry.Value.UnassignedIssues.Any());
            projectDetails.RemoveIf(entry => !entry.Value.UnassignedIssues.Any());

            return new SummaryViewModel()
            {
                Users = userDetails.Values.ToList(),
                Projects = projectDetails.Values.ToList()
            };
        }

        private Task<List<Project>> ListProjectsAsync()
        {
            return Task.Run(() => ExecutePageableRequest<ProjectsList, Project>("projects/search?unused=1"));
        }

        private Task<List<User>> ListUsersAsync()
        {
            return Task.Run(() => ExecutePageableRequest<UsersList, User>("users/search?unused=1"));
        }

        private Task<List<Issue>> GetOverdueIssuesForUserAsync(string username)
        {
            return Task.Run(() => ExecutePageableRequest<IssuesList, Issue>($"issues/search?statuses=OPEN,REOPENED&assignees={username}&createdBefore={MaxCreationDate.ToUniversalTime():yyyy-MM-dd}&resolved=false&s=CREATION_DATE&asc=true"));
        }

        private Task<List<User>> GetUsersForProjectAsync(string projectKey, List<User> users)
        {
            return Task.Run(() =>
            {
                // Slightly complex - permissions can be granted via groups or directly on users
                // With no web service to retrieve aggregated data, we need to lookup group + user details
                List<GroupPermission> groupPermissions = ExecutePageableRequest<GroupPermissionsList, GroupPermission>($"permissions/groups?projectKey={projectKey}");

                HashSet<string> groupKeys = groupPermissions
                    .Where(groupPermission => groupPermission.Permissions.Count > 0) // Ignore empty entries
                    .Where(groupPermission => groupPermission.GroupKey != "sonar-administrators") // Ignore admins (otherwise admin would be spammed with details for every project)
                    .Where(groupPermission => !AdminGroups.Contains(groupPermission.GroupKey)) // See SNLX-1: supplemental admin groups
                    .Select(groupPermission => groupPermission.GroupKey)
                    .ToHashSet();

                List<UserPermission> userPermissions = ExecutePageableRequest<UserPermissionsList, UserPermission>($"permissions/users?projectKey={projectKey}");

                HashSet<string> userKeys = userPermissions
                    .Where(userPermission => userPermission.Permissions.Count > 0) // Ignore empty entries
                    .Select(userPermission => userPermission.UserKey)
                    .ToHashSet();

                return users.Where(user => userKeys.Contains(user.Key) || groupKeys.Intersect(user.Groups).Any()).ToList();
            });
        }

        private Task<List<Issue>> GetUnassignedIssuesForProjectAsync(List<Project> projects)
        {
            return Task.Run(() => ExecutePageableRequest<IssuesList, Issue>($"issues/search?assigned=false&componentKeys={string.Join(",", projects.Select(p => p.Key))}&resolved=false&s=CREATION_DATE&asc=true"));
        }

        private List<R> ExecutePageableRequest<T, R>(string url) where T : DetailsList<R>
        {
            List<R> buffer = new List<R>();
            int pageNumber = 1;
            while (true)
            {
                using (RestClient client = new RestClient($"{SonarUrl}/api/{url}&p={pageNumber}&ps={PAGE_SIZE}"))
                {
                    client.SetConfig(jsonConfig => jsonConfig.CheckAdditionalContent = true);
                    client.SetMethod("GET");
                    client.SetHeader("Authorization", string.Format("Basic {0}", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Token}:"))));
                    List<R> results = Require.NonNull(Require.NonNull(client.Execute<T>(), "Result").Results, "Result.Results");
                    buffer.AddRange(results);
                    Logger.Debug($"{client.URL} - page {pageNumber}, {results.Count} results");
                    if (results.Count < PAGE_SIZE)
                    {
                        return buffer;
                    }
                    pageNumber++;
                }
            }
        }
    }
}
