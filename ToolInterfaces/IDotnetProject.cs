using System.Collections.Generic;

namespace ToolInterfaces;

public interface IDotNetProject
{
    string FileName { get; }

    bool IsSdkProject { get; set; }

    List<string> ProjectReferences { get; }

    List<string> PackageReferences { get; }

    List<string> PrivatePackageReferences { get; }

    List<string> ReferencedSubPackages { get; }

    bool PackageReferencesMissing { get; set; }

    List<IDotNetProject> ReferencedProjects { get; }

    List<IDotNetProject> ReferencedSubProjects { get; }

    List<IDotNetProject> RedundantReferencedProjects { get; }

    List<string> RedundantPackageReferences { get; }
}