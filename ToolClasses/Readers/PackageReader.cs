using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Konfidence.Base;

namespace ToolClasses.Readers;

internal class PackageReader
{
    private const string AssetsFolder = "obj";

    private const string AssetsFileName = "project.assets.json";

    private const string TargetsProperty = "targets";

    private const string LibrariesProperty = "libraries";

    private const string PackageFoldersProperty = "packageFolders";

    private const string DependenciesProperty = "dependencies";

    private const string PathProperty = "path";

    private const string FilesProperty = "files";

    private const string TypeProperty = "type";

    private const string PackageType = "package";

    private const string NuspecExtension = ".nuspec";

    private readonly List<Dictionary<string, List<string>>> _targetDependencies;

    private readonly Dictionary<string, HashSet<string>> _compileExcludedDependencies;

    public bool IsAvailable { get; }

    private PackageReader(
        List<Dictionary<string, List<string>>> targetDependencies,
        Dictionary<string, HashSet<string>> compileExcludedDependencies,
        bool isAvailable)
    {
        _targetDependencies = targetDependencies;
        _compileExcludedDependencies = compileExcludedDependencies;
        IsAvailable = isAvailable;
    }

    public static PackageReader Read(string projectFileName)
    {
        string assetsFileName = GetAssetsFileName(projectFileName);

        if (!File.Exists(assetsFileName))
        {
            return new PackageReader([], [], false);
        }

        using JsonDocument assets = JsonDocument.Parse(File.ReadAllText(assetsFileName));

        return new PackageReader(ReadDependencies(assets.RootElement), ReadCompileExcludedDependencies(assets.RootElement), true);
    }

    private static string GetAssetsFileName(string projectFileName)
    {
        string projectPath = Path.GetDirectoryName(projectFileName) ?? string.Empty;

        return Path.Combine(projectPath, AssetsFolder, AssetsFileName);
    }

    private static List<Dictionary<string, List<string>>> ReadDependencies(JsonElement assets)
    {
        List<Dictionary<string, List<string>>> targetDependencies = [];

        if (!assets.TryGetProperty(TargetsProperty, out JsonElement targets))
        {
            return targetDependencies;
        }

        foreach (JsonProperty target in targets.EnumerateObject())
        {
            targetDependencies.Add(ReadTargetDependencies(target.Value));
        }

        return targetDependencies;
    }

    private static Dictionary<string, List<string>> ReadTargetDependencies(JsonElement target)
    {
        Dictionary<string, List<string>> dependencies = new(StringComparer.OrdinalIgnoreCase);

        foreach (JsonProperty library in target.EnumerateObject())
        {
            AddLibraryDependencies(dependencies, library);
        }

        return dependencies;
    }

    private static void AddLibraryDependencies(Dictionary<string, List<string>> dependencies, JsonProperty library)
    {
        if (!IsPackage(library.Value))
        {
            return;
        }

        string packageName = GetPackageName(library);

        if (!library.Value.TryGetProperty(DependenciesProperty, out JsonElement libraryDependencies))
        {
            return;
        }

        List<string> dependencyNames = [.. libraryDependencies.EnumerateObject().Select(dependency => dependency.Name)];

        dependencies[packageName] = dependencyNames;
    }

    private static Dictionary<string, HashSet<string>> ReadCompileExcludedDependencies(JsonElement assets)
    {
        Dictionary<string, HashSet<string>> compileExcludedDependencies = new(StringComparer.OrdinalIgnoreCase);

        if (!assets.TryGetProperty(LibrariesProperty, out JsonElement libraries))
        {
            return compileExcludedDependencies;
        }

        List<string> packageFolders = GetPackageFolders(assets);

        foreach (JsonProperty library in libraries.EnumerateObject())
        {
            AddCompileExcludedDependencies(compileExcludedDependencies, packageFolders, library);
        }

        return compileExcludedDependencies;
    }

