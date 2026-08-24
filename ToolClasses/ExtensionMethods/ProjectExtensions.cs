using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolInterfaces;

namespace ToolClasses.ExtensionMethods;

[UsedImplicitly]
internal static class ProjectExtensions
{
    private const string SdkAttribute = "Sdk";

    private const string IncludeAttribute = "Include";

    private const string ProjectReferenceElement = "ProjectReference";

    private const string PackageReferenceElement = "PackageReference";

    extension(IDotNetProject dotNetProject)
    {
        public IDotNetProject BuildDotnetProject()
        {
            XElement? projectElement = dotNetProject.ReadProjectElement();

            if (projectElement.IsAssigned())
            {
                dotNetProject.SetProjectProperties(projectElement);
                dotNetProject.BuildProjectReferences(projectElement);
                dotNetProject.BuildPackageReferences(projectElement);
            }

            return dotNetProject;
        }

        private XElement? ReadProjectElement()
        {
            try
            {
                return XDocument.Load(dotNetProject.FileName).Root;
            }
            catch (XmlException xmlException)
            {
                $"unreadable project - '{dotNetProject.FileName}' : {xmlException.Message}".WriteLine();

                return null;
            }
        }

        private void SetProjectProperties(XElement projectElement)
        {
            dotNetProject.IsSdkProject = projectElement.Attribute(SdkAttribute).IsAssigned();
        }

        private void BuildProjectReferences(XElement projectElement)
        {
            string projectPath = Path.GetDirectoryName(dotNetProject.FileName) ?? string.Empty;

            dotNetProject.ProjectReferences.AddRange(projectElement
                .Descendants()
                .Where(element => element.Name.LocalName == ProjectReferenceElement)
                .Select(element => (string?)element.Attribute(IncludeAttribute))
                .Where(include => include.IsAssigned())
                .Select(include => Path.GetFullPath(Path.Combine(projectPath, include!))));
        }

        private void BuildPackageReferences(XElement projectElement)
        {
            dotNetProject.PackageReferences.AddRange(projectElement
                .Descendants()
                .Where(element => element.Name.LocalName == PackageReferenceElement)
                .Select(element => (string?)element.Attribute(IncludeAttribute))
                .Where(include => include.IsAssigned())
                .Select(include => include!));
        }

        public List<string> GetPackageReferencesFromReferencedProjects()
        {
            return
            [
                .. dotNetProject
                    .ReferencedProjects
                    .Concat(dotNetProject.ReferencedSubProjects)
                    .SelectMany(referencedProject => referencedProject.PackageReferences)
                    .Distinct()
            ];
        }

        public IEnumerable<IDotNetProject> GetSubProjectReferences()
        {
            List<IDotNetProject> subProjectReferences = [];

            foreach (IDotNetProject referencedSdkProject in dotNetProject.ReferencedProjects)
            {
                subProjectReferences.AddRange(referencedSdkProject.ReferencedProjects);

                if (referencedSdkProject.SubProjectReferencesResolved)
                {
                    continue;
                }

                subProjectReferences.AddRange(referencedSdkProject.GetSubProjectReferences());
            }

            dotNetProject.SubProjectReferencesResolved = true;

            return subProjectReferences.Distinct().ToList();
        }
    }
}