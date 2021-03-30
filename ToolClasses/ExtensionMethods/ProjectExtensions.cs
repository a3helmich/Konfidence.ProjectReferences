using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolInterfaces;

namespace ToolClasses.ExtensionMethods
{
    [UsedImplicitly]
    internal static class ProjectExtensions
    {
        [NotNull]
        public static IDotNetProject BuildDotnetProject([NotNull] this IDotNetProject dotNetProject)
        {
            dotNetProject
                .ReadProjectLines()
                .SetProjectProperties()
                .BuildProjectReferences();

            return dotNetProject;
        }

        [NotNull]
        internal static IDotNetProject ReadProjectLines([NotNull] this IDotNetProject dotNetProject)
        {
            using var sr = new StreamReader(dotNetProject.FileName);

            string line;

            while (!(line = sr.ReadLine()).IsEof())
            {
                dotNetProject.ProjectLines.Add(line.Trim());
            }

            return dotNetProject;
        }

        [NotNull]
        internal static IDotNetProject SetProjectProperties([NotNull] this IDotNetProject dotNetProject)
        {
            const string project = @"<project ";
            const string sdk = @"sdk=";

            var projectLines = dotNetProject.ProjectLines;

            dotNetProject.IsSdkProject = projectLines
                .Where(line => line.StartsWith(project, StringComparison.OrdinalIgnoreCase))
                .Select(line => line.TrimStartIgnoreCase(project))
                .Any(line => line.StartsWith(sdk, StringComparison.OrdinalIgnoreCase));

            var assemblyNameLine = dotNetProject.ProjectLines
                .FirstOrDefault(line => line.StartsWith(@"<AssemblyName>", StringComparison.OrdinalIgnoreCase));

            dotNetProject.AssemblyName = assemblyNameLine.IsAssigned() ? assemblyNameLine.TrimStart("<AssemblyName>").TrimEnd("</AssemblyName>") : string.Empty;

            return dotNetProject;
        }

        [NotNull]
        internal static IDotNetProject BuildProjectReferences([NotNull] this IDotNetProject dotNetProject)
        {
            var projectPath = Path.GetDirectoryName(dotNetProject.FileName) ?? string.Empty;

            var projectReferences = dotNetProject.ProjectLines
                .Where(line => line.StartsWith(@"<ProjectReference Include=", StringComparison.OrdinalIgnoreCase))
                .ToList();

            dotNetProject.ProjectReferences.AddRange(projectReferences
                .Select(line => line.TrimStartIgnoreCase(@"<ProjectReference Include="))
                .Select(line => line.TrimEnd(@"/>").TrimEnd(@">"))
                .Select(line => line.TrimQuotes())
                .Select(projectName => Path.GetFullPath(Path.Combine(projectPath, projectName))));

            return dotNetProject;
        }

        [NotNull]
        public static IEnumerable<IDotNetProject> GetSubProjectReferences([NotNull] this IDotNetProject dotNetProject)
        {
            var subProjectReferences = new List<IDotNetProject>();

            foreach (var referencedSdkProject in dotNetProject.ReferencedProjects)
            {
                subProjectReferences.AddRange(referencedSdkProject.ReferencedProjects);

                if (referencedSdkProject.SubProjectReferencesResolved)
                {
                    continue;
                }

                subProjectReferences.AddRange(referencedSdkProject.GetSubProjectReferences());
            }

            dotNetProject.SubProjectReferencesResolved = true;

            return subProjectReferences.Distinct().ToList();
        }

        [NotNull]
        public static IEnumerable<string> GetBinaryReferences([NotNull] this IDotNetProject dotNetProject)
        {
            var binaryReferences = dotNetProject.ProjectLines
                .Where(line => line.StartsWith(@"<Reference Include=", StringComparison.OrdinalIgnoreCase) && line.EndsWith(">") && !line.EndsWith("/>"))
                .ToList();

            binaryReferences = binaryReferences
                .Select(line => line.TrimStartIgnoreCase(@"<Reference Include="))
                .Select(line => line.TrimEnd(@">"))
                .Select(line => line.TrimQuotes())
                .ToList();

            return binaryReferences;
        }
    }
}
