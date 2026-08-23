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

        WriteHelpHint();

        if (HasValidArguments(applicationConfiguration))
        {
            return true;
        }

        ReportInvalidArguments(applicationConfiguration);

        return false;
    }

    private static bool HasValidArguments(ApplicationConfiguration applicationConfiguration)
    {
        if (!Directory.Exists(applicationConfiguration.BasePath))
        {
            return false;
        }

        if (applicationConfiguration.SolutionFile.IsAssigned())
        {
            return SolutionFileExists(applicationConfiguration);
        }

        return true;
    }

    private static void ReportInvalidArguments(ApplicationConfiguration applicationConfiguration)
    {
        if (!Directory.Exists(applicationConfiguration.BasePath))
        {
            $"not found : path - '{applicationConfiguration.BasePath}'".WriteLine();

            return;
        }

        if (applicationConfiguration.SolutionFile.IsAssigned())
        {
            $"not found : solution file - '{GetSolutionFileName(applicationConfiguration)}'".WriteLine();

            return;
        }

        WriteUsage();
    }

    private static bool SolutionFileExists(ApplicationConfiguration applicationConfiguration)
    {
        return File.Exists(GetSolutionFileName(applicationConfiguration));
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

        $"{Arguments.BasePath} : path where to look for .csproj files, recursively, if no .sln file is specified the first .sln file found is parsed".WriteLine();
        $"{Arguments.Solution} : the solutionfile [with path] to parse to get the .csproj files".WriteLine();
        $"{Arguments.AllProjects} : switch, scans every .csproj below {Arguments.BasePath}, ignoring any solution file".WriteLine();
        $"{Arguments.Help} : switch, shows this text and exits".WriteLine();

        new string('=', 78).WriteLine();
    }
}
