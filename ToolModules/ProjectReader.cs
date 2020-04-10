using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ToolInterfaces;

namespace ToolModules
{
    public class ProjectReader
    {
        private string _basePath;

        public List<IDotnetProject> SdkProjects = new List<IDotnetProject>();
        public List<IDotnetProject> FrameworkProjects = new List<IDotnetProject>();

        public Dictionary<string, IDotnetProject> ProjectLookup;

        public void ReadProjects(string basePath)
        {
            _basePath = basePath;

            var fullPath = Path.GetFullPath(_basePath);

            var projectNames = Directory.GetFiles(fullPath, @"*.csproj", SearchOption.AllDirectories).ToList();

            var allProjects = projectNames.Select(BuildSdkProject).ToList();

            FillProjectsCollections(allProjects);
        }


        private void FillProjectsCollections(List<IDotnetProject> allProjects)
        {
            ProjectLookup = allProjects.ToDictionary(project => project.Name);

            SdkProjects = allProjects.Where(x => x.IsSdkProject).ToList();
            FrameworkProjects = allProjects.Where(x => !x.IsSdkProject).ToList();
        }

        private static IDotnetProject BuildSdkProject(string name)
        {
            var sdkProject = new DotnetProject(name);

            sdkProject.ReadProjectLines();

            sdkProject.SetIsSdkProject();

            return sdkProject;
        }
    }
}
