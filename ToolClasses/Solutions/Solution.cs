using System.Collections.Generic;
using ToolInterfaces;

namespace ToolClasses.Solutions;

public class Solution : ISolution
{
    public List<ISolutionProject> SolutionProjects { get; } = [];

    public string SolutionFile { get; }

    public string SolutionPath { get; }

    public List<string> SolutionLines { get; set; } = [];

    public List<string> ProjectLines { get; set; } = [];

    public Solution(ApplicationConfiguration applicationConfiguration)
    {
        SolutionPath = applicationConfiguration.BasePath;
        SolutionFile = applicationConfiguration.SolutionFile;
    }
}