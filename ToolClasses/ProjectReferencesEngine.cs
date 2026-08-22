using System.Collections.Generic;
using System.IO;
using System.Linq;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolClasses.Solutions;
using ToolInterfaces;

namespace ToolClasses;

public class ProjectReferencesEngine
{
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly ArgumentParser _argumentParser;

    private ProjectReader? _projectReader;

    private readonly SolutionReader _solutionReader;

    public ProjectReferencesEngine(
        ApplicationConfiguration applicationConfiguration,
        ArgumentParser argumentParser,
        SolutionReader solutionReader)
    {
        _applicationConfiguration = applicationConfiguration;
        _argumentParser = argumentParser;
        _solutionReader = solutionReader;
    }

    public void Execute()
    {
        if (!_argumentParser.ValidateArguments(_applicationConfiguration))
        {
            return;
        }

        if (_applicationConfiguration.SolutionFile.IsAssigned())
        {
            _solutionReader.Execute();
        }

        _projectReader = new ProjectReader(_applicationConfiguration.BasePath);

        // todo: GetFullProjectNames => introduce projectFileFinder, encapsulates solutionReader => projectReader GetFullProjectNames functionality extracted
        // the solutionReader is injected, so it is always assigned: only the solution file tells us whether it has anything to offer
        List<string> projectNames = _applicationConfiguration.SolutionFile.IsAssigned()
            ? _solutionReader.GetFullProjectNames()
            : _projectReader.GetFullProjectNames();

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