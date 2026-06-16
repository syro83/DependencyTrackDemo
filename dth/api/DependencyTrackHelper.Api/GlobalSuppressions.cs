// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "Not this optimaization", Scope = "namespaceanddescendants", Target = "~N:DependencyTrackHelper.Api.Services")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "ASP.NET Core app - no SynchronizationContext.")]
[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "LoggerMessage refactoring not required in this application.")]
[assembly: SuppressMessage("Performance", "CA1873:Avoid potentially expensive logging", Justification = "LoggerMessage refactoring not required in this application.")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Classes are instantiated by DI container and JSON serializer via reflection.")]
