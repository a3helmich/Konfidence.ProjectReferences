using System.Collections.Generic;
using System.IO;
using System.Linq;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses.Solutions;

public class SolutionReader
{
    private readonly ISolution _solution;

    public SolutionReader(ISolution solution, ApplicationConfiguration applicationConfiguration)
    {
        _solution = solution;

        if (SolutionFileExists(applicationConfiguration))
        {
            Execute();
        }
    }

    private static bool SolutionFileExists(ApplicationConfiguration applicationConfiguration)
    {
        return applicationConfiguration.SolutionFile.IsAssigned()
               && File.Exists(Path.Combine(applicationConfiguration.BasePath, applicationConfiguration.SolutionFile));
    }

    private void Execute()
    {
        _solution
            .ReadSolutionLines()
            .BuildSolution()
            .BuildSolutionProjects()
            .BuildSolutionProjectsFullName();
    }

    public List<string> GetFullProjectNames()
    {
        return [.. _solution.SolutionProjects.Select(x => x.ProjectFileName)];
    }
}
