using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses.Solutions
{
    public class SolutionReader
    {
        public string SolutionPath { get; }

        public string SolutionFile { get; }

        public ISolution Solution { get; private set; }

        public SolutionReader([NotNull] string solutionFile)
        {
            SolutionPath = Path.GetDirectoryName(solutionFile);
            SolutionFile = Path.GetFullPath(solutionFile);
        }

        public void Execute()
        {
            Solution = new Solution(SolutionFile)
                .ReadSolutionLines()
                .BuildSolution()
                .BuildSolutionProjects()
                .BuildSolutionProjectsFullName()
                .BuildDotNetProjects();
        }

        [NotNull]
        public List<string> GetFullProjectNames()
        {
            return Solution.SolutionProjects.Select(x => x.ProjectFileName).ToList();
        }
    }
}
