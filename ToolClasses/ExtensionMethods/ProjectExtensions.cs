using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolClasses.Readers;
using ToolInterfaces;

namespace ToolClasses.ExtensionMethods;

[UsedImplicitly]
internal static class ProjectExtensions
{
    private const string SdkAttribute = "Sdk";

    private const string IncludeAttribute = "Include";

    private const string ProjectReferenceElement = "ProjectReference";

    private const string PackageReferenceElement = "PackageReference";

    private const string PrivateAssetsName = "PrivateAssets";

    private const string AllAssets = "all";

    private static bool KeepsAssetsPrivate(XElement packageReference)
    {
        string privateAssets = (string?)packageReference.Attribute(PrivateAssetsName)
                               ?? packageReference
                                   .Elements()
                                   .FirstOrDefault(element => element.Name.LocalName == PrivateAssetsName)?.Value
                               ?? string.Empty;

        return privateAssets.Trim().Equals(AllAssets, StringComparison.OrdinalIgnoreCase);
    }

    private static void AddSubProjectReferences(IDotNetProject project, HashSet<IDotNetProject> subProjectReferences)
    {
        foreach (IDotNetProject referencedProject in project.ReferencedProjects)
        {
            if (subProjectReferences.Add(referencedProject))
            {
                AddSubProjectReferences(referencedProject, subProjectReferences);
            }
        }
    }

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
            List<XElement> packageReferences =
            [
                .. projectElement
                    .Descendants()
                    .Where(element => element.Name.LocalName == PackageReferenceElement)
                    .Where(element => ((string?)element.Attribute(IncludeAttribute)).IsAssigned())
            ];

            dotNetProject.PackageReferences.AddRange(packageReferences
                .Select(element => (string)element.Attribute(IncludeAttribute)!));

            dotNetProject.PrivatePackageReferences.AddRange(packageReferences
                .Where(KeepsAssetsPrivate)
                .Select(element => (string)element.Attribute(IncludeAttribute)!));
        }

        public List<string> GetPackageReferencesFromReferencedProjects()
        {
            return
            [
                .. dotNetProject
                    .ReferencedProjects
                    .Concat(dotNetProject.ReferencedSubProjects)
                    .SelectMany(referencedProject => referencedProject.PackageReferences.Except(referencedProject.PrivatePackageReferences))
                    .Distinct()
            ];
        }

        public List<string> GetSubPackageReferences()
        {
            PackageReader packageReader = PackageReader.Read(dotNetProject.FileName);

            dotNetProject.PackageReferencesMissing = !packageReader.IsAvailable && dotNetProject.PackageReferences.Any();

            return packageReader.GetSubPackageReferences(dotNetProject.PackageReferences);
        }

        public IEnumerable<IDotNetProject> GetSubProjectReferences()
        {
            HashSet<IDotNetProject> subProjectReferences = [];

            foreach (IDotNetProject referencedSdkProject in dotNetProject.ReferencedProjects)
            {
                AddSubProjectReferences(referencedSdkProject, subProjectReferences);
            }

            return [.. subProjectReferences];
        }
    }
}