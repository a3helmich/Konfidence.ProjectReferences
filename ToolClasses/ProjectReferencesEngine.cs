using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolClasses.Readers;
using ToolInterfaces;

namespace ToolClasses;

public class ProjectReferencesEngine
{
    private readonly ApplicationConfiguration _applicationConfiguration;

    private readonly ProjectReader _projectReader;

    private readonly ProjectNames _projectNames;

    private readonly RedundancyReport _redundancyReport;

    public ProjectReferencesEngine(
        ApplicationConfiguration applicationConfiguration,
        ProjectReader projectReader,
        ProjectNames projectNames,
        RedundancyReport redundancyReport)
    {
        _applicationConfiguration = applicationConfiguration;
        _projectReader = projectReader;
        _projectNames = projectNames;
        _redundancyReport = redundancyReport;
    }

    public async Task Execute()
    {
        if (!_applicationConfiguration.ValidConfiguration())
        {
            return;
        }

        List<string> projectNames = await _projectNames.GetFullProjectNames();

        _projectReader
            .Execute(projectNames)
            .ExtendProjectsWithProjectReferences()
            .ExtendProjectsWithAllSubProjectReferences()
            .ExtendProjectsWithAllSubPackageReferences()
            .ExtendProjectsWithAllRedundantProjectReferences()
            .ExtendProjectsWithAllRedundantPackageReferences();

        ReportMissingPackageReferences();

        await _redundancyReport.Write(GetProjectsWithRedundantReferences());
    }

    private List<IDotNetProject> GetProjectsWithRedundantReferences()
    {
        return _projectReader
            .SdkProjects
            .Where(sdkProject => sdkProject.RedundantReferencedProjects.Any() || sdkProject.RedundantPackageReferences.Any())
            .ToList();
    }

    private void ReportMissingPackageReferences()
    {
        int projectsWithoutPackageReferences = _projectReader.SdkProjects.Count(sdkProject => sdkProject.PackageReferencesMissing);

        if (projectsWithoutPackageReferences > 0)
        {
            $"note : {projectsWithoutPackageReferences} project(s) have no restore output, package dependencies were not checked for them".WriteLine();
        }
    }
}
