using System.Collections.Generic;

namespace ToolInterfaces;

public interface IDotNetProject
{
    string FileName { get; }

    bool SubProjectReferencesResolved { get; set; }

    bool IsSdkProject { get; set; }

    List<string> ProjectReferences { get; }

    List<string> PackageReferences { get; }

    List<IDotNetProject> ReferencedProjects { get; }

    List<IDotNetProject> ReferencedSubProjects { get; }

    List<IDotNetProject> RedundantReferencedProjects { get; }

    List<string> RedundantPackageReferences { get; }
}