using System.Collections.Generic;
using System.Linq;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses.Solutions;

public class SolutionReader
{
    private readonly ISolution _solution;

    public SolutionReader(ISolution solution)
    {
        _solution = solution;
    }

    public void Execute()
    {
        _solution
            .ReadSolutionLines()
            .BuildSolution()
            .BuildSolutionProjects()
            .BuildSolutionProjectsFullName()
            .BuildDotNetProjects();
    }

    public List<string> GetFullProjectNames()
    {
        return _solution.IsAssigned()
            ? [.. _solution.SolutionProjects.Select(x => x.ProjectFileName)]
            : [];
    }
}