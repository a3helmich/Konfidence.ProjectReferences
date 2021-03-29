using System.Collections.Generic;

namespace ToolInterfaces
{
    public interface ISolution
    {
        List<ISolutionProject> SolutionProjects { get; }

        string SolutionFile { get; }

        string SolutionPath { get; }

        List<string> SolutionLines { get; set; }

        List<string> ProjectLines { get; set; }
    }
}
