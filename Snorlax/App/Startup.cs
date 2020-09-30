using System;
using System.IO;
using System.Reflection;
using Shared.App.Utils;
using System.Globalization;
using System.Linq;
using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Appender;
using log4net.Core;

namespace Snorlax
{
    class Startup
    {
        static void Main(string[] args)
        {
            ILoggerRepository logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            ILog logger = LogManager.GetLogger(typeof(Startup));
            logger.Info($"Snorlax v{Assembly.GetExecutingAssembly().GetName().Version} starting ...");
            logger.Info($"URL: {Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description}");

            if ("true".Equals(Environment.GetEnvironmentVariable("SNORLAX_VERBOSE")))
            {
                logRepository.GetAppenders().OfType<AppenderSkeleton>().Single(a => a.Name == "snorlax-default").Threshold = Level.All;
                logger.Info("*** VERBOSE ***");
            }

            Processor processor = new Processor();

            processor.SonarUrl = Require.NonNullNonEmpty(Environment.GetEnvironmentVariable("SNORLAX_SONAR_URL"), "SNORLAX_SONAR_URL not defined");
            processor.Token = Require.NonNullNonEmpty(Environment.GetEnvironmentVariable("SNORLAX_SONAR_TOKEN"), "SNORLAX_SONAR_TOKEN not defined");
            processor.MaxCreationDate = DateTime.Now.Subtract(TimeSpan.FromDays(int.Parse(Environment.GetEnvironmentVariable("SNORLAX_DAYS_OVERDUE") ?? "10", NumberStyles.None, CultureInfo.InvariantCulture)));

            processor.ProjectFilter.UnionWith((Environment.GetEnvironmentVariable("SNORLAX_PROJECT_FILTER") ?? "").Split(',').Where(IsNotNullOrEmpty));
            processor.IgnoredProjects.UnionWith((Environment.GetEnvironmentVariable("SNORLAX_IGNORED_PROJECTS") ?? "").Split(',').Where(IsNotNullOrEmpty));

            processor.UserFilter.UnionWith((Environment.GetEnvironmentVariable("SNORLAX_USER_FILTER") ?? "").Split(',').Where(IsNotNullOrEmpty));
            processor.IgnoredUsers.UnionWith((Environment.GetEnvironmentVariable("SNORLAX_IGNORED_USERS") ?? "").Split(',').Where(IsNotNullOrEmpty));

            processor.SmtpServer = Require.NonNullNonEmpty(Environment.GetEnvironmentVariable("SNORLAX_SMTP_SERVER"), "SNORLAX_SMTP_SERVER not defined");
            processor.SmtpSenderAddress = (Environment.GetEnvironmentVariable("SNORLAX_SMTP_SENDER") ?? $"snorlax-outbound@{new Uri(processor.SonarUrl!).Host}");
            processor.SmtpSslEnabled = "true".Equals(Environment.GetEnvironmentVariable("SNORLAX_SMTP_SSL_ENABLED"));
            processor.SmtpUsername = Environment.GetEnvironmentVariable("SNORLAX_SMTP_USERNAME");
            processor.SmtpPassword = Environment.GetEnvironmentVariable("SNORLAX_SMTP_PASSWORD");

            processor.RecipientFilter.UnionWith((Environment.GetEnvironmentVariable("SNORLAX_RECIPIENT_FILTER") ?? "").Split(',').Where(IsNotNullOrEmpty));
            processor.IgnoredRecipients.UnionWith((Environment.GetEnvironmentVariable("SNORLAX_IGNORED_RECIPIENTS") ?? "").Split(',').Where(IsNotNullOrEmpty));

            processor.SummaryRecipients.UnionWith((Environment.GetEnvironmentVariable("SNORLAX_SUMMARY") ?? "").Split(',').Where(IsNotNullOrEmpty));

            processor.AdminGroups.UnionWith((Environment.GetEnvironmentVariable("SNORLAX_ADMIN_GROUPS") ?? "").Split(',').Where(IsNotNullOrEmpty));

            processor.Process();
        }

        private static bool IsNotNullOrEmpty(string value)
        {
            return !string.IsNullOrEmpty(value);
        }
    }
}
