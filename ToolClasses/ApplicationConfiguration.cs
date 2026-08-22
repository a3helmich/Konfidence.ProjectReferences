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

    public ApplicationConfiguration(IConfiguration configuration)
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

        if (SolutionFile.IsAssigned())
        {
            if (SolutionFile.EndsWith(".sln"))
            {
                return;
            }

            if (File.Exists(Path.Combine(BasePath, $"{SolutionFile}.sln")))
            {
                SolutionFile = $"{SolutionFile}.sln";

                return;
            }

            SolutionFile = string.Empty;
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
}
