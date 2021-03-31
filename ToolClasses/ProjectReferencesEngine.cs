using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolClasses.Solutions;

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

            var projectNames = _solutionReader.IsAssigned()
                ? _solutionReader.GetFullProjectNames()
                : _projectReader.GetFullProjectNames();

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

                return;
            }

            using var sw = new StreamWriter(@".\redundant.txt");

            "Redundant project references:".WriteLine();

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

            "See => 'redundant.txt'".WriteLine();
        }
    }
}
