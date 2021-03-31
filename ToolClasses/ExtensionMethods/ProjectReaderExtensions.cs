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
        [NotNull]
        public static ProjectReader ExtendProjectsWithProjectReferences([NotNull] this ProjectReader projectReader)
        {
            foreach (var project in projectReader.SdkProjects)
            {
                foreach (var projectReference in project.ProjectReferences)
                {
                    if (projectReader.ProjectFileNameLookup.TryGetValue(projectReference, out var referencedProject))
                    {
                        project.ReferencedProjects.Add(referencedProject);
                    }
                }
            }

            return projectReader;
        }

        [NotNull]
        public static ProjectReader ExtendProjectsWithBinaryReferences([NotNull] this ProjectReader projectReader)
        {
            foreach (var project in projectReader.SdkProjects)
            {
                var binaryReferences = project.GetBinaryReferences();

                foreach (var binaryReference in binaryReferences)
                {
                    var referencedProject = projectReader.SdkProjects.FirstOrDefault(x => x.AssemblyName == binaryReference);

                    if (referencedProject.IsAssigned())
                    {
                        project.BinaryReferencedProjects.Add(referencedProject);
                    }
                }
            }

            return projectReader;
        }

        [NotNull]
        public static ProjectReader ExtendProjectsWithAllSubProjectReferences([NotNull] this ProjectReader projectReader)
        {
            foreach (var sdkProject in projectReader.SdkProjects)
            {
                var subProjectReferences = sdkProject.GetSubProjectReferences();

                sdkProject.ReferencedSubProjects.AddRange(subProjectReferences);
            }

            return projectReader;
        }

        [NotNull]
        public static ProjectReader ExtendProjectsWithSolutionProjects([NotNull] this ProjectReader projectReader, List<ISolutionProject> solutionProjects)
        {
            if (!solutionProjects.IsAssigned() || !solutionProjects.Any())
            {
                return projectReader;
            }

            foreach (var sdkProject in projectReader.SdkProjects)
            {
                var solutionProject = solutionProjects.FirstOrDefault(x => x.ProjectFileName == sdkProject.FileName);

                if (!solutionProject.IsAssigned())
                {
                    continue;
                }

                sdkProject.AssemblyName = sdkProject.AssemblyName.IsAssigned() ? sdkProject.AssemblyName : solutionProject.ProjectName;
                sdkProject.ProjectName = solutionProject.ProjectName;
                sdkProject.ProjectId = solutionProject.ProjectId;
                sdkProject.ProjectTypeId = solutionProject.ProjectTypeId;
            }

            foreach (var frameworkProject in projectReader.FrameworkProjects)
            {
                var solutionProject = solutionProjects.FirstOrDefault(x => x.ProjectFileName == frameworkProject.FileName);

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
        public static ProjectReader ExtendProjectsWithAllRedundantProjectReferences([NotNull] this ProjectReader projectReader)
        {
            foreach (var sdkProject in projectReader.SdkProjects)
            {
                var redundantReferencedSubProjects = sdkProject
                    .ReferencedProjects
                    .Where(referencedProject => sdkProject.ReferencedSubProjects.Any(referencedSubProject => referencedProject == referencedSubProject));

                sdkProject.RedundantReferencedProjects.AddRange(redundantReferencedSubProjects);
            }

            return projectReader;
        }
    }
}
