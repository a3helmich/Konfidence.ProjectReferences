using System;
using System.Collections.Generic;
using System.Linq;
using Konfidence.Base;
using ToolInterfaces;

namespace ToolClasses.ExtensionMethods;

public static class CommandLineExtensions
{
    private static readonly string[] SwitchPrefixes = ["--", "/"];

    /// <summary>
    /// every valueless switch the tool accepts, so the call sites cannot drift apart
    /// </summary>
    public static readonly Arguments[] SwitchArguments = [Arguments.AllProjects, Arguments.Help];

    extension(string[] args)
    {
        /// <summary>
        /// AddCommandLine has no notion of a valueless switch: for '--Verbose' it takes the
        /// argument that follows as its value, swallowing that argument. Rewriting '--Verbose'
        /// to '--Verbose=true' hands the configuration provider the key/value pair it expects,
        /// so switches and flags can be mixed in any order.
        /// </summary>
        public string[] ExpandSwitchArguments(params Arguments[] switchArguments)
        {
            List<string> switchNames = [.. switchArguments.Select(switchArgument => switchArgument.ToString())];

            return [.. args.Select(argument => argument.ExpandSwitchArgument(switchNames))];
        }
    }

    extension(string argument)
    {
        private string ExpandSwitchArgument(List<string> switchNames)
        {
            string prefix = SwitchPrefixes.FirstOrDefault(switchPrefix => argument.StartsWith(switchPrefix, StringComparison.Ordinal)) ?? string.Empty;

            if (!prefix.IsAssigned())
            {
                return argument;
            }

            string switchName = argument[prefix.Length..];

            return switchNames.Contains(switchName, StringComparer.OrdinalIgnoreCase)
                ? $"{argument}=true"
                : argument;
        }
    }
}
