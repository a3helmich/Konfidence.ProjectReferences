using System.Collections.Generic;
using System.IO;
using System.Linq;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses;

public static class ArgumentParser
{
    public static bool ValidateArguments(ApplicationConfiguration applicationConfiguration)
    {
        if (applicationConfiguration.Help)
        {
            WriteUsage();

            return false;
        }

        if (applicationConfiguration.IgnoredArguments.Any())
        {
            ReportIgnoredArguments(applicationConfiguration.IgnoredArguments);

            WriteUsage();

            return false;
        }

        if (HasValidArguments(applicationConfiguration))
        {
            WriteHelpHint();

            return true;
        }

        ReportInvalidArguments(applicationConfiguration);

        WriteUsage();

        return false;
    }

    private static bool HasValidArguments(ApplicationConfiguration applicationConfiguration)
    {
        return Directory.Exists(applicationConfiguration.BasePath) && IsValidSolutionFile(applicationConfiguration);
    }

    private static bool IsValidSolutionFile(ApplicationConfiguration applicationConfiguration)
    {
        return !applicationConfiguration.SolutionFile.IsAssigned() || SolutionFileExists(applicationConfiguration);
    }

    private static bool SolutionFileExists(ApplicationConfiguration applicationConfiguration)
    {
        return File.Exists(GetSolutionFileName(applicationConfiguration));
    }

    private static void ReportInvalidArguments(ApplicationConfiguration applicationConfiguration)
    {
        if (!Directory.Exists(applicationConfiguration.BasePath))
        {
            $"not found : path - '{applicationConfiguration.BasePath}'".WriteLine();

            return;
        }

        $"not found : solution file - '{GetReportedSolutionFileName(applicationConfiguration)}'".WriteLine();
    }

    private static string GetReportedSolutionFileName(ApplicationConfiguration applicationConfiguration)
    {
        string solutionName = Path.GetFileNameWithoutExtension(applicationConfiguration.SolutionFile);

        return Path.Combine(applicationConfiguration.BasePath, $"{solutionName}.sln(x)");
    }

    private static string GetSolutionFileName(ApplicationConfiguration applicationConfiguration)
    {
        return Path.Combine(applicationConfiguration.BasePath, applicationConfiguration.SolutionFile);
    }

    private static void ReportIgnoredArguments(List<string> ignoredArguments)
    {
        string ignored = string.Join(", ", ignoredArguments.Select(argument => $"'{argument}'"));

        $"ignored : {ignored} - arguments start with --, see --{Arguments.Help}".WriteLine();
    }

    private static void WriteHelpHint()
    {
        $"use --{Arguments.Help} to show the available arguments".WriteLine();
    }

    private static void WriteUsage()
    {
        new string('=', 78).WriteLine();

        "reports project/package references a project already gets another way : through a project".WriteLine();
        "it references, or through another package it references".WriteLine();
        string.Empty.WriteLine();
        "package dependencies are read from the restore output, so packages a project gets through".WriteLine();
        "another package are only checked once that project has been restored".WriteLine();
        string.Empty.WriteLine();

        $"valid arguments : [--{Arguments.BasePath}={Arguments.BasePath}] [--{Arguments.Solution}={Arguments.Solution}] [--{Arguments.AllProjects}] [--{Arguments.Help}]".WriteLine();

        $"{Arguments.BasePath} : path to work from, defaults to the current folder".WriteLine();
        $"{Arguments.Solution} : the solutionfile to parse to get the .csproj files, '.sln' and '.slnx' are".WriteLine();
        $"{new string(' ', Arguments.Solution.ToString().Length)}   both accepted and the extension is optional. Without it the solution".WriteLine();
        $"{new string(' ', Arguments.Solution.ToString().Length)}   named after the {Arguments.BasePath} folder is used".WriteLine();
        $"{Arguments.AllProjects} : switch, scans every .csproj below {Arguments.BasePath} instead, ignoring any solution".WriteLine();
        $"{Arguments.Help} : switch, shows this text and exits".WriteLine();

        new string('=', 78).WriteLine();
    }
}
