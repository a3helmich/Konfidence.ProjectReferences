using System.Collections.Generic;
using JetBrains.Annotations;
using ToolInterfaces;

namespace ToolModules
{
    public class DotNetProject : IDotNetProject
    {
        public string Name { get; }
        public bool SubReferencesResolved { get; set; } = false;
        public bool IsSdkProject { get; set; } = false;
        public List<string> ProjectLines { get; set; } = new List<string>();

        public List<IDotNetProject> ReferencedProjects { get; set; } = new List<IDotNetProject>();
        public List<IDotNetProject> SubReferencedProjects { get; set; } = new List<IDotNetProject>();
        public List<IDotNetProject> RedundantReferencedProjects { get; set; } = new List<IDotNetProject>();
        public string ProjectId { get; set; }
        public string AssemblyName { get; set; }
        public string ProjectType { get; set; }

        public DotNetProject([NotNull] string projectName)
        {
            Name = projectName;
        }
    }
}
