using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses.Projects
{
    internal class ProjectReader
    {
        private readonly string _basePath;

        public List<IDotNetProject> SdkProjects { get; private set; }

        public List<IDotNetProject> FrameworkProjects { get; private set; }

        public Dictionary<string, IDotNetProject> ProjectFileNameLookup { get; private set; }

        public ProjectReader([NotNull] string basePath)
        {
            _basePath = Path.GetFullPath(basePath);
        }

        [NotNull]
        public List<string> GetFullProjectNames()
        {
            return Directory
                .GetFiles(_basePath, @"*.csproj", SearchOption.AllDirectories)
                .ToList();
        }

        [NotNull]
        public ProjectReader Execute([NotNull] List<string> projectFileNames)
        {
            var allProjects = projectFileNames
                .Select(projectFileName => new DotNetProject(projectFileName).BuildDotnetProject())
                .ToList();

            ProjectFileNameLookup = allProjects
                .ToDictionary(project => project.FileName);

            SdkProjects = allProjects
                .Where(x => x.IsSdkProject)
                .ToList();

            FrameworkProjects = allProjects
                .Where(x => !x.IsSdkProject)
                .ToList();

            return this;
        }
    }
}
