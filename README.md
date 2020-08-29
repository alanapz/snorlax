# SNORLAX

- GitHub: https://github.com/alanapz/snorlax
- Docker: https://hub.docker.com/r/alanmpinder/snorlax

Snorlax is a simple program that uses the SONAR API interface to retrieve, for every user and project, the number of unresolved and unassigned issues.
It then, using the Razor library, generates two sets of reports:

- For users, a list of unresolved overdue issues and all unassigned issues on all projects the user has access to
- For managers, a list of all users (with the number of unresolved overdue issues for every user) and all projects (with the number of unassigned issues for every project)

Snorlax is a NET Core portable application that is designed to be completely configurable - all the parameters are specified in the form of environment variables (to be Docker friendly) and the e-mail templates are easily modified.

The program has been written to be easily called automatically as a scheduled task.

## How To Run

The easiest way to run is via the official Docker image: docker.io/alanmpinder/snorlax:1.0

```
docker run -it -e SNORLAX_SONAR_URL=*SonarUrl* -e SNORLAX_SONAR_TOKEN=*ApiToken* -e SNORLAX_SMTP_SERVER=*SmtpServer* docker.io/alanmpinder/snorlax:1.0
```

You can use the SNORLAX_RECIPIENT_FILTER and SNORLAX_IGNORED_RECIPIENTS environment variables to set which SONAR users receive alerts:

```
docker run -it -e SNORLAX_SONAR_URL=*SonarUrl* -e SNORLAX_SONAR_TOKEN=*ApiToken* -e SNORLAX_SMTP_SERVER=*SmtpServer* -e SNORLAX_RECIPIENT_FILTER="jsmith,bjones" docker.io/alanmpinder/snorlax:1.0
```

And the SNORLAX_SUMMARY environment variable to set which e-mail addresses receive summary reports:

```
docker run -it -e SNORLAX_SONAR_URL=*SonarUrl* -e SNORLAX_SONAR_TOKEN=*ApiToken* -e SNORLAX_SMTP_SERVER=*SmtpServer* -e SNORLAX_RECIPIENT_FILTER="jsmith,bjones" -e SNORLAX_SUMMARY="user1@email1.com,user2@email2.com" docker.io/alanmpinder/snorlax:1.0
```

See the Configuration section below for details on the required environment variables.

## Configuration

Snorlax is configured purely via environment variables.

(Array type values are separated by commas)

| Name | Required | Type | Details |
|------|----------|------|---------|
| SNORLAX_SONAR_URL | Required | String | URL of Sonar server, with trailing slash (eg: https://sonar.ci.object23.it/ ) |
| SNORLAX_SONAR_TOKEN | Required | String | Sonar API access token |
| SNORLAX_DAYS_OVERDUE | Optional | Integer | The number of days until an issue is considered overdue (10 by default) |
| SNORLAX_PROJECT_FILTER | Optional | String[] | If defined, include only these projects - Incompatible with SNORLAX_IGNORED_PROJECTS |
| SNORLAX_IGNORED_PROJECTS | Optional | String[] | List project keys to be ignored - Incompatible with SNORLAX_PROJECT_FILTER |
| SNORLAX_USER_FILTER | Optional | String[] | if defined, include only these users - Incompatible with SNORLAX_IGNORED_USERS |
| SNORLAX_IGNORED_USERS | Optional | String[] | List of usernames to be ignored (eg: user1,user2) = Incompatible with SNORLAX_USER_FILTER |
| SNORLAX_SMTP_SERVER | Required | String | SMTP server hostname |
| SNORLAX_SMTP_SENDER | Optional | String | The sender e-mail address used for outgoing messages (eg: snorlax-out@alanpinder.com) - Defaults to snorlax-outbound@SNORLAX_SONAR_URL if undefineD |
| SNORLAX_SMTP_SSL_ENABLED | Optional | Boolean | If set to "true", use SSL to encrypt SMTP connection - defaults to false if undefined |
| SNORLAX_SMTP_USERNAME | Optional | String | If set, the SMTP username to use to send outgoing messages (if set, SNORLAX_SMTP_PASSWORD must also be set) |
| SNORLAX_SMTP_PASSWORD | Optional | String | If set, the SMTP password to use to send outgoing messages (if set, SNORLAX_SMTP_USERNAME must also be set) |
| SNORLAX_RECIPIENT_FILTER | Optional | String[] | if defined, only send notify these users - Incompatible with SNORLAX_IGNORED_RECIPIENTS |
| SNORLAX_IGNORED_RECIPIENTS | Optional | String[] | List of usernames that will -NOT- be notified - Incompatible with SNORLAX_RECIPIENT_FILTER |
| SNORLAX_SUMMARY | Optional | String[] | If present, a list of e-mail addresses to retrieve summary reports |
| SNORLAX_VERBOSE | Optional | Boolean | If set to "true", will output trace information to console (false by default) |

## Customisation

The subject and content of mails can be customised by modifying the appropriate cshtml file:

| Path | Description |
|------|-------------|
| [Views\UserSubject\Template.cshtml](https://raw.githubusercontent.com/alanapz/snorlax/master/Snorlax/Views/UserSubject/Template.cshtml) | Subject of mail to user |
| [Views\UserBody\Template.cshtml](https://raw.githubusercontent.com/alanapz/snorlax/master/Snorlax/Views/UserBody/Template.cshtml) | Body of mail to user |
| [Views\SummarySubject\Template.cshtml](https://raw.githubusercontent.com/alanapz/snorlax/master/Snorlax/Views/SummarySubject/Template.cshtml) | Subject of report summary |
| [Views\SummaryBody\Template.cshtml](https://raw.githubusercontent.com/alanapz/snorlax/master/Snorlax/Views/SummaryBody/Template.cshtml) | Body of report summary |

For more convenient Docker mounts, every template is in it's own folder.
