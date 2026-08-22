using System;
using System.IO;
using System.Linq;
using Konfidence.Base;
using Microsoft.Extensions.Configuration;
using ToolInterfaces;

namespace ToolClasses;

public class ApplicationConfiguration
{
    public string BasePath { get; private set; }

    public string SolutionFile { get; set; }

    public bool Verbose { get; private set; }

    /// <summary>
    /// commandline arguments example: --BasePath "C:\Projects\DayTradingServices.FakeFork" --Verbose --solution "ProjectReferences"
    /// Valueless switches such as --Verbose must be expanded with ExpandSwitchArguments before
    /// they reach AddCommandLine, otherwise they swallow the argument that follows them.
    /// </summary>
    /// <param name="configuration"></param>

    public ApplicationConfiguration(IConfiguration configuration)
    {
        BasePath = configuration.GetValue(nameof(Arguments.BasePath), ".");

        Verbose = configuration.GetValue(nameof(Arguments.Verbose), false);

        SolutionFile = configuration.GetValue(nameof(Arguments.Solution), string.Empty);

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

        string[] files = Directory.GetFiles(BasePath, "*.sln", SearchOption.TopDirectoryOnly) ?? [];

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

        SolutionFile = files.First();
    }
}
