using System.IO;
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

        if (HasValidArguments(applicationConfiguration))
        {
            WriteHelpHint();

            return true;
        }

        if (ReportInvalidArguments(applicationConfiguration))
        {
            WriteUsage();
        }

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

    private static bool ReportInvalidArguments(ApplicationConfiguration applicationConfiguration)
    {
        if (!Directory.Exists(applicationConfiguration.BasePath))
        {
            $"not found : path - '{applicationConfiguration.BasePath}'".WriteLine();

            return true;
        }

        if (applicationConfiguration.SolutionFile.IsAssigned())
        {
            $"not found : solution file - '{GetSolutionFileName(applicationConfiguration)}'".WriteLine();

            return true;
        }

        return false;
    }

    private static string GetSolutionFileName(ApplicationConfiguration applicationConfiguration)
    {
        return Path.Combine(applicationConfiguration.BasePath, applicationConfiguration.SolutionFile);
    }

    private static void WriteHelpHint()
    {
        $"use --{Arguments.Help} to show the available arguments".WriteLine();
    }

    private static void WriteUsage()
    {
        new string('=', 78).WriteLine();

        $"valid arguments : [--{Arguments.BasePath}={Arguments.BasePath}] [--{Arguments.Solution}={Arguments.Solution}] [--{Arguments.AllProjects}] [--{Arguments.Help}]".WriteLine();

        $"{Arguments.BasePath} : path to work from, defaults to the current folder".WriteLine();
        $"{Arguments.Solution} : the solutionfile to parse to get the .csproj files, the '.sln' is optional.".WriteLine();
        $"{new string(' ', Arguments.Solution.ToString().Length)}   without it the solution named after the {Arguments.BasePath} folder is used".WriteLine();
        $"{Arguments.AllProjects} : switch, scans every .csproj below {Arguments.BasePath} instead, ignoring any solution".WriteLine();
        $"{Arguments.Help} : switch, shows this text and exits".WriteLine();

        new string('=', 78).WriteLine();
    }
}
