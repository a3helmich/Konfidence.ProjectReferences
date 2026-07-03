using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolClasses.Projects;
using ToolInterfaces;

namespace ToolClasses.ExtensionMethods
{
    [UsedImplicitly]
    internal static class ProjectReaderExtensions
    {
        extension([NotNull] ProjectReader projectReader)
        {
            [NotNull]
            public ProjectReader ExtendProjectsWithProjectReferences()
            {
                foreach (IDotNetProject project in projectReader.SdkProjects)
                {
                    foreach (string projectReference in project.ProjectReferences)
                    {
                        if (projectReader.ProjectFileNameLookup.TryGetValue(projectReference, out IDotNetProject referencedProject))
                        {
                            project.ReferencedProjects.Add(referencedProject);
                        }
                    }
                }

                return projectReader;
            }

            [NotNull]
            public ProjectReader ExtendProjectsWithBinaryReferences()
            {
                foreach (IDotNetProject project in projectReader.SdkProjects)
                {
                    IEnumerable<string> binaryReferences = project.GetBinaryReferences();

                    foreach (string binaryReference in binaryReferences)
                    {
                        IDotNetProject referencedProject = projectReader.SdkProjects.FirstOrDefault(x => x.AssemblyName == binaryReference);

                        if (referencedProject.IsAssigned())
                        {
                            project.BinaryReferencedProjects.Add(referencedProject);
                        }
                    }
                }

                return projectReader;
            }

            [NotNull]
            public ProjectReader ExtendProjectsWithAllSubProjectReferences()
            {
                foreach (IDotNetProject sdkProject in projectReader.SdkProjects)
                {
                    IEnumerable<IDotNetProject> subProjectReferences = sdkProject.GetSubProjectReferences();

                    sdkProject.ReferencedSubProjects.AddRange(subProjectReferences);
                }

                return projectReader;
            }

            [NotNull]
            public ProjectReader ExtendProjectsWithSolutionProjects(List<ISolutionProject> solutionProjects)
            {
                if (!solutionProjects.IsAssigned() || !solutionProjects.Any())
                {
                    return projectReader;
                }

                foreach (IDotNetProject sdkProject in projectReader.SdkProjects)
                {
                    ISolutionProject solutionProject = solutionProjects.FirstOrDefault(x => x.ProjectFileName == sdkProject.FileName);

                    if (!solutionProject.IsAssigned())
                    {
                        continue;
                    }

                    sdkProject.AssemblyName = sdkProject.AssemblyName.IsAssigned() ? sdkProject.AssemblyName : solutionProject.ProjectName;
                    sdkProject.ProjectName = solutionProject.ProjectName;
                    sdkProject.ProjectId = solutionProject.ProjectId;
                    sdkProject.ProjectTypeId = solutionProject.ProjectTypeId;
                }

                foreach (IDotNetProject frameworkProject in projectReader.FrameworkProjects)
                {
                    ISolutionProject solutionProject = solutionProjects.FirstOrDefault(x => x.ProjectFileName == frameworkProject.FileName);

                    if (!solutionProject.IsAssigned())
                    {
                        continue;
                    }

                    frameworkProject.AssemblyName = frameworkProject.AssemblyName.IsAssigned() ? frameworkProject.AssemblyName : solutionProject.ProjectName;
                    frameworkProject.ProjectName = solutionProject.ProjectName;
                    frameworkProject.ProjectId = solutionProject.ProjectId;
                    frameworkProject.ProjectTypeId = solutionProject.ProjectTypeId;
                }

                return projectReader;
            }

            [NotNull]
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
        }
    }
}
