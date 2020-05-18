using System.IO;
using System.Linq;
using ToolInterfaces;

namespace ToolModules.ExtensionMethods
{
    public static class Extensions
    {
        public static string TrimStart(this string text, string trimStart)
        {
            return text.StartsWith(trimStart) ? text[trimStart.Length..].TrimStart() : text;
        }
        public static string TrimEnd(this string text, string trimEnd)
        {
            return text.EndsWith(trimEnd) ? text[..^trimEnd.Length].TrimEnd() : text;
        }

        public static void ReadProjectLines(this IDotnetProject dotnetProject)
        {
            using var sr = new StreamReader(dotnetProject.Name);

            string line;

            while (!(line = sr.ReadLine()).IsEof())
            {
                dotnetProject.ProjectLines.Add(line);
            }
        }

        public static void SetIsSdkProject(this IDotnetProject dotnetProject)
        {
            const string project = @"<project ";
            const string sdk = @"sdk=";

            var projectLines = dotnetProject.ProjectLines;

            dotnetProject.IsSdkProject = projectLines
                .FirstOrDefault(line => line.ToLowerInvariant().StartsWith(project) 
                                        && line.ToLowerInvariant().TrimStart(project).StartsWith(sdk))
                .IsAssigned();
        }
    }
}
