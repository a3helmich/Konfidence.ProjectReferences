using System.Collections.Generic;
using JetBrains.Annotations;
using ToolInterfaces;

namespace ToolClasses.Projects
{
    internal class DotNetProject : IDotNetProject
    {
        public string FileName { get; }

        public bool SubProjectReferencesResolved { get; set; } = false;

        public bool IsSdkProject { get; set; } = false;

        public string AssemblyName { get; set; }

        public string ProjectName { get; set; }

        public string ProjectId { get; set; }

        public string ProjectTypeId { get; set; }

        public List<string> ProjectLines { get; set; } = [];

        public List<string> ProjectReferences { get; set; } = [];

        public List<IDotNetProject> BinaryReferencedProjects { get; set; } = [];

        public List<IDotNetProject> ReferencedProjects { get; set; } = [];

        public List<IDotNetProject> ReferencedSubProjects { get; set; } = [];

        public List<IDotNetProject> RedundantReferencedProjects { get; set; } = [];

        public DotNetProject([NotNull] string projectName)
        {
            FileName = projectName;
        }
    }
}
