using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ToolInterfaces;
using ToolModules.ExtensionMethods;

namespace ToolModules
{
    public class ProjectReader
    {
        private string _basePath;

        public List<IDotNetProject> SdkProjects = new List<IDotNetProject>();
        public List<IDotNetProject> FrameworkProjects = new List<IDotNetProject>();

        public Dictionary<string, IDotNetProject> ProjectLookup;

        public void ReadProjects(string basePath)
        {
            _basePath = basePath;

            var fullPath = Path.GetFullPath(_basePath);

            var projectNames = Directory.GetFiles(fullPath, @"*.csproj", SearchOption.AllDirectories).ToList();

            var allProjects = projectNames.Select(BuildSdkProject).ToList();

            FillProjectsCollections(allProjects);
        }


        private void FillProjectsCollections(List<IDotNetProject> allProjects)
        {
            ProjectLookup = allProjects.ToDictionary(project => project.Name);

            SdkProjects = allProjects.Where(x => x.IsSdkProject).ToList();
            FrameworkProjects = allProjects.Where(x => !x.IsSdkProject).ToList();
        }

        private static IDotNetProject BuildSdkProject(string projectName)
        {
            var sdkProject = new DotNetProject(projectName);

            sdkProject.ReadProjectLines();

            sdkProject.SetIsSdkProject();

            return sdkProject;
        }
    }
}
