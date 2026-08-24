using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ToolClasses.Readers;

internal class PackageReader
{
    private const string AssetsFolder = "obj";

    private const string AssetsFileName = "project.assets.json";

    private const string TargetsProperty = "targets";

    private const string DependenciesProperty = "dependencies";

    private const string TypeProperty = "type";

    private const string PackageType = "package";

    private readonly Dictionary<string, List<string>> _dependencies;

    public bool IsAvailable { get; }

    private PackageReader(Dictionary<string, List<string>> dependencies, bool isAvailable)
    {
        _dependencies = dependencies;
        IsAvailable = isAvailable;
    }

    public static PackageReader Read(string projectFileName)
    {
        string assetsFileName = GetAssetsFileName(projectFileName);

        if (!File.Exists(assetsFileName))
        {
            return new PackageReader([], false);
        }

        return new PackageReader(ReadDependencies(assetsFileName), true);
    }

    private static string GetAssetsFileName(string projectFileName)
    {
        string projectPath = Path.GetDirectoryName(projectFileName) ?? string.Empty;

        return Path.Combine(projectPath, AssetsFolder, AssetsFileName);
    }

    private static Dictionary<string, List<string>> ReadDependencies(string assetsFileName)
    {
        Dictionary<string, List<string>> dependencies = new(StringComparer.OrdinalIgnoreCase);

        using JsonDocument assets = JsonDocument.Parse(File.ReadAllText(assetsFileName));

        if (!assets.RootElement.TryGetProperty(TargetsProperty, out JsonElement targets))
        {
            return dependencies;
        }

        foreach (JsonProperty target in targets.EnumerateObject())
        {
            foreach (JsonProperty library in target.Value.EnumerateObject())
            {
                AddLibraryDependencies(dependencies, library);
            }
        }

        return dependencies;
    }

    private static void AddLibraryDependencies(Dictionary<string, List<string>> dependencies, JsonProperty library)
    {
        if (!IsPackage(library.Value))
        {
            return;
        }

        string packageName = library.Name.Split('/')[0];

        if (!library.Value.TryGetProperty(DependenciesProperty, out JsonElement libraryDependencies))
        {
            return;
        }

        List<string> dependencyNames = [.. libraryDependencies.EnumerateObject().Select(dependency => dependency.Name)];

        dependencies[packageName] = dependencyNames;
    }

    private static bool IsPackage(JsonElement library)
    {
        return library.TryGetProperty(TypeProperty, out JsonElement type)
               && string.Equals(type.GetString(), PackageType, StringComparison.OrdinalIgnoreCase);
    }

    public List<string> GetSubPackageReferences(List<string> packageReferences)
    {
        HashSet<string> subPackageReferences = new(StringComparer.OrdinalIgnoreCase);

        foreach (string packageReference in packageReferences)
        {
            AddSubPackageReferences(packageReference, subPackageReferences);
        }

        return [.. subPackageReferences];
    }

    private void AddSubPackageReferences(string packageReference, HashSet<string> subPackageReferences)
    {
        if (!_dependencies.TryGetValue(packageReference, out List<string>? dependencies))
        {
            return;
        }

        foreach (string dependency in dependencies)
        {
            if (subPackageReferences.Add(dependency))
            {
                AddSubPackageReferences(dependency, subPackageReferences);
            }
        }
    }
}
