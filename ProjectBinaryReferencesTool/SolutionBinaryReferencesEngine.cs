using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JetBrains.Annotations;
using ToolInterfaces;
using ToolModules;
using ToolModules.ExtensionMethods;

namespace ProjectBinaryReferencesTool
{
    public class SolutionBinaryReferencesEngine
    {
        private readonly SolutionReader _solutionReader = new SolutionReader();
        private Dictionary<string, ProjectType> _solutionProjectTypes;
        private Dictionary<string, ProjectType> _solutionProjectTypesByName;

        public void Execute(string solutionFile)
        {
            var solution = _solutionReader.ReadSolution(solutionFile);

            _solutionProjectTypes = SolutionProjectTypes.ProjectTypes;
            _solutionProjectTypesByName = SolutionProjectTypes.ProjectTypesByName;

            ShowAllProjectTypesInSolution(solution);

            GetAllProjectsFromSolution(solution);
        }

        private void GetAllProjectsFromSolution(Solution solution)
        {
            var listOfProjects = new List<IDotNetProject>();

            var csProjectTypeId = _solutionProjectTypesByName["C#"].ProjectTypeGuid;
            var cppProjectTypeId = _solutionProjectTypesByName["C++"].ProjectTypeGuid;

            var projectLines = solution
                .SolutionLines
                .Where(x => x.GetProjectIdString() == csProjectTypeId || x.GetProjectIdString() == cppProjectTypeId)
                .Select(x => BuildDotNetProject(solution.BasePath, x.GetProjectIdString(), x.GetProjectString()))
                .ToList();

            //foreach (var solutionLine in solution.SolutionLines)
            //{

            //}
        }

        [NotNull]
        private IDotNetProject BuildDotNetProject(string basePath, string projectType, string projectString)
        {
            var projectParts = projectString.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).TrimList();

            var assemblyName = projectParts[0].TrimQuotes();
            var projectName = Path.GetFullPath(Path.Combine(basePath, projectParts[1].TrimQuotes()));
            var projectId = projectParts[2].TrimQuotes();

            var project = new DotNetProject(projectName)
            {
                AssemblyName = assemblyName, 
                ProjectId = projectId,
                ProjectType = projectType
            };

            if (File.Exists(projectName))
            {
                project.ReadProjectLines();
            }
            else
            {
                $"{project.Name} is not found".WriteLine();
            }

            return project;
        }

        private void ShowAllProjectTypesInSolution(Solution solution)
        {
            var listOfProjects = new List<string>();

            foreach (var solutionLine in solution.SolutionLines)
            {
                if (solutionLine.StartsWith("Project(\""))
                {
                    var projectString = solutionLine.GetProjectIdString();

                    listOfProjects.Add(projectString);
                }
            }

            listOfProjects = listOfProjects.Distinct().ToList();

            "ProjectTypes in solution".WriteLine();
            string.Empty.WriteLine();

            foreach (var project in listOfProjects)
            {
                if (_solutionProjectTypes.TryGetValue(project, out var projectType))
                {
                    $"\t{_solutionProjectTypes[project].Name}".WriteLine();
                    continue;
                }

                $"\t unknown projectGuid {project}".WriteLine();

            }
        }
    }
}
