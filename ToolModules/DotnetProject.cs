using System;
using System.Collections.Generic;
using System.Text;
using ToolInterfaces;

namespace ToolModules
{
    public class DotnetProject : IDotnetProject
    {
        public string Name { get; }
        public bool SubReferencesResolved { get; set; } = false;
        public bool IsSdkProject { get; set; } = false;
        public List<string> ProjectLines { get; set; } = new List<string>();

        public List<IDotnetProject> ReferencedProjects { get; set; } = new List<IDotnetProject>();
        public List<IDotnetProject> SubReferencedProjects { get; set; } = new List<IDotnetProject>();
        public List<IDotnetProject> RedundantReferencedProjects { get; set; } = new List<IDotnetProject>();

        public DotnetProject(string name)
        {
            Name = name;
        }
    }
}
