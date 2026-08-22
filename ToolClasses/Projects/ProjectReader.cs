using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses.Projects;

internal class ProjectReader
{
    private readonly string _basePath;

    public List<IDotNetProject> SdkProjects { get; private set; } = [];

    public Dictionary<string, IDotNetProject> ProjectFileNameLookup { get; private set; } = [];

    public ProjectReader(string basePath)
    {
        _basePath = Path.GetFullPath(basePath);
    }

    public List<string> GetFullProjectNames()
    {
        return Directory
            .GetFiles(_basePath, @"*.csproj", SearchOption.AllDirectories)
            .ToList();
    }

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