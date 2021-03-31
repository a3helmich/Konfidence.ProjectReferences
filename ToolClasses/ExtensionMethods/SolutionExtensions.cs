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
        [NotNull]
        public static ISolution ReadSolutionLines([NotNull] this ISolution solution)
        {
            using var sr = new StreamReader(Path.Combine(solution.SolutionPath, solution.SolutionFile));

            string line;

            while (!(line = sr.ReadLine()).IsEof())
            {
                solution.SolutionLines.Add(line.Trim());
            }

            return solution;
        }

        [NotNull]
        public static ISolution BuildSolution([NotNull] this ISolution solution)
        {

            var validProjectTypeIds = new List<string>
            {
                VSProjectTypes.ProjectTypesByName["C#"].ProjectTypeGuid,
                VSProjectTypes.ProjectTypesByName["C++"].ProjectTypeGuid,
                VSProjectTypes.ProjectTypesByName["ASP.NET Core"].ProjectTypeGuid
            };

            solution.ProjectLines = solution
                .SolutionLines
                .Where(x => x.StartsWith("Project", StringComparison.OrdinalIgnoreCase))
                .Where(projectLine => validProjectTypeIds.Any(projectTypeId => projectLine.GetProjectTypeId() == projectTypeId))
                .ToList();

            return solution;
        }

        [NotNull]
        private static string GetProjectTypeId([NotNull] this string solutionProjectLine)
        {
            var solutionProjectLineParts = solutionProjectLine.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).TrimList();

            return solutionProjectLineParts.GetProjectTypeIdString();
        }

        [NotNull]
        public static ISolution BuildSolutionProjects([NotNull] this ISolution solution)
        {
            var solutionProjects = solution.ProjectLines
                .Select(x => x.BuildSolutionProject())
                .ToList();

            solution.SolutionProjects.AddRange(solutionProjects);

            return solution;
        }

        [NotNull]
        public static ISolution BuildSolutionProjectsFullName([NotNull] this ISolution solution)
        {
            var solutionDirectory = solution.SolutionPath;// Path.GetDirectoryName(solution.SolutionFile) ?? string.Empty;

            var currentDirectory = Directory.GetCurrentDirectory();

            Directory.SetCurrentDirectory(solutionDirectory);

            solution.SolutionProjects.ForEach(x => x.ProjectFileName = Path.GetFullPath(x.ProjectFileName));

            Directory.SetCurrentDirectory(currentDirectory);

            return solution;
        }

        [NotNull]
        public static ISolution BuildDotNetProjects([NotNull] this ISolution solution)
        {

            return solution;
        }

        [NotNull]
        public static SolutionProject BuildSolutionProject([NotNull] this string solutionProjectLine)
        {
            var solutionProject = new SolutionProject();

            var solutionProjectLineParts = solutionProjectLine.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).TrimList();

            solutionProject.ProjectTypeId = solutionProjectLineParts.GetProjectTypeIdString();
            solutionProject.ProjectId = solutionProjectLineParts.GetProjectIdString();
            solutionProject.ProjectName = solutionProjectLineParts.GetProjectName();
            solutionProject.ProjectFileName = solutionProjectLineParts.GetProjectFileName();

            return solutionProject;
        }

        [NotNull]
        private static string GetProjectTypeIdString([NotNull] this List<string> projectLineParts)
        {
            var lineParts = projectLineParts[0]
                .Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)
                .TrimList();

            var projectTypeId = lineParts[0].TrimStart("Project(").TrimEnd(")").TrimQuotes();

            return projectTypeId.IsGuid() ? projectTypeId : string.Empty;
        }

        [NotNull]
        private static string GetProjectIdString([NotNull] this List<string> projectLineParts)
        {
            var projectId = projectLineParts.Last().TrimQuotes();

            return projectId.IsGuid() ? projectId : string.Empty;
        }

        [NotNull]
        private static string GetProjectName([NotNull] this List<string> projectLineParts)
        {
            var lineParts = projectLineParts[0]
                .Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)
                .TrimList();

            var projectName = lineParts.Last().TrimQuotes();

            return projectName;
        }

        [NotNull]
        private static string GetProjectFileName([NotNull] this List<string> projectLineParts)
        {
            var projectFileName = projectLineParts[1].TrimQuotes();
            
            return projectFileName;
        }
    }
}
