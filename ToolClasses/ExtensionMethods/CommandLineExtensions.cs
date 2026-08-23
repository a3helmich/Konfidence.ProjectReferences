using System;
using System.Collections.Generic;
using System.Linq;
using Konfidence.Base;
using ToolInterfaces;

namespace ToolClasses.ExtensionMethods;

public static class CommandLineExtensions
{
    private static readonly string[] SwitchPrefixes = ["--", "/"];

    public static readonly Arguments[] SwitchArguments = [Arguments.AllProjects, Arguments.Help];

    extension(string[] args)
    {
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
