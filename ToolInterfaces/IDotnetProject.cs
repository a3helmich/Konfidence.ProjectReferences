using System;
using System.Collections.Generic;
using System.Text;

namespace ToolInterfaces
{
    public interface IDotnetProject
    {
        string Name { get; }
        bool SubReferencesResolved { get; set; } 
        List<string> ProjectLines { get; set; }

        bool IsSdkProject { get; set; }

        List<IDotnetProject> ReferencedProjects { get; set; }
        List<IDotnetProject> SubReferencedProjects { get; set; } 
        List<IDotnetProject> RedundantReferencedProjects { get; set; }
    }
}
