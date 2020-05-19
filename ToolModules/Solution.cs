using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using ToolInterfaces;

namespace ToolModules
{
    public class Solution : ISolution
    {
        public string Name { get; }

        public List<string> SolutionLines { get; set; }
        public string BasePath { get; set; }

        public Solution(string name)
        {
            SolutionLines = new List<string>();

            Name = name;
        }
    }
}
