using System.Collections.Generic;
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

    public List<string> GetFullProjectNames()
    {
        return ProjectNamesFromSolutionValid()
            ? _solutionReader.GetFullProjectNames()
            : _projectReader.GetFullProjectNames();
    }
}
