using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolInterfaces;

namespace ToolClasses
{
    public class ProjectReferencesEngine
    {
        private ProjectReader _projectReader;

        public void Execute(string basePath)
        {
            _projectReader = new ProjectReader(basePath);

            var projectNames = _projectReader.GetFullProjectNames();

            _projectReader
                .Execute(projectNames)
                .ExtendProjectsWithProjectReferences()
                .ExtendProjectsWithAllSubProjectReferences()
                .ExtendProjectsWithAllRedundantProjectReferences();

            var projectsWithRedundantReferences = _projectReader
                .SdkProjects
                .Where(x => x.RedundantReferencedProjects.Any())
                .ToList();

            var tab = new string(' ', 4);

            if (!projectsWithRedundantReferences.Any())
            {
                "No redundant project references found.".WriteLine();
            }

            using var sw = new StreamWriter(@".\redundant.txt");

            foreach (var projectWithRedundantReferences in projectsWithRedundantReferences)
            {
                var line = $@"{projectWithRedundantReferences.FileName.TrimStartIgnoreCase(basePath)}".WriteLine();

                sw.WriteLine(line);

                foreach (var redundantReferencedProject in projectWithRedundantReferences.RedundantReferencedProjects)
                {
                    line = $@"{tab} - {redundantReferencedProject.FileName.TrimStartIgnoreCase(basePath)}".WriteLine();

                    sw.WriteLine(line);
                }
            }
        }
    }
}
