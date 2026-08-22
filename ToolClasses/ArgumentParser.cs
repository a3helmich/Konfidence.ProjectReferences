using System.IO;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses;

public class ArgumentParser
{
    public bool ValidateArguments(ApplicationConfiguration applicationConfiguration)
    {
        if (!applicationConfiguration.SolutionFile.IsAssigned()
            && Directory.Exists(applicationConfiguration.BasePath))
        {
            return true;
        }

        if (applicationConfiguration.SolutionFile.IsAssigned()
            && Directory.Exists(applicationConfiguration.BasePath)
            && File.Exists(Path.Combine(applicationConfiguration.BasePath, applicationConfiguration.SolutionFile)))
        {
            return true;
        }

        if (applicationConfiguration.BasePath.IsAssigned() && !Directory.Exists(applicationConfiguration.BasePath))
        {
            $"not found : path - '{applicationConfiguration.BasePath}'".WriteLine();

            return false;
        }

        if (applicationConfiguration.SolutionFile.IsAssigned() && !File.Exists(Path.Combine(applicationConfiguration.BasePath, applicationConfiguration.SolutionFile)))
        {
            $"not found : solution file - '{Path.Combine(applicationConfiguration.BasePath, applicationConfiguration.SolutionFile)}'".WriteLine();

            return false;
        }

        new string('=', 78).WriteLine();

        $"valid arguments : [--{Arguments.BasePath}={Arguments.BasePath}] [--{Arguments.Solution}={Arguments.Solution}] [--{Arguments.Verbose}]".WriteLine();

        $"{Arguments.BasePath} : path where to look for .csproj files, recursively".WriteLine();
        $"{Arguments.Solution} : the solutionfile [with path] to parse to get the .csproj files".WriteLine();
        $"{Arguments.Verbose} : switch, reports what the tool is doing".WriteLine();

        return false;
    }
}