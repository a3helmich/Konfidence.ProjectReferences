using System;
using System.Collections.Generic;
using System.IO;
using Konfidence.Base;
using Microsoft.Extensions.Configuration;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses;

public class ApplicationConfiguration
{
    private const string SolutionExtension = ".sln";

    private const string SolutionXmlExtension = ".slnx";

    public string BasePath { get; }

    public string SolutionFile { get; }

    public bool AllProjects { get; }

    public bool Help { get; }

    public List<string> IgnoredArguments { get; }

    public ApplicationConfiguration(
        IConfiguration configuration,
        string[] commandLineArguments)
    {
        IgnoredArguments = commandLineArguments.GetIgnoredArguments();

        BasePath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(configuration.GetValue(nameof(Arguments.BasePath), Directory.GetCurrentDirectory())));

        AllProjects = configuration.GetValue(nameof(Arguments.AllProjects), false);

        Help = configuration.GetValue(nameof(Arguments.Help), false);

        SolutionFile = configuration.GetValue(nameof(Arguments.Solution), string.Empty);

        if (Help)
        {
            return;
        }

        if (AllProjects)
        {
            SolutionFile = string.Empty;

            return;
        }

        if (SolutionFile.IsAssigned())
        {
            SolutionFile = ResolveSolutionFile(SolutionFile);

            return;
        }

        string topPath = Path.GetFileName(BasePath);

        if (topPath.IsAssigned())
        {
            SolutionFile = ResolveSolutionFile(topPath);
        }
    }

    private string ResolveSolutionFile(string solutionName)
    {
        if (HasSolutionExtension(solutionName))
        {
            return solutionName;
        }

        string solutionFile = $"{solutionName}{SolutionExtension}";

        return File.Exists(Path.Combine(BasePath, solutionFile))
            ? solutionFile
            : $"{solutionName}{SolutionXmlExtension}";
    }

    private static bool HasSolutionExtension(string solutionName)
    {
        return solutionName.EndsWith(SolutionExtension, StringComparison.OrdinalIgnoreCase)
               || solutionName.EndsWith(SolutionXmlExtension, StringComparison.OrdinalIgnoreCase);
    }

    public bool ValidConfiguration()
    {
        return ArgumentParser.ValidateArguments(this);
    }
}
