using System.Collections.Generic;
using System.IO;
using System.Linq;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolInterfaces;

namespace ToolClasses;

public class ProjectReferencesEngine
{
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

    public void Execute()
    {
        if (!_applicationConfiguration.ValidConfiguration())
        {
            return;
        }

        List<string> projectNames = _projectNames.GetFullProjectNames();

        // TODO : get projectReferenceTree of the solution

        _projectReader
            .Execute(projectNames)
            .ExtendProjectsWithProjectReferences()
            .ExtendProjectsWithAllSubProjectReferences()
            .ExtendProjectsWithAllRedundantProjectReferences();

        List<IDotNetProject> projectsWithRedundantReferences = _projectReader
            .SdkProjects
            .Where(x => x.RedundantReferencedProjects.Any())
            .ToList();

        string tab = new(' ', 4);

        if (!projectsWithRedundantReferences.Any())
        {
            "No redundant project references found.".WriteLine();

            return;
        }

        using StreamWriter sw = new(@".\redundant.txt");

        if (_applicationConfiguration.SolutionFile.IsAssigned())
        {
            $"Redundant project references in solution '{_applicationConfiguration.SolutionFile}':".WriteLine();
        }
        else
        {
            "Redundant project references:".WriteLine();
        }

        foreach (IDotNetProject projectWithRedundantReferences in projectsWithRedundantReferences)
        {
            string line = $@"{projectWithRedundantReferences.FileName.TrimStartIgnoreCase(_applicationConfiguration.BasePath)}".WriteLine();

            sw.WriteLine(line);

            foreach (IDotNetProject redundantReferencedProject in projectWithRedundantReferences.RedundantReferencedProjects)
            {
                line = $@"{tab} - {redundantReferencedProject.FileName.TrimStartIgnoreCase(_applicationConfiguration.BasePath)}".WriteLine();

                sw.WriteLine(line);
            }
        }

        "See => 'redundant.txt'".WriteLine();
    }
}
