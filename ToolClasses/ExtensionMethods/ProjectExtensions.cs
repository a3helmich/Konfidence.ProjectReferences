using System;
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

    private const string PrivateAssetsName = "PrivateAssets";

    private const string AllAssets = "all";

    private static bool KeepsAssetsPrivate(XElement packageReference)
    {
        return GetPrivateAssets(packageReference).Equals(AllAssets, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetPrivateAssets(XElement packageReference)
    {
        string privateAssets = GetPrivateAssetsAttribute(packageReference);

        return privateAssets.IsAssigned()
            ? privateAssets
            : GetPrivateAssetsElement(packageReference);
    }

    private static string GetPrivateAssetsAttribute(XElement packageReference)
    {
        string? privateAssets = (string?)packageReference.Attribute(PrivateAssetsName);

        return privateAssets.IsAssigned()
            ? privateAssets.Trim()
            : string.Empty;
    }

    private static string GetPrivateAssetsElement(XElement packageReference)
    {
        string? privateAssets = packageReference
            .Elements()
            .FirstOrDefault(element => element.Name.LocalName == PrivateAssetsName)?.Value;

        return privateAssets.IsAssigned()
            ? privateAssets.Trim()
            : string.Empty;
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

            if (!projectElement.IsAssigned())
            {
                return dotNetProject;
            }

            dotNetProject.SetProjectProperties(projectElement);
            dotNetProject.BuildProjectReferences(projectElement);
            dotNetProject.BuildPackageReferences(projectElement);

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
            List<XElement> packageReferences = projectElement
                .Descendants()
                .Where(element => element.Name.LocalName == PackageReferenceElement)
                .Where(element => ((string?)element.Attribute(IncludeAttribute)).IsAssigned())
                .ToList();

            dotNetProject.PackageReferences.AddRange(packageReferences
                .Select(element => (string)element.Attribute(IncludeAttribute)!));

            dotNetProject.PrivatePackageReferences.AddRange(packageReferences
                .Where(KeepsAssetsPrivate)
                .Select(element => (string)element.Attribute(IncludeAttribute)!));
        }

        public List<string> GetPackageReferencesFromReferencedProjects()
        {
            return dotNetProject
                .ReferencedProjects
                .Concat(dotNetProject.ReferencedSubProjects)
                .SelectMany(referencedProject => referencedProject.PackageReferences.Except(referencedProject.PrivatePackageReferences))
                .Distinct()
                .ToList();
        }

        public IEnumerable<IDotNetProject> GetSubProjectReferences()
        {
            HashSet<IDotNetProject> subProjectReferences = [];

            foreach (IDotNetProject referencedSdkProject in dotNetProject.ReferencedProjects)
            {
                AddSubProjectReferences(referencedSdkProject, subProjectReferences);
            }

            return subProjectReferences;
        }
    }
}
