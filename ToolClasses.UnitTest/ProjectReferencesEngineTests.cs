using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolClasses.Solutions;

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
        IConfiguration configuration = new ConfigurationBuilder()
            .AddCommandLine(args.ExpandSwitchArguments(CommandLineExtensions.SwitchArguments))
            .Build();

        ApplicationConfiguration applicationConfiguration = new(configuration);

        ProjectReader projectReader = new(applicationConfiguration);

        SolutionReader solutionReader = new(applicationConfiguration);

        ProjectNames projectNames = new(solutionReader, projectReader, applicationConfiguration);

        return new TestContext(new ProjectReferencesEngine(applicationConfiguration, projectReader, projectNames));
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
