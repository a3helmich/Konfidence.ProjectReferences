using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolClasses.Readers;

namespace ToolClasses.UnitTest;

[TestClass]
public class PackageReaderTests
{
    private const string ProjectName = "TestProject";

    private const string SecondProjectName = "SecondTestProject";

    private const string AssetsFolder = "obj";

    private const string AssetsFileName = "project.assets.json";

    private const string PackageVersion = "9.0.13";

    private const string LatestFramework = "net10.0";

    private const string PreviousFramework = "net9.0";

    private const string SqlClient = "Microsoft.Data.SqlClient";

    private const string ConfigurationManager = "System.Configuration.ConfigurationManager";

    private const string SqlServerServer = "Microsoft.SqlServer.Server";

    private const string CachingMemory = "Microsoft.Extensions.Caching.Memory";

    private readonly Dictionary<string, List<string>> _targetEntries = [];

    private readonly Dictionary<string, List<PackageDependency>> _packages = [];

    private readonly List<string> _packagesWithoutNuspecFile = [];

    private readonly List<string> _packagesWithoutMetadata = [];

    private string _basePath = string.Empty;

    private string _packageFolder = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _basePath = Path.Combine(Path.GetTempPath(), $"ProjectReferences.{Guid.NewGuid():N}");

        _packageFolder = Path.Combine(_basePath, "packages");

