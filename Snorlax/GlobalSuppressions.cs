// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Critical Vulnerability", "S4830:Server certificates should be verified during SSL/TLS connections", Justification = "<Pending>", Scope = "member", Target = "~M:Sonaxx.Net.RestClient.Execute``1~``0")]
[assembly: SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty", Justification = "<Pending>", Scope = "member", Target = "~M:Sonaxx.Net.ConnectionListener.OnRequest(System.String,System.Uri,System.String)")]
[assembly: SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty", Justification = "<Pending>", Scope = "member", Target = "~M:Sonaxx.Net.ConnectionListener.OnResponse(System.Net.HttpStatusCode,System.String)")]
[assembly: SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty", Justification = "<Pending>", Scope = "member", Target = "~M:Sonaxx.Net.ConnectionListener.OnError(System.Net.WebExceptionStatus,System.Nullable{System.Net.HttpStatusCode},System.String)")]
[assembly: SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>", Scope = "type", Target = "~T:Sonaxx.Net.HttpStatusCodeException")]
