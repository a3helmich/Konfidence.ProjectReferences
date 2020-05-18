using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ToolInterfaces;
using ToolModules;
using ToolModules.ExtensionMethods;

namespace ProjectReferencesTool
{
    public class ProjectReferencesEngine
    {
        private string _basePath = string.Empty;
        private readonly ProjectReader _projectReader = new ProjectReader();

        public void Execute(string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                Console.WriteLine($@"Path not found: {basePath}");

                return;
            }

            _basePath = basePath;

            _projectReader.ReadProjects(_basePath);

            ExtendProjectsWithReferences(_projectReader.SdkProjects, _projectReader.ProjectLookup);

            CollectAllSubReferences(_projectReader.SdkProjects);

            CollectAllRedundantReferences(_projectReader.SdkProjects);

            var smellyProjects = _projectReader.SdkProjects.Where(x => x.RedundantReferencedProjects.Any()).ToList();

            const char tab = '\t';

            using var sw = new StreamWriter(@".\redundant.txt");

            foreach (var smellyProject in smellyProjects)
            {
                Console.WriteLine($@"{smellyProject.Name.TrimStart(_basePath)}");
                Debug.WriteLine($@"{smellyProject.Name.TrimStart(_basePath)}");
                sw.WriteLine($@"{smellyProject.Name.TrimStart(_basePath)}");

                foreach (var smellyProjectRedundantReferencedProject in smellyProject.RedundantReferencedProjects)
                {
                    Console.WriteLine($@"{tab} - {smellyProjectRedundantReferencedProject.Name.TrimStart(_basePath)}");
                    Debug.WriteLine($@"{tab} - {smellyProjectRedundantReferencedProject.Name.TrimStart(_basePath)}");
                    sw.WriteLine($@"{tab} - {smellyProjectRedundantReferencedProject.Name.TrimStart(_basePath)}");
                }
            }
        }

        private static void ExtendProjectsWithReferences(IEnumerable<IDotnetProject> allProjects, Dictionary<string, IDotnetProject> projectLookup)
        {
            foreach (var project in allProjects)
            {
                var projectRefenceNames = GetProjectsReferences(project.Name, project.ProjectLines);

                foreach (var projectRefenceName in projectRefenceNames)
                {
                    if (projectLookup.TryGetValue(projectRefenceName, out var referencedProject))
                    {
                        project.ReferencedProjects.Add(referencedProject);
                    }
                }
            }
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

        private static void CollectAllSubReferences(IEnumerable<IDotnetProject> allProjectsWithReferences)
        {
            foreach (var sdkProject in allProjectsWithReferences)
            {
                var subReferences = CollectSubReferences(sdkProject);

                sdkProject.SubReferencedProjects.AddRange(subReferences);
            }
        }

        private static IEnumerable<IDotnetProject> CollectSubReferences(IDotnetProject dotnetProject)
        {
            var collection = new List<IDotnetProject>();

            foreach (var referencedSdkProject in dotnetProject.ReferencedProjects)
            {
                collection.AddRange(referencedSdkProject.ReferencedProjects);

                if (!referencedSdkProject.SubReferencesResolved)
                {
                    collection.AddRange(CollectSubReferences(referencedSdkProject));
                }
            }

            dotnetProject.SubReferencesResolved = true;

            return collection.Distinct().ToList();
        }

        private static void CollectAllRedundantReferences(IEnumerable<IDotnetProject> allProjects)
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
