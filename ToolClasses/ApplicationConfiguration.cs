using System;
using System.IO;
using Konfidence.Base;
using Microsoft.Extensions.Configuration;
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

    public ApplicationConfiguration(
        IConfiguration configuration)
    {
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

        if (File.Exists(Path.Combine(BasePath, solutionFile)))
        {
            return solutionFile;
        }

        return $"{solutionName}{SolutionXmlExtension}";
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
