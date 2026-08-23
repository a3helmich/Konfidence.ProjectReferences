using System.IO;
using Konfidence.Base;
using Microsoft.Extensions.Configuration;
using ToolInterfaces;

namespace ToolClasses;

public class ApplicationConfiguration
{
    public string BasePath { get; }

    public string SolutionFile { get; }

    public bool AllProjects { get; }

    public bool Help { get; }

    public ApplicationConfiguration(
        IConfiguration configuration)
    {
        BasePath = Path.TrimEndingDirectorySeparator(configuration.GetValue(nameof(Arguments.BasePath), Directory.GetCurrentDirectory()));

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
            SolutionFile = SolutionFile.EndsWith(".sln") ? SolutionFile : $"{SolutionFile}.sln";

            return;
        }

        string topPath = Path.GetFileName(BasePath);

        if (topPath.IsAssigned())
        {
            SolutionFile = $"{topPath}.sln";
        }
    }

    public bool ValidConfiguration()
    {
        return ArgumentParser.ValidateArguments(this);
    }
}
