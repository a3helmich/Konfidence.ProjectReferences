using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolClasses.Solutions;
using ToolInterfaces;

namespace ToolClasses.ExtensionMethods
{
    [UsedImplicitly]
    internal static class SolutionExtensions
    {
        extension([NotNull] ISolution solution)
        {
            [NotNull]
            public ISolution ReadSolutionLines()
            {
                using StreamReader sr = new(Path.Combine(solution.SolutionPath, solution.SolutionFile));

                string line;

                while (!(line = sr.ReadLine()).IsEof())
                {
                    solution.SolutionLines.Add(line.Trim());
                }

                return solution;
            }

            [NotNull]
            public ISolution BuildSolution()
            {

                List<string> validProjectTypeIds =
                [
                    VSProjectTypes.ProjectTypesByName["C#"].ProjectTypeGuid,
                    VSProjectTypes.ProjectTypesByName["C++"].ProjectTypeGuid,
                    VSProjectTypes.ProjectTypesByName["ASP.NET Core"].ProjectTypeGuid
                ];

                solution.ProjectLines = solution
                    .SolutionLines
                    .Where(x => x.StartsWith("Project", StringComparison.OrdinalIgnoreCase))
                    .Where(projectLine => validProjectTypeIds.Any(projectTypeId => projectLine.GetProjectTypeId() == projectTypeId))
                    .ToList();

                return solution;
            }
        }

        [NotNull]
        private static string GetProjectTypeId([NotNull] this string solutionProjectLine)
        {
            List<string> solutionProjectLineParts = solutionProjectLine.Split([","], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            return solutionProjectLineParts.GetProjectTypeIdString();
        }

        extension([NotNull] ISolution solution)
        {
            [NotNull]
            public ISolution BuildSolutionProjects()
            {
                List<SolutionProject> solutionProjects = solution.ProjectLines
                    .Select(x => x.BuildSolutionProject())
                    .ToList();

                solution.SolutionProjects.AddRange(solutionProjects);

                return solution;
            }

            [NotNull]
            public ISolution BuildSolutionProjectsFullName()
            {
                string solutionDirectory = solution.SolutionPath;// Path.GetDirectoryName(solution.SolutionFile) ?? string.Empty;

                string currentDirectory = Directory.GetCurrentDirectory();

                Directory.SetCurrentDirectory(solutionDirectory);

                solution.SolutionProjects.ForEach(x => x.ProjectFileName = Path.GetFullPath(x.ProjectFileName));

                Directory.SetCurrentDirectory(currentDirectory);

                return solution;
            }

            [NotNull]
            public ISolution BuildDotNetProjects()
            {

                return solution;
            }
        }

        [NotNull]
        public static SolutionProject BuildSolutionProject([NotNull] this string solutionProjectLine)
        {
            SolutionProject solutionProject = new();

            List<string> solutionProjectLineParts = solutionProjectLine.Split([","], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            solutionProject.ProjectTypeId = solutionProjectLineParts.GetProjectTypeIdString();
            solutionProject.ProjectId = solutionProjectLineParts.GetProjectIdString();
            solutionProject.ProjectName = solutionProjectLineParts.GetProjectName();
            solutionProject.ProjectFileName = solutionProjectLineParts.GetProjectFileName();

            return solutionProject;
        }

        extension([NotNull] List<string> projectLineParts)
        {
            [NotNull]
            private string GetProjectTypeIdString()
            {
                List<string> lineParts = projectLineParts[0]
                    .Split(["="], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();

                string projectTypeId = lineParts[0].TrimStart("Project(").TrimEnd(")").TrimQuotes();

                return projectTypeId.IsGuid() ? projectTypeId : string.Empty;
            }

            [NotNull]
            private string GetProjectIdString()
            {
                string projectId = projectLineParts.Last().TrimQuotes();

                return projectId.IsGuid() ? projectId : string.Empty;
            }

            [NotNull]
            private string GetProjectName()
            {
                List<string> lineParts = projectLineParts[0]
                    .Split(["="], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();

                string projectName = lineParts.Last().TrimQuotes();

                return projectName;
            }

            [NotNull]
            private string GetProjectFileName()
            {
                string projectFileName = projectLineParts[1].TrimQuotes();

                return projectFileName;
            }
        }
    }
}