        Directory.CreateDirectory(_packageFolder);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }

    [TestMethod]
    public void GetSubPackageReferences_WithACompileExcludedDependency_DoesNotReportThatDependency()
    {
        // Arrange
        AddPackage(SqlClient, CompileExcluded(ConfigurationManager));
        AddPackage(ConfigurationManager);

        TestContext context = CreateContext();

        // Act
        List<string> subPackageReferences = context.PackageReader.GetSubPackageReferences([SqlClient]);

        // Assert
        CollectionAssert.DoesNotContain(subPackageReferences, ConfigurationManager);
    }

    [TestMethod]
    public void GetSubPackageReferences_WithANormalDependency_ReportsThatDependency()
    {
        // Arrange
        AddPackage(SqlClient, Flowing(SqlServerServer));
        AddPackage(SqlServerServer);

        TestContext context = CreateContext();

        // Act
        List<string> subPackageReferences = context.PackageReader.GetSubPackageReferences([SqlClient]);

        // Assert
        CollectionAssert.Contains(subPackageReferences, SqlServerServer);
    }

    [TestMethod]
    public void GetSubPackageReferences_WithARuntimeOnlyDependency_DoesNotReportThatDependency()
    {
        // Arrange
        AddPackage(SqlClient, RuntimeOnly(ConfigurationManager));
        AddPackage(ConfigurationManager);

        TestContext context = CreateContext();

        // Act
        List<string> subPackageReferences = context.PackageReader.GetSubPackageReferences([SqlClient]);

        // Assert
        CollectionAssert.DoesNotContain(subPackageReferences, ConfigurationManager);
    }

    [TestMethod]
    public void GetSubPackageReferences_WithADependencyBehindACompileExcludedDependency_DoesNotReportThatDependency()
    {
        // Arrange
        AddPackage(SqlClient, CompileExcluded(ConfigurationManager));
        AddPackage(ConfigurationManager, Flowing(SqlServerServer));
        AddPackage(SqlServerServer);

        TestContext context = CreateContext();

        // Act
        List<string> subPackageReferences = context.PackageReader.GetSubPackageReferences([SqlClient]);

        // Assert
        Assert.AreEqual(0, subPackageReferences.Count);
    }

    [TestMethod]
    public void GetSubPackageReferences_WithACompileExcludedDependencyAlsoReachedNormally_ReportsThatDependency()
    {
        // Arrange
        AddPackage(SqlClient, CompileExcluded(ConfigurationManager));
        AddPackage(CachingMemory, Flowing(ConfigurationManager));
        AddPackage(ConfigurationManager);

        TestContext context = CreateContext();

        // Act
        List<string> subPackageReferences = context.PackageReader.GetSubPackageReferences([SqlClient, CachingMemory]);

        // Assert
        CollectionAssert.Contains(subPackageReferences, ConfigurationManager);
    }

    [TestMethod]
    public void GetSubPackageReferences_WithADependencyBroughtByOnlyOneTargetFramework_DoesNotReportThatDependency()
    {
        // Arrange
        AddPackageForFramework(PreviousFramework, CachingMemory, Flowing(ConfigurationManager));
        AddPackageForFramework(LatestFramework, CachingMemory);

        AddPackageForBothFrameworks(ConfigurationManager);

        TestContext context = CreateContext();

        // Act
        List<string> subPackageReferences = context.PackageReader.GetSubPackageReferences([CachingMemory]);

        // Assert
        CollectionAssert.DoesNotContain(subPackageReferences, ConfigurationManager);
    }

    [TestMethod]
    public void GetSubPackageReferences_WithADependencyBroughtByEveryTargetFramework_ReportsThatDependency()
    {
        // Arrange
        AddPackageForBothFrameworks(CachingMemory, Flowing(ConfigurationManager));
        AddPackageForBothFrameworks(ConfigurationManager);

        TestContext context = CreateContext();

        // Act
        List<string> subPackageReferences = context.PackageReader.GetSubPackageReferences([CachingMemory]);

        // Assert
        CollectionAssert.Contains(subPackageReferences, ConfigurationManager);
    }

    [TestMethod]
    public void GetSubPackageReferences_ForASecondProjectUsingTheSamePackage_AppliesTheSameCompileExclusion()
    {
        // Arrange
        AddPackage(SqlClient, CompileExcluded(ConfigurationManager), Flowing(SqlServerServer));
        AddPackage(ConfigurationManager);
        AddPackage(SqlServerServer);

        CreateContext();

        TestContext secondContext = CreateContext(SecondProjectName);

        // Act
        List<string> subPackageReferences = secondContext.PackageReader.GetSubPackageReferences([SqlClient]);

        // Assert
        CollectionAssert.AreEquivalent(new[] { SqlServerServer }, subPackageReferences);
    }

    [TestMethod]
    public void GetSubPackageReferences_WithAMissingNuspecFile_ReportsThatDependency()
    {
        // Arrange
        AddPackageWithoutNuspecFile(SqlClient, CompileExcluded(ConfigurationManager));
        AddPackage(ConfigurationManager);

        TestContext context = CreateContext();

        // Act
        List<string> subPackageReferences = context.PackageReader.GetSubPackageReferences([SqlClient]);

        // Assert
        CollectionAssert.Contains(subPackageReferences, ConfigurationManager);
    }

    [TestMethod]
    public void GetSubPackageReferences_WithoutPackageMetadata_ReportsThatDependency()
    {
        // Arrange
        AddPackageWithoutPackageMetadata(SqlClient, CompileExcluded(ConfigurationManager));
        AddPackage(ConfigurationManager);

        TestContext context = CreateContext();

        // Act
        List<string> subPackageReferences = context.PackageReader.GetSubPackageReferences([SqlClient]);

        // Assert
        CollectionAssert.Contains(subPackageReferences, ConfigurationManager);
    }

    [TestMethod]
    public void Read_WithoutAnAssetsFile_IsNotAvailable()
    {
        // Arrange
        string projectFileName = Path.Combine(_basePath, ProjectName, $"{ProjectName}.csproj");

        // Act
        PackageReader packageReader = PackageReader.Read(projectFileName);

        // Assert
        Assert.IsFalse(packageReader.IsAvailable);
    }

    private static PackageDependency Flowing(string dependencyName)
    {
        return new PackageDependency(dependencyName, string.Empty);
    }

    private static PackageDependency CompileExcluded(string dependencyName)
    {
        return new PackageDependency(dependencyName, @" exclude=""Compile""");
    }

    private static PackageDependency RuntimeOnly(string dependencyName)
    {
        return new PackageDependency(dependencyName, @" include=""Runtime""");
    }

    private void AddPackage(string packageName, params PackageDependency[] dependencies)
    {
        AddPackageForFramework(LatestFramework, packageName, dependencies);
    }

    private void AddPackageForBothFrameworks(string packageName, params PackageDependency[] dependencies)
    {
        AddPackageForFramework(LatestFramework, packageName, dependencies);

        AddPackageForFramework(PreviousFramework, packageName, dependencies);
    }

    private void AddPackageWithoutNuspecFile(string packageName, params PackageDependency[] dependencies)
    {
        _packagesWithoutNuspecFile.Add(packageName);

        AddPackageForFramework(LatestFramework, packageName, dependencies);
    }

    private void AddPackageWithoutPackageMetadata(string packageName, params PackageDependency[] dependencies)
    {
        _packagesWithoutMetadata.Add(packageName);

        AddPackageForFramework(LatestFramework, packageName, dependencies);
    }

    private void AddPackageForFramework(string framework, string packageName, params PackageDependency[] dependencies)
    {
        if (!_targetEntries.TryGetValue(framework, out List<string>? frameworkEntries))
        {
            frameworkEntries = [];

            _targetEntries[framework] = frameworkEntries;
        }

        frameworkEntries.Add($@"""{packageName}/{PackageVersion}"": {{ ""type"": ""package""{GetDependencySection(dependencies)} }}");

        AddPackageDependencies(packageName, dependencies);
    }

    private void AddPackageDependencies(string packageName, PackageDependency[] dependencies)
    {
        if (!_packages.TryGetValue(packageName, out List<PackageDependency>? packageDependencies))
        {
            packageDependencies = [];

            _packages[packageName] = packageDependencies;
        }

        packageDependencies.AddRange(dependencies.Where(dependency => !packageDependencies.Any(added => added.Name == dependency.Name)));
    }

    private static string GetDependencySection(PackageDependency[] dependencies)
    {
        if (dependencies.Length == 0)
        {
            return string.Empty;
        }

        string dependencyList = string.Join(", ", dependencies.Select(dependency => $@"""{dependency.Name}"": ""{PackageVersion}"""));

        return $@", ""dependencies"": {{ {dependencyList} }}";
    }

    private static string PackagePath(string packageName)
    {
        return $"{packageName.ToLowerInvariant()}/{PackageVersion}";
    }

    private static string NuspecFile(string packageName)
    {
        return $"{packageName.ToLowerInvariant()}.nuspec";
    }

    private void WriteNuspec(string packageName, List<PackageDependency> dependencies)
    {
        string packagePath = Path.Combine(_packageFolder, packageName.ToLowerInvariant(), PackageVersion);

        Directory.CreateDirectory(packagePath);

        string dependencyElements = string.Join(Environment.NewLine, dependencies.Select(GetDependencyElement));

        string nuspec = $@"<?xml version=""1.0""?>
<package xmlns=""http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"">
  <metadata>
    <id>{packageName}</id>
    <version>{PackageVersion}</version>
    <dependencies>
      <group targetFramework=""{LatestFramework}"">
{dependencyElements}
      </group>
    </dependencies>
  </metadata>
</package>";

        File.WriteAllText(Path.Combine(packagePath, NuspecFile(packageName)), nuspec);
    }

    private static string GetDependencyElement(PackageDependency dependency)
    {
        return $@"        <dependency id=""{dependency.Name}"" version=""{PackageVersion}""{dependency.AssetAttributes} />";
    }

    private TestContext CreateContext()
    {
        return CreateContext(ProjectName);
    }

    private TestContext CreateContext(string projectName)
    {
        string projectPath = Path.Combine(_basePath, projectName);

        Directory.CreateDirectory(Path.Combine(projectPath, AssetsFolder));

        WriteNuspecs();

        File.WriteAllText(Path.Combine(projectPath, AssetsFolder, AssetsFileName), GetAssets());

        return new TestContext(PackageReader.Read(Path.Combine(projectPath, $"{projectName}.csproj")));
    }

    private void WriteNuspecs()
    {
        foreach (KeyValuePair<string, List<PackageDependency>> package in _packages.Where(package => !_packagesWithoutNuspecFile.Contains(package.Key)))
        {
            WriteNuspec(package.Key, package.Value);
        }
    }

    private string GetAssets()
    {
        string targets = string.Join(", ", _targetEntries.Select(target => $@"""{target.Key}"": {{ {string.Join(", ", target.Value)} }}"));

        string libraries = string.Join(", ", _packages.Keys.Select(GetLibraryEntry));

        return $@"{{
  ""version"": 3,
  ""targets"": {{ {targets} }},
  ""libraries"": {{ {libraries} }},
  ""packageFolders"": {{ ""{_packageFolder.Replace("\\", "\\\\")}"": {{}} }}
}}";
    }

    private string GetLibraryEntry(string packageName)
    {
        if (_packagesWithoutMetadata.Contains(packageName))
        {
            return $@"""{packageName}/{PackageVersion}"": {{ ""type"": ""package"" }}";
        }

        return $@"""{packageName}/{PackageVersion}"": {{ ""type"": ""package"", ""path"": ""{PackagePath(packageName)}"", ""files"": [ ""{NuspecFile(packageName)}"" ] }}";
    }

    private sealed class TestContext
    {
        public PackageReader PackageReader { get; }

        public TestContext(PackageReader packageReader)
        {
            PackageReader = packageReader;
        }
    }

    private sealed class PackageDependency
    {
        public string Name { get; }

        public string AssetAttributes { get; }

        public PackageDependency(string name, string assetAttributes)
        {
            Name = name;
            AssetAttributes = assetAttributes;
        }
    }
}
