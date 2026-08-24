using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolInterfaces;

namespace ToolClasses;

public class ProjectReferencesEngine
{
    private const string PackageExtension = ".nupkg";

    private readonly ApplicationConfiguration _applicationConfiguration;

    private readonly ProjectReader _projectReader;

    private readonly ProjectNames _projectNames;

    public ProjectReferencesEngine(
        ApplicationConfiguration applicationConfiguration,
        ProjectReader projectReader,
        ProjectNames projectNames)
    {
        _applicationConfiguration = applicationConfiguration;
        _projectReader = projectReader;
        _projectNames = projectNames;
    }

    public async Task Execute()
    {
        if (!_applicationConfiguration.ValidConfiguration())
        {
            return;
        }

        List<string> projectNames = await _projectNames.GetFullProjectNames();

        // TODO : get projectReferenceTree of the solution

        _projectReader
            .Execute(projectNames)
            .ExtendProjectsWithProjectReferences()
            .ExtendProjectsWithAllSubProjectReferences()
            .ExtendProjectsWithAllSubPackageReferences()
            .ExtendProjectsWithAllRedundantProjectReferences()
            .ExtendProjectsWithAllRedundantPackageReferences();

        List<IDotNetProject> projectsWithRedundantReferences = _projectReader
            .SdkProjects
            .Where(x => x.RedundantReferencedProjects.Any() || x.RedundantPackageReferences.Any())
            .ToList();

        int projectsWithoutPackageGraph = _projectReader.SdkProjects.Count(x => x.PackageGraphMissing);

        if (projectsWithoutPackageGraph > 0)
        {
            $"note : {projectsWithoutPackageGraph} project(s) have no restore output, package dependencies were not checked for them".WriteLine();
        }

        string tab = new(' ', 4);

        if (!projectsWithRedundantReferences.Any())
        {
            "No redundant project/package references found.".WriteLine();

            return;
        }

        await using StreamWriter sw = new(@".\redundant.txt");

        string solutionText = _applicationConfiguration.SolutionFile.IsAssigned()
            ? $" in solution '{_applicationConfiguration.SolutionFile}': "
            : ": ";

        await sw.WriteLineAsync($"Redundant project/package references{solutionText}".WriteLine());

        foreach (IDotNetProject projectWithRedundantReferences in projectsWithRedundantReferences)
        {
            string line = $"{projectWithRedundantReferences.FileName.TrimStartIgnoreCase(_applicationConfiguration.BasePath)}".WriteLine();

            await sw.WriteLineAsync(line);

            foreach (IDotNetProject redundantReferencedProject in projectWithRedundantReferences.RedundantReferencedProjects)
            {
                line = $"{tab} - {redundantReferencedProject.FileName.TrimStartIgnoreCase(_applicationConfiguration.BasePath)}".WriteLine();

                await sw.WriteLineAsync(line);
            }

            foreach (string redundantPackageReference in projectWithRedundantReferences.RedundantPackageReferences)
            {
                line = $"{tab} - {redundantPackageReference}{PackageExtension}".WriteLine();

                await sw.WriteLineAsync(line);
            }
        }

        "See => 'redundant.txt'".WriteLine();
    }
}
