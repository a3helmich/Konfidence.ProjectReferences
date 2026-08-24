using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Konfidence.Base;
using Microsoft.VisualStudio.SolutionPersistence;
using Microsoft.VisualStudio.SolutionPersistence.Model;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;

namespace ToolClasses.Solutions;

public class SolutionReader
{
    private const string ProjectExtension = ".csproj";

    private readonly ApplicationConfiguration _applicationConfiguration;

    public SolutionReader(ApplicationConfiguration applicationConfiguration)
    {
        _applicationConfiguration = applicationConfiguration;
    }

    public async Task<List<string>> GetFullProjectNames()
    {
        string solutionFileName = GetSolutionFileName();

        if (!SolutionFileExists(solutionFileName))
        {
            return [];
        }

        ISolutionSerializer? serializer = SolutionSerializers.GetSerializerByMoniker(solutionFileName);

        if (!serializer.IsAssigned())
        {
            return [];
        }

        SolutionModel solution = await serializer.OpenAsync(solutionFileName, CancellationToken.None);

        string solutionPath = Path.GetDirectoryName(solutionFileName) ?? string.Empty;

        List<string> fullProjectNames = solution
            .SolutionProjects
            .Where(IsDotNetProject)
            .Select(solutionProject => Path.GetFullPath(Path.Combine(solutionPath, solutionProject.FilePath)))
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

    private static bool IsDotNetProject(SolutionProjectModel solutionProject)
    {
        return solutionProject.Extension.Equals(ProjectExtension, StringComparison.OrdinalIgnoreCase);
    }
}
