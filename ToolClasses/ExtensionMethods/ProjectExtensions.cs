using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolInterfaces;

namespace ToolClasses.ExtensionMethods;

[UsedImplicitly]
internal static class ProjectExtensions
{
    extension(IDotNetProject dotNetProject)
    {
        public IDotNetProject BuildDotnetProject()
        {
            dotNetProject
                .ReadProjectLines()
                .SetProjectProperties()
                .BuildProjectReferences();

            return dotNetProject;
        }

        private IDotNetProject ReadProjectLines()
        {
            using StreamReader sr = new(dotNetProject.FileName);

            string? line;

            while (!(line = sr.ReadLine()).IsEof())
            {
                dotNetProject.ProjectLines.Add(line.Trim());
            }

            return dotNetProject;
        }

        private IDotNetProject SetProjectProperties()
        {
            const string project = @"<project ";
            const string sdk = @"sdk=";

            List<string> projectLines = dotNetProject.ProjectLines;

            dotNetProject.IsSdkProject = projectLines
                .Where(line => line.StartsWith(project, StringComparison.OrdinalIgnoreCase))
                .Select(line => line.TrimStartIgnoreCase(project))
                .Any(line => line.StartsWith(sdk, StringComparison.OrdinalIgnoreCase));

            return dotNetProject;
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private IDotNetProject BuildProjectReferences()
        {
            string projectPath = Path.GetDirectoryName(dotNetProject.FileName) ?? string.Empty;

            List<string> projectReferences = dotNetProject.ProjectLines
                .Where(line => line.StartsWith(@"<ProjectReference Include=", StringComparison.OrdinalIgnoreCase))
                .ToList();

            dotNetProject.ProjectReferences.AddRange(projectReferences
                .Select(line => line.TrimStartIgnoreCase(@"<ProjectReference Include="))
                .Select(line => line.TrimEnd(@"/>").TrimEnd(@">"))
                .Select(line => line.TrimQuotes())
                .Select(projectName => Path.GetFullPath(Path.Combine(projectPath, projectName))));

            return dotNetProject;
        }

        public IEnumerable<IDotNetProject> GetSubProjectReferences()
        {
            List<IDotNetProject> subProjectReferences = [];

            foreach (IDotNetProject referencedSdkProject in dotNetProject.ReferencedProjects)
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
    }
}