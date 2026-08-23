using System.Collections.Generic;
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

        if (applicationConfiguration.SolutionFile.IsAssigned())
        {
            Execute();
        }
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
