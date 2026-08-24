using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolClasses.Readers;

namespace ToolClasses.UnitTest;

[TestClass]
public class ProjectReferencesEngineTests
{
    private const string RedundantFileName = "redundant.txt";

    private const string SolutionName = "TestSolution";

    private const string CSharpProjectTypeId = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";

    private string _basePath = string.Empty;

    private string _outputPath = string.Empty;

    private string _currentDirectory = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _basePath = CreateFolder("base");
        _outputPath = CreateFolder("output");

        WriteProject("C");
        WriteProject("B", "C");
        WriteProject("A", "B", "C");

        _currentDirectory = Directory.GetCurrentDirectory();

        Directory.SetCurrentDirectory(_outputPath);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        Directory.SetCurrentDirectory(_currentDirectory);

        DeleteFolder(_basePath);
        DeleteFolder(_outputPath);
    }

    [TestMethod]
    public async Task Execute_WithAllProjectsSwitch_ReportsRedundantReferencesFoundByScanningTheBasePath()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, Path.Combine("A", "A.csproj"));
        StringAssert.Contains(redundant, Path.Combine("C", "C.csproj"));
    }

    [TestMethod]
    public async Task Execute_WithAllProjectsSwitch_DoesNotReportProjectsThatAreOnlyReferencedOnce()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        Assert.IsFalse(redundant.Contains(Path.Combine("B", "B.csproj"), StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Execute_WithSolutionThatOmitsAProject_DoesNotReportTheOmittedProject()
    {
        // Arrange
        WriteSolution("A", "B");

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        AssertRedundantFileWasNotWritten();
    }

    [TestMethod]
    public async Task Execute_WithAllProjectsSwitch_BypassesTheSolutionAndReportsProjectsOutsideIt()
    {
        // Arrange
        WriteSolution("A", "B");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects", "--solution", SolutionName);

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, Path.Combine("A", "A.csproj"));
        StringAssert.Contains(redundant, Path.Combine("C", "C.csproj"));
    }

    [TestMethod]
    public async Task Execute_WithReferencesOnTheItemGroupLine_ReportsTheRedundantReference()
    {
        // Arrange
        WriteProjectWithInlineReferences("A", "B", "C");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, Path.Combine("A", "A.csproj"));
        StringAssert.Contains(redundant, Path.Combine("C", "C.csproj"));
    }

    [TestMethod]
    public async Task Execute_WithReferencesCarryingExtraAttributes_ReportsTheRedundantReference()
    {
        // Arrange
        WriteProjectWithDecoratedReferences("A", "B", "C");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, Path.Combine("A", "A.csproj"));
        StringAssert.Contains(redundant, Path.Combine("C", "C.csproj"));
    }

    [TestMethod]
    public async Task Execute_WithAnUnreadableProject_StillReportsTheReadableOnes()
    {
        // Arrange
        WriteMalformedProject("D");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, Path.Combine("A", "A.csproj"));
        StringAssert.Contains(redundant, Path.Combine("C", "C.csproj"));
    }

    [TestMethod]
    public async Task Execute_WithAPackageAlreadyBroughtByAReferencedProject_ReportsItAsRedundant()
    {
        // Arrange
        WriteProject("C", [], "Serilog");
        WriteProject("B", ["C"]);
        WriteProject("A", ["B"], "Serilog");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, "Serilog");
    }

    [TestMethod]
    public async Task Execute_WithAPackageNoReferencedProjectBrings_DoesNotReportIt()
    {
        // Arrange
        WriteProject("C", []);
        WriteProject("B", ["C"]);
        WriteProject("A", ["B"], "Serilog");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        AssertRedundantFileWasNotWritten();
    }

    [TestMethod]
    public async Task Execute_WithAPackageBroughtOnlyThroughAnIndirectProject_ReportsItAsRedundant()
    {
        // Arrange
        WriteProject("C", [], "Newtonsoft.Json");
        WriteProject("B", ["C"]);
        WriteProject("A", ["B"], "Newtonsoft.Json");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, "Newtonsoft.Json");
    }

    [TestMethod]
    public async Task Execute_WithAPackageAlreadyBroughtByAnotherPackage_ReportsItAsRedundant()
    {
        // Arrange
        WriteProject("A", [], "Outer", "Inner");
        WriteAssetsFile("A", ("Outer", ["Inner"]), ("Inner", []));

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, "Inner.nupkg");
    }

    [TestMethod]
    public async Task Execute_WithAPackageBroughtThroughAChainOfPackages_ReportsItAsRedundant()
    {
        // Arrange
        WriteProject("A", [], "Outer", "Deepest");
        WriteAssetsFile("A", ("Outer", ["Middle"]), ("Middle", ["Deepest"]), ("Deepest", []));

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, "Deepest.nupkg");
    }

    [TestMethod]
    public async Task Execute_WithUnrelatedPackages_DoesNotReportThem()
    {
        // Arrange
        WriteProject("A", [], "Outer", "Unrelated");
        WriteAssetsFile("A", ("Outer", ["Inner"]), ("Inner", []), ("Unrelated", []));

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        AssertRedundantFileWasNotWritten();
    }

    [TestMethod]
    public async Task Execute_WithoutRestoreOutput_DoesNotReportPackagesBroughtByOtherPackages()
    {
        // Arrange
        WriteProject("A", [], "Outer", "Inner");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        AssertRedundantFileWasNotWritten();
    }

    [TestMethod]
    public async Task Execute_WithAnUnrelatedProjectWalkedFirst_StillReportsTheRedundantReference()
    {
        // Arrange
        // B -> E is redundant: E is reachable as B -> C -> D -> E.
        // A only references B, so it must not change what is reported for B.
        WriteProject("E", []);
        WriteProject("D", ["E"]);
        WriteProject("C", ["D"]);
        WriteProject("B", ["C", "E"]);
        WriteProject("A", ["B"]);

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, Path.Combine("B", "B.csproj"));
        StringAssert.Contains(redundant, Path.Combine("E", "E.csproj"));
    }

    [TestMethod]
    public async Task Execute_WithAPrivateAssetsPackageInAReferencedProject_DoesNotReportItAsRedundant()
    {
        // Arrange
        WriteProjectWithPrivatePackage("C", "Serilog");
        WriteProject("B", ["C"]);
        WriteProject("A", ["B"], "Serilog");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        AssertRedundantFileWasNotWritten();
    }

    [TestMethod]
    public async Task Execute_WithAPrivateAssetsAttributeInAReferencedProject_DoesNotReportItAsRedundant()
    {
        // Arrange
        WriteProjectWithPrivatePackageAttribute("C", "Serilog");
        WriteProject("B", ["C"]);
        WriteProject("A", ["B"], "Serilog");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        AssertRedundantFileWasNotWritten();
    }

    [TestMethod]
    public async Task Execute_WithAnAssetsFileHoldingProjectAndLeafEntries_StillReportsTheRedundantPackage()
    {
        // Arrange
        // a real assets file also holds project entries and packages carrying no dependencies at all
        WriteProject("A", [], "Outer", "Inner");
        WriteRealisticAssetsFile("A");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, "Inner.nupkg");
    }

    [TestMethod]
    public async Task Execute_WithProjectsMissingRestoreOutput_PutsTheNoteInTheReportFile()
    {
        // Arrange
        WriteProject("C", [], "Serilog");
        WriteProject("B", ["C"]);
        WriteProject("A", ["B"], "Serilog");

        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        await context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, "no restore output");
        StringAssert.StartsWith(redundant, "note :");
    }

    private string ReadRedundantFile()
    {
        string redundantFile = Path.Combine(_outputPath, RedundantFileName);

        Assert.IsTrue(File.Exists(redundantFile), $"expected the engine to report redundant references in '{redundantFile}'");

        return File.ReadAllText(redundantFile);
    }

    private void AssertRedundantFileWasNotWritten()
    {
        string redundantFile = Path.Combine(_outputPath, RedundantFileName);

        Assert.IsFalse(File.Exists(redundantFile), $"expected no redundant references to be reported in '{redundantFile}'");
    }

    private static string CreateFolder(string name)
    {
        string folder = Path.Combine(Path.GetTempPath(), $"ProjectReferences.{name}.{Guid.NewGuid():N}");

        Directory.CreateDirectory(folder);

        return folder;
    }

    private static void DeleteFolder(string folder)
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private void WriteProject(string projectName, params string[] referencedProjectNames)
    {
        string projectFolder = Path.Combine(_basePath, projectName);

        Directory.CreateDirectory(projectFolder);

        string references = string.Empty;

        foreach (string referencedProjectName in referencedProjectNames)
        {
            references += $@"    <ProjectReference Include=""..\{referencedProjectName}\{referencedProjectName}.csproj"" />{Environment.NewLine}";
        }

        string project = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
  <ItemGroup>
{references}  </ItemGroup>
</Project>";

        File.WriteAllText(Path.Combine(projectFolder, $"{projectName}.csproj"), project);
    }

    private void WriteProject(string projectName, string[] referencedProjectNames, params string[] packageNames)
    {
        string projectFolder = Path.Combine(_basePath, projectName);

        Directory.CreateDirectory(projectFolder);

        string references = string.Empty;

        foreach (string referencedProjectName in referencedProjectNames)
        {
            references += $@"    <ProjectReference Include=""..\{referencedProjectName}\{referencedProjectName}.csproj"" />{Environment.NewLine}";
        }

        foreach (string packageName in packageNames)
        {
            references += $@"    <PackageReference Include=""{packageName}"" Version=""1.0.0"" />{Environment.NewLine}";
        }

        string project = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
  <ItemGroup>
{references}  </ItemGroup>
</Project>";

        File.WriteAllText(Path.Combine(projectFolder, $"{projectName}.csproj"), project);
    }

    private void WriteProjectWithPrivatePackageAttribute(string projectName, string packageName)
    {
        string projectFolder = Path.Combine(_basePath, projectName);

        Directory.CreateDirectory(projectFolder);

        string project = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""{packageName}"" Version=""1.0.0"" PrivateAssets=""all"" />
  </ItemGroup>
</Project>";

        File.WriteAllText(Path.Combine(projectFolder, $"{projectName}.csproj"), project);
    }

    private void WriteProjectWithPrivatePackage(string projectName, string packageName)
    {
        string projectFolder = Path.Combine(_basePath, projectName);

        Directory.CreateDirectory(projectFolder);

        string project = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
    <PackageReference Include=""{packageName}"" Version=""1.0.0"">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
</Project>";

        File.WriteAllText(Path.Combine(projectFolder, $"{projectName}.csproj"), project);
    }

    private void WriteProjectWithInlineReferences(string projectName, params string[] referencedProjectNames)
    {
        string projectFolder = Path.Combine(_basePath, projectName);

        Directory.CreateDirectory(projectFolder);

        string references = string.Empty;

        foreach (string referencedProjectName in referencedProjectNames)
        {
            string reference = $@"<ProjectReference Include=""..\{referencedProjectName}\{referencedProjectName}.csproj"" />";
            references += reference;
        }

        string project = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
  <ItemGroup>{references}</ItemGroup>
</Project>";

        File.WriteAllText(Path.Combine(projectFolder, $"{projectName}.csproj"), project);
    }

    private void WriteProjectWithDecoratedReferences(string projectName, params string[] referencedProjectNames)
    {
        string projectFolder = Path.Combine(_basePath, projectName);

        Directory.CreateDirectory(projectFolder);

        string references = string.Empty;

        foreach (string referencedProjectName in referencedProjectNames)
        {
            references += $@"    <ProjectReference PrivateAssets=""all""{Environment.NewLine}                      Include=""..\{referencedProjectName}\{referencedProjectName}.csproj""{Environment.NewLine}                      OutputItemType=""Analyzer"" />{Environment.NewLine}";
        }

        string project = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup Condition=""'$(Configuration)'=='Debug'"">
{references}  </ItemGroup>
</Project>";

        File.WriteAllText(Path.Combine(projectFolder, $"{projectName}.csproj"), project);
    }

    private void WriteMalformedProject(string projectName)
    {
        string projectFolder = Path.Combine(_basePath, projectName);

        Directory.CreateDirectory(projectFolder);

        File.WriteAllText(Path.Combine(projectFolder, $"{projectName}.csproj"), @"<Project Sdk=""Microsoft.NET.Sdk"">
  <ItemGroup>
</Project>");
    }

    private void WriteRealisticAssetsFile(string projectName)
    {
        string assetsFolder = Path.Combine(_basePath, projectName, "obj");

        Directory.CreateDirectory(assetsFolder);

        string assets = @"{
  ""version"": 3,
  ""targets"": {
    ""net10.0"": {
      ""SomeReferencedProject/1.0.0"": { ""type"": ""project"", ""framework"": "".NETCoreApp,Version=v10.0"" },
      ""Inner/1.0.0"": { ""type"": ""package"" },
      ""Outer/1.0.0"": { ""type"": ""package"", ""dependencies"": { ""Inner"": ""1.0.0"" } }
    }
  }
}";

        File.WriteAllText(Path.Combine(assetsFolder, "project.assets.json"), assets);
    }

    private void WriteAssetsFile(string projectName, params (string Package, string[] Dependencies)[] packages)
    {
        string assetsFolder = Path.Combine(_basePath, projectName, "obj");

        Directory.CreateDirectory(assetsFolder);

        string libraries = string.Empty;

        foreach ((string package, string[] dependencies) in packages)
        {
            string dependencyEntries = string.Join(", ", dependencies.Select(dependency => $@"""{dependency}"": ""1.0.0"""));

            libraries += $@"      ""{package}/1.0.0"": {{ ""type"": ""package"", ""dependencies"": {{ {dependencyEntries} }} }},";
        }

        string assets = $@"{{
  ""version"": 3,
  ""targets"": {{
    ""net10.0"": {{
{libraries.TrimEnd(',')}
    }}
  }}
}}";

        File.WriteAllText(Path.Combine(assetsFolder, "project.assets.json"), assets);
    }

    private void WriteSolution(params string[] projectNames)
    {
        string solution = $"Microsoft Visual Studio Solution File, Format Version 12.00{Environment.NewLine}";

        foreach (string projectName in projectNames)
        {
            solution += $@"Project(""{CSharpProjectTypeId}"") = ""{projectName}"", ""{projectName}\{projectName}.csproj"", ""{{{Guid.NewGuid()}}}""{Environment.NewLine}";
            solution += $"EndProject{Environment.NewLine}";
        }

        File.WriteAllText(Path.Combine(_basePath, $"{SolutionName}.sln"), solution);
    }

    private static TestContext CreateContext(params string[] args)
    {
        string[] expandedArguments = args.ExpandSwitchArguments(CommandLineExtensions.SwitchArguments);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddCommandLine(expandedArguments)
            .Build();

        ApplicationConfiguration applicationConfiguration = new(configuration, expandedArguments);

        ProjectReader projectReader = new();

        SolutionReader solutionReader = new(applicationConfiguration);

        ProjectNames projectNames = new(solutionReader, applicationConfiguration);

        return new TestContext(new ProjectReferencesEngine(applicationConfiguration, projectReader, projectNames, new RedundancyReport(applicationConfiguration)));
    }

    private sealed class TestContext
    {
        public ProjectReferencesEngine ProjectReferencesEngine { get; }

        public TestContext(ProjectReferencesEngine projectReferencesEngine)
        {
            ProjectReferencesEngine = projectReferencesEngine;
        }
    }
}
