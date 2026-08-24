using System.Collections.Generic;
using System.Linq;
using ToolClasses.Projects;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses.Readers;

public class ProjectReader
{
    public List<IDotNetProject> SdkProjects { get; private set; } = [];

    public Dictionary<string, IDotNetProject> ProjectFileNameLookup { get; private set; } = [];

    public ProjectReader Execute(List<string> projectFileNames)
    {
        List<IDotNetProject> allProjects = projectFileNames
            .Select(projectFileName => new DotNetProject(projectFileName).BuildDotnetProject())
            .ToList();

        ProjectFileNameLookup = allProjects
            .ToDictionary(project => project.FileName);

        SdkProjects = allProjects
            .Where(x => x.IsSdkProject)
            .ToList();

        return this;
    }
}