    private static void AddCompileExcludedDependencies(
        Dictionary<string, HashSet<string>> compileExcludedDependencies,
        List<string> packageFolders,
        JsonProperty library)
    {
        if (!IsPackage(library.Value))
        {
            return;
        }

        string nuspecFileName = GetNuspecFileName(packageFolders, library.Value);

        if (!nuspecFileName.IsAssigned())
        {
            return;
        }

        compileExcludedDependencies[GetPackageName(library)] = NuspecReader.GetCompileExcludedDependencies(nuspecFileName);
    }

    private static List<string> GetPackageFolders(JsonElement assets)
    {
        if (!assets.TryGetProperty(PackageFoldersProperty, out JsonElement packageFolders))
        {
            return [];
        }

        return [.. packageFolders.EnumerateObject().Select(packageFolder => packageFolder.Name)];
    }

    private static string GetNuspecFileName(List<string> packageFolders, JsonElement library)
    {
        string packagePath = GetPropertyValue(library, PathProperty);

        string nuspecFile = GetNuspecFile(library);

        if (!nuspecFile.IsAssigned())
        {
            return string.Empty;
        }

        return packageFolders
            .Select(packageFolder => Path.Combine(packageFolder, packagePath, nuspecFile))
            .FirstOrDefault(File.Exists) ?? string.Empty;
    }

    private static string GetNuspecFile(JsonElement library)
    {
        if (!library.TryGetProperty(FilesProperty, out JsonElement files))
        {
            return string.Empty;
        }

        return files
            .EnumerateArray()
            .Select(file => file.GetString() ?? string.Empty)
            .FirstOrDefault(file => file.EndsWith(NuspecExtension, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }

    private static string GetPropertyValue(JsonElement library, string propertyName)
    {
        if (!library.TryGetProperty(propertyName, out JsonElement property))
        {
            return string.Empty;
        }

        return property.GetString() ?? string.Empty;
    }

    private static string GetPackageName(JsonProperty library)
    {
        return library.Name.Split('/')[0];
    }

    private static bool IsPackage(JsonElement library)
    {
        return library.TryGetProperty(TypeProperty, out JsonElement type)
               && string.Equals(type.GetString(), PackageType, StringComparison.OrdinalIgnoreCase);
    }

    public List<string> GetSubPackageReferences(List<string> packageReferences)
    {
        List<HashSet<string>> subPackageReferencesPerTarget =
            [.. _targetDependencies.Select(dependencies => GetSubPackageReferences(dependencies, packageReferences))];

        if (subPackageReferencesPerTarget.Count == 0)
        {
            return [];
        }

        return [.. subPackageReferencesPerTarget.Aggregate(IntersectSubPackageReferences)];
    }

    private static HashSet<string> IntersectSubPackageReferences(HashSet<string> subPackageReferences, HashSet<string> targetSubPackageReferences)
    {
        subPackageReferences.IntersectWith(targetSubPackageReferences);

        return subPackageReferences;
    }

    private HashSet<string> GetSubPackageReferences(Dictionary<string, List<string>> dependencies, List<string> packageReferences)
    {
        HashSet<string> subPackageReferences = new(StringComparer.OrdinalIgnoreCase);

        foreach (string packageReference in packageReferences)
        {
            AddSubPackageReferences(dependencies, packageReference, subPackageReferences);
        }

        return subPackageReferences;
    }

    private void AddSubPackageReferences(Dictionary<string, List<string>> dependencies, string packageReference, HashSet<string> subPackageReferences)
    {
        if (!dependencies.TryGetValue(packageReference, out List<string>? packageDependencies))
        {
            return;
        }

        foreach (string dependency in packageDependencies.Where(dependency => FlowsCompileAssets(packageReference, dependency)))
        {
            if (subPackageReferences.Add(dependency))
            {
                AddSubPackageReferences(dependencies, dependency, subPackageReferences);
            }
        }
    }

    private bool FlowsCompileAssets(string packageReference, string dependency)
    {
        if (!_compileExcludedDependencies.TryGetValue(packageReference, out HashSet<string>? compileExcludedDependencies))
        {
            return true;
        }

        return !compileExcludedDependencies.Contains(dependency);
    }
}
