using System;
using System.Collections.Generic;
using System.Text;

namespace ToolInterfaces
{
    public interface IDotNetProject
    {
        string Name { get; }
        bool SubReferencesResolved { get; set; } 
        List<string> ProjectLines { get; set; }

        bool IsSdkProject { get; set; }

        List<IDotNetProject> ReferencedProjects { get; set; }
        List<IDotNetProject> SubReferencedProjects { get; set; } 
        List<IDotNetProject> RedundantReferencedProjects { get; set; }
    }
}
