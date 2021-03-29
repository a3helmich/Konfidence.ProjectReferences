using System.Collections.Generic;

namespace ToolInterfaces
{
    public interface IDotNetProject
    {
        string FileName { get; }

        bool SubProjectReferencesResolved { get; set; }

        string ProjectId { get; set; }

        string ProjectTypeId { get; set; }

        string ProjectName { get; set; }

        string AssemblyName { get; set; }

        List<string> ProjectLines { get; set; }

        bool IsSdkProject { get; set; }

        List<string> ProjectReferences { get; set; }

        List<IDotNetProject> BinaryReferencedProjects { get; set; }

        List<IDotNetProject> ReferencedProjects { get; set; }

        List<IDotNetProject> ReferencedSubProjects { get; set; } 

        List<IDotNetProject> RedundantReferencedProjects { get; set; }
    }
}
