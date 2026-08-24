using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ToolClasses.Projects;
using ToolClasses.Readers;
using ToolInterfaces;

namespace ToolClasses.ExtensionMethods;

[UsedImplicitly]
internal static class ProjectReaderExtensions
{
    extension(ProjectReader projectReader)
    {
        public ProjectReader ExtendProjectsWithProjectReferences()
        {
            foreach (IDotNetProject project in projectReader.SdkProjects)
            {
                foreach (string projectReference in project.ProjectReferences)
                {
                    if (projectReader.ProjectFileNameLookup.TryGetValue(projectReference, out IDotNetProject? referencedProject))
                    {
                        project.ReferencedProjects.Add(referencedProject);
                    }
                }
            }

            return projectReader;
        }

        public ProjectReader ExtendProjectsWithAllSubProjectReferences()
        {
            foreach (IDotNetProject sdkProject in projectReader.SdkProjects)
            {
                IEnumerable<IDotNetProject> subProjectReferences = sdkProject.GetSubProjectReferences();

                sdkProject.ReferencedSubProjects.AddRange(subProjectReferences);
            }

            return projectReader;
        }

        public ProjectReader ExtendProjectsWithAllSubPackageReferences()
        {
            foreach (IDotNetProject sdkProject in projectReader.SdkProjects)
            {
                sdkProject.ReferencedSubPackages.AddRange(sdkProject.GetSubPackageReferences());
            }

            return projectReader;
        }

        public ProjectReader ExtendProjectsWithAllRedundantProjectReferences()
        {
            foreach (IDotNetProject sdkProject in projectReader.SdkProjects)
            {
                IEnumerable<IDotNetProject> redundantReferencedSubProjects = sdkProject
                    .ReferencedProjects
                    .Where(referencedProject => sdkProject.ReferencedSubProjects.Any(referencedSubProject => referencedProject == referencedSubProject));

                sdkProject.RedundantReferencedProjects.AddRange(redundantReferencedSubProjects);
            }

            return projectReader;
        }

        public ProjectReader ExtendProjectsWithAllRedundantPackageReferences()
        {
            foreach (IDotNetProject sdkProject in projectReader.SdkProjects)
            {
                List<string> packagesFromElsewhere =
                [
                    .. sdkProject.GetPackageReferencesFromReferencedProjects(),
                    .. sdkProject.ReferencedSubPackages
                ];

                IEnumerable<string> redundantPackageReferences = sdkProject
                    .PackageReferences
                    .Where(packageReference => packagesFromElsewhere.Any(referencedPackage => referencedPackage == packageReference));

                sdkProject.RedundantPackageReferences.AddRange(redundantPackageReferences);
            }

            return projectReader;
        }
    }
}