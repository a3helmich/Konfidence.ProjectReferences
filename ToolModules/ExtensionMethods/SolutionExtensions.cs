using System;
using System.IO;
using System.Linq;
using ToolInterfaces;

namespace ToolModules.ExtensionMethods
{
    public static class SolutionExtensions
    {
        public static void ReadSolutionLines(this ISolution solution)
        {
            using var sr = new StreamReader(solution.Name);

            string line;

            while (!(line = sr.ReadLine()).IsEof())
            {
                solution.SolutionLines.Add(line.Trim());
            }
        }

        public static string GetProjectIdString(this string line)
        {
            var lineParts = line
                .Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)
                .TrimList();

            return lineParts[0].TrimStart("Project(\"").TrimEnd("\")");
        }

        public static string GetProjectString(this string line)
        {
            var lineParts = line
                .Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)
                .TrimList();

            return lineParts[1];//.TrimStart("Project(\"").TrimEnd("\")");
        }
    }
}
