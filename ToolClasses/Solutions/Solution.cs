using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using ToolInterfaces;

namespace ToolClasses.Solutions
{
    public class Solution : ISolution
    {
        public List<ISolutionProject> SolutionProjects { get; }

        public string SolutionFile { get; }

        public List<string> SolutionLines { get; set; }

        public List<string> ProjectLines { get; set; }

        public string SolutionPath { get; }

        public Solution([NotNull] string solutionFile)
        {
            SolutionPath = Path.GetDirectoryName(solutionFile);

            SolutionFile = Path.GetFileName(solutionFile);

            SolutionLines = new List<string>();

            SolutionProjects = new List<ISolutionProject>();
        }
    }
}
