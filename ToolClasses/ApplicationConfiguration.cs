using System.IO;
using System.Linq;
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
        BasePath = configuration.GetValue(nameof(Arguments.BasePath), Directory.GetCurrentDirectory());

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

        // a named solution is taken as given, extension or not: whether it exists is the argument
        // parser's call, so it can report a solution file it cannot find rather than quietly
        // falling back to whatever solution happens to sit in the base path
        if (SolutionFile.IsAssigned())
        {
            SolutionFile = SolutionFile.EndsWith(".sln") ? SolutionFile : $"{SolutionFile}.sln";

            return;
        }

        string[] files = Directory.GetFiles(BasePath, "*.sln", SearchOption.TopDirectoryOnly);

        if (!files.Any())
        {
            return;
        }

        string topPath = Path.GetFileNameWithoutExtension(BasePath);

        if (topPath.IsAssigned() && files.Contains(Path.Combine(BasePath, $"{topPath}.sln")))
        {
            SolutionFile = $"{topPath}.sln";

            return;
        }

        SolutionFile = Path.GetFileName(files.First());
    }

    public bool ValidConfiguration()
    {
        return ArgumentParser.ValidateArguments(this);
    }
}
