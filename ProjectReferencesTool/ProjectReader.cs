using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjectReferencesTool
{
    public class ProjectReader
    {
        private static string BasePath = string.Empty;

        public void Execute(string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                Console.WriteLine($"Path not found: {basePath}");
            }

            BasePath = basePath;

            var allProjects = GetAllProjects();
            var projectLookup = allProjects.ToDictionary(project => project.Name);

            ExtendProjectsWithReferences(allProjects, projectLookup);

            CollectAllSubReferences(allProjects);

            CollectAllRedundantReferences(allProjects);

            var smellyProjects = allProjects.Where(x => x.RedundantReferencedProjects.Any()).ToList();

            const char tab = '\t';

            using var sw = new StreamWriter(@".\redundant.txt");

            foreach (var smellyProject in smellyProjects)
            {
                Console.WriteLine($@"{smellyProject.Name.TrimStart(BasePath)}");
                Debug.WriteLine($@"{smellyProject.Name.TrimStart(BasePath)}");
                sw.WriteLine($@"{smellyProject.Name.TrimStart(BasePath)}");

                foreach (var smellyProjectRedundantReferencedProject in smellyProject.RedundantReferencedProjects)
                {
                    Console.WriteLine($@"{tab} - {smellyProjectRedundantReferencedProject.Name.TrimStart(BasePath)}");
                    Debug.WriteLine($@"{tab} - {smellyProjectRedundantReferencedProject.Name.TrimStart(BasePath)}");
                    sw.WriteLine($@"{tab} - {smellyProjectRedundantReferencedProject.Name.TrimStart(BasePath)}");
                }
            }
        }

        private static List<SdkProject> GetAllProjects()
        {
            var fullPath = Path.GetFullPath(BasePath);

            var files = Directory.GetFiles(fullPath, @"*.csproj", SearchOption.AllDirectories).ToList();

            return files.Select(name => new SdkProject(name)).ToList();
        }

        private static void ExtendProjectsWithReferences(IEnumerable<SdkProject> allProjects, Dictionary<string, SdkProject> projectLookup)
        {
            foreach (var project in allProjects)
            {
                var projectLines = GetProjectLines(project.Name).ToList();

                if (!IsSdkProject(projectLines))
                {
                    continue;
                }

                var projectRefenceNames = GetProjectsReferences(project.Name, projectLines);

                foreach (var projectRefenceName in projectRefenceNames)
                {
                    if (projectLookup.TryGetValue(projectRefenceName, out var referencedProject))
                    {
                        project.ReferencedProjects.Add(referencedProject);
                    }
                }
            }
        }

        private static bool IsSdkProject(List<string> projectLines)
        {
            string project = @"<project ";
            string sdk = @"sdk=";

            var projectLine = projectLines[0].ToLowerInvariant();

            if (projectLine.StartsWith(project))
            {
                projectLine = projectLine.Substring(project.Length).TrimStart();

                if (projectLine.StartsWith(sdk))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> GetProjectLines(string project)
        {
            var inputLines = new List<string>();
            string line;

            using var sr = new StreamReader(project);

            while ((line = sr.ReadLine()) != null)
            {
                inputLines.Add(line);
            }

            return inputLines;
        }

        private static IEnumerable<string> GetProjectsReferences(string project, IEnumerable<string> projectLines)
        {
            var projectPath = Path.GetDirectoryName(project) + @"\";

            var projectReferences = projectLines
                .Select(line => line.Trim())
                .Where(line => line.StartsWith(@"<ProjectReference Include="))
                .ToList();

            projectReferences = projectReferences
                .Select(line => line.TrimStart(@"<ProjectReference Include=").TrimStart('"'))
                .Select(line => line.TrimEnd(@"/>").TrimEnd(@">").TrimEnd('"'))
                .Select(line => Path.GetFullPath(projectPath + line))
                .ToList();

            return projectReferences;
        }

        private static void CollectAllSubReferences(IEnumerable<SdkProject> allProjectsWithReferences)
        {
            foreach (var sdkProject in allProjectsWithReferences)
            {
                var subReferences = CollectSubReferences(sdkProject);

                sdkProject.SubReferencedProjects.AddRange(subReferences);
            }
        }

        private static IEnumerable<SdkProject> CollectSubReferences(SdkProject sdkProject)
        {
            var collection = new List<SdkProject>();

            foreach (var referencedSdkProject in sdkProject.ReferencedProjects)
            {
                collection.AddRange(referencedSdkProject.ReferencedProjects);

                if (!referencedSdkProject.SubReferencesResolved)
                {
                    collection.AddRange(CollectSubReferences(referencedSdkProject));
                }
            }

            sdkProject.SubReferencesResolved = true;

            return collection.Distinct().ToList();
        }

        private static void CollectAllRedundantReferences(IEnumerable<SdkProject> allProjects)
        {
            foreach (var sdkProject in allProjects)
            {
                var subReferencedProjects = sdkProject
                    .ReferencedProjects
                    .Where(referencedProject => sdkProject.SubReferencedProjects.Contains(referencedProject));

                sdkProject.RedundantReferencedProjects.AddRange(subReferencedProjects);
            }
        }
    }
}
