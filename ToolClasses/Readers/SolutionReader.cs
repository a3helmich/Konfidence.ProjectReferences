using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Konfidence.Base;
using Konfidence.MsBuild;
using Konfidence.MsBuild.Solution;

namespace ToolClasses.Readers;

public class SolutionReader
{
    private const string ProjectExtension = ".csproj";

    private readonly ApplicationConfiguration _applicationConfiguration;

    public SolutionReader(ApplicationConfiguration applicationConfiguration)
    {
        _applicationConfiguration = applicationConfiguration;
    }

    public List<string> GetFullProjectNames()
    {
        string solutionFileName = GetSolutionFileName();

        if (!SolutionFileExists(solutionFileName))
        {
            return [];
        }

        SolutionDocument solution = SolutionDocument.GetSolutionDocument(solutionFileName);

        string solutionPath = Path.GetDirectoryName(solutionFileName) ?? string.Empty;

        List<string> fullProjectNames = solution
            .Projects
            .Where(IsDotNetProject)
            .Select(solutionProject => Path.GetFullPath(Path.Combine(solutionPath, solutionProject.ProjectFile)))
            .ToList();

        return fullProjectNames;
    }

    private bool SolutionFileExists(string solutionFileName)
    {
        return _applicationConfiguration.SolutionFile.IsAssigned() && File.Exists(solutionFileName);
    }

    private string GetSolutionFileName()
    {
        return Path.Combine(_applicationConfiguration.BasePath, _applicationConfiguration.SolutionFile);
    }

    private static bool IsDotNetProject(SolutionProject solutionProject)
    {
        return Path.GetExtension(solutionProject.ProjectFile).Equals(ProjectExtension, StringComparison.OrdinalIgnoreCase);
    }
}
