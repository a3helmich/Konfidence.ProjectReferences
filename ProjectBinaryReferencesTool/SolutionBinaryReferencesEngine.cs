using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ToolModules;
using ToolModules.ExtensionMethods;

namespace ProjectBinaryReferencesTool
{
    public class SolutionBinaryReferencesEngine
    {
        private readonly SolutionReader _solutionReader = new SolutionReader();
        private Dictionary<string, ProjectType> _solutionProjectTypes;

        public void Execute(string solutionFile)
        {
            var solution = _solutionReader.ReadSolution(solutionFile);

            _solutionProjectTypes = SolutionProjectTypes.ProjectTypes;

            ShowAllProjectTypesInSolution(solution);
        }

        private void ShowAllProjectTypesInSolution(Solution solution)
        {
            var listOfProjects = new List<string>();

            foreach (var solutionLine in solution.SolutionLines)
            {
                if (solutionLine.StartsWith("Project(\""))
                {
                    var projectString = solutionLine.GetProjectString();

                    listOfProjects.Add(projectString);
                }
            }

            listOfProjects = listOfProjects.Distinct().ToList();

            Console.WriteLine("ProjectTypes in solution");
            Console.WriteLine(string.Empty);

            foreach (var project in listOfProjects)
            {
                if (_solutionProjectTypes.TryGetValue(project, out var projectType))
                {
                    Console.WriteLine($"\t{_solutionProjectTypes[project].Name}");
                    continue;
                }

                Console.WriteLine($"\t unknown projectGuid {project}");

            }
        }
    }
}
