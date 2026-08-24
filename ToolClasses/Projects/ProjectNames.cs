using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Konfidence.Base;
using ToolClasses.Solutions;

namespace ToolClasses.Projects;

public class ProjectNames
{
    private readonly SolutionReader _solutionReader;

    private readonly ProjectReader _projectReader;

    private readonly ApplicationConfiguration _applicationConfiguration;

    public ProjectNames(
        SolutionReader solutionReader,
        ProjectReader projectReader,
        ApplicationConfiguration applicationConfiguration)
    {
        _solutionReader = solutionReader;
        _projectReader = projectReader;
        _applicationConfiguration = applicationConfiguration;
    }

    private bool ProjectNamesFromSolutionValid()
    {
        return _applicationConfiguration.SolutionFile.IsAssigned() && !_applicationConfiguration.AllProjects;
    }

    public async Task<List<string>> GetFullProjectNames()
    {
        if (ProjectNamesFromSolutionValid())
        {
            return await _solutionReader.GetFullProjectNames();
        }

        return GetProjectNames();
    }

    private List<string> GetProjectNames()
    {
        return Directory
            .GetFiles(_applicationConfiguration.BasePath, @"*.csproj", SearchOption.AllDirectories)
            .ToList();
    }
}
