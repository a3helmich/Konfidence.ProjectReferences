using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolClasses.Solutions;
using ToolInterfaces;

namespace ToolClasses
{
    public class ProjectReferencesEngine
    {
        private ProjectReader _projectReader;
        private SolutionReader _solutionReader;

        public void Execute([NotNull] string solutionFile, [NotNull] string basePath)
        {
            if (solutionFile.IsAssigned())
            {
                _solutionReader = new SolutionReader(Path.Combine(basePath, solutionFile));

                _solutionReader.Execute();
            }

            _projectReader = new ProjectReader(basePath);

            List<string> projectNames = _solutionReader.IsAssigned()
                ? _solutionReader.GetFullProjectNames()
                : _projectReader.GetFullProjectNames();

            _projectReader
                .Execute(projectNames)
                .ExtendProjectsWithProjectReferences()
                .ExtendProjectsWithAllSubProjectReferences()
                .ExtendProjectsWithAllRedundantProjectReferences();

            List<IDotNetProject> projectsWithRedundantReferences = _projectReader
                .SdkProjects
                .Where(x => x.RedundantReferencedProjects.Any())
                .ToList();

            string tab = new(' ', 4);

            if (!projectsWithRedundantReferences.Any())
            {
                "No redundant project references found.".WriteLine();

                return;
            }

            using StreamWriter sw = new(@".\redundant.txt");

            if (solutionFile.IsAssigned())
            {
                $"Redundant project references in solution '{solutionFile}':".WriteLine();
            }
            else
            {
                "Redundant project references:".WriteLine();
            }

            foreach (IDotNetProject projectWithRedundantReferences in projectsWithRedundantReferences)
            {
                string line = $@"{projectWithRedundantReferences.FileName.TrimStartIgnoreCase(basePath)}".WriteLine();

                sw.WriteLine(line);

                foreach (IDotNetProject redundantReferencedProject in projectWithRedundantReferences.RedundantReferencedProjects)
                {
                    line = $@"{tab} - {redundantReferencedProject.FileName.TrimStartIgnoreCase(basePath)}".WriteLine();

                    sw.WriteLine(line);
                }
            }

            "See => 'redundant.txt'".WriteLine();
        }
    }
}
