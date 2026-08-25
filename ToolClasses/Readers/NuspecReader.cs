using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Konfidence.Base;

namespace ToolClasses.Readers;

internal static class NuspecReader
{
    private const string DependencyElement = "dependency";

    private const string IdAttribute = "id";

    private const string IncludeAttribute = "include";

    private const string ExcludeAttribute = "exclude";

    private const string CompileAssets = "Compile";

    private const string AllAssets = "All";

    private static readonly Dictionary<string, HashSet<string>> ReadNuspecs = new(StringComparer.OrdinalIgnoreCase);

    public static HashSet<string> GetCompileExcludedDependencies(string nuspecFileName)
    {
        if (ReadNuspecs.TryGetValue(nuspecFileName, out HashSet<string>? compileExcludedDependencies))
        {
            return compileExcludedDependencies;
        }

        compileExcludedDependencies = ReadCompileExcludedDependencies(nuspecFileName);

        ReadNuspecs[nuspecFileName] = compileExcludedDependencies;

        return compileExcludedDependencies;
    }

    private static HashSet<string> ReadCompileExcludedDependencies(string nuspecFileName)
    {
        HashSet<string> compileExcludedDependencies = new(StringComparer.OrdinalIgnoreCase);

        foreach (XElement dependency in GetDependencies(nuspecFileName).Where(DoesNotFlowCompileAssets))
        {
            string dependencyName = GetAttributeValue(dependency, IdAttribute);

            if (dependencyName.IsAssigned())
            {
                compileExcludedDependencies.Add(dependencyName);
            }
        }

        return compileExcludedDependencies;
    }

    private static List<XElement> GetDependencies(string nuspecFileName)
    {
        XDocument nuspec = XDocument.Load(nuspecFileName);

        return [.. nuspec.Descendants().Where(element => element.Name.LocalName.Equals(DependencyElement, StringComparison.OrdinalIgnoreCase))];
    }

    private static bool DoesNotFlowCompileAssets(XElement dependency)
    {
        if (ContainsCompileAssets(GetAttributeValue(dependency, ExcludeAttribute)))
        {
            return true;
        }

        string included = GetAttributeValue(dependency, IncludeAttribute);

        if (included.IsAssigned())
        {
            return !ContainsCompileAssets(included);
        }

        return false;
    }

    private static bool ContainsCompileAssets(string assets)
    {
        return assets
            .Split(',')
            .Select(asset => asset.Trim())
            .Any(asset => asset.Equals(CompileAssets, StringComparison.OrdinalIgnoreCase)
                          || asset.Equals(AllAssets, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetAttributeValue(XElement element, string attributeName)
    {
        return element
            .Attributes()
            .FirstOrDefault(attribute => attribute.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
            ?.Value ?? string.Empty;
    }
}
