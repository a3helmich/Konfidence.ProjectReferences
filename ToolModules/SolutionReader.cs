using System;
using System.Collections.Generic;
using System.Text;
using ToolModules.ExtensionMethods;

namespace ToolModules
{
    public class SolutionReader
    {
        public Solution ReadSolution(string solutionPath)
        {
            var solution = new Solution(solutionPath);

            solution.ReadSolutionLines();

            return solution;
        }
    }
}
