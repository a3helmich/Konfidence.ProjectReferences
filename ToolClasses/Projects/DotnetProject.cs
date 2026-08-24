using System.Collections.Generic;
using ToolInterfaces;

namespace ToolClasses.Projects;

internal class DotNetProject : IDotNetProject
{
    public string FileName { get; }

    public bool SubProjectReferencesResolved { get; set; } = false;

    public bool IsSdkProject { get; set; } = false;

    public List<string> ProjectReferences { get; set; } = [];

    public List<string> PackageReferences { get; set; } = [];

    public List<IDotNetProject> ReferencedProjects { get; set; } = [];

    public List<IDotNetProject> ReferencedSubProjects { get; set; } = [];

    public List<IDotNetProject> RedundantReferencedProjects { get; set; } = [];

    public List<string> RedundantPackageReferences { get; set; } = [];

    public DotNetProject(string projectName)
    {
        FileName = projectName;
    }
}