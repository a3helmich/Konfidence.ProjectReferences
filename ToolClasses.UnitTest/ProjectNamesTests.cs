using System;
using System.Collections.Generic;
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
public class ProjectNamesTests
{
    private const string SolutionName = "TestSolution";

    private const string CSharpProjectTypeId = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";

    private string _basePath = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _basePath = Path.Combine(Path.GetTempPath(), $"ProjectReferences.{Guid.NewGuid():N}");

        Directory.CreateDirectory(_basePath);

        WriteProject("A");
        WriteProject("B");
        WriteProject("C");
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
    public async Task GetFullProjectNames_WithAllProjectsSwitch_ReturnsEveryProjectBelowTheBasePath()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        List<string> projectNames = await context.ProjectNames.GetFullProjectNames();

        // Assert
        CollectionAssert.AreEquivalent(new[] { ProjectFile("A"), ProjectFile("B"), ProjectFile("C") }, projectNames);
    }

    [TestMethod]
    public async Task GetFullProjectNames_WithASolutionFile_ReturnsOnlyTheProjectsInThatSolution()
    {
        // Arrange
        WriteSolution("A", "B");

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        List<string> projectNames = await context.ProjectNames.GetFullProjectNames();

        // Assert
        CollectionAssert.AreEquivalent(new[] { ProjectFile("A"), ProjectFile("B") }, projectNames);
    }

    [TestMethod]
    public async Task GetFullProjectNames_WithAllProjectsSwitch_IgnoresTheSolutionAndReturnsEveryProject()
    {
        // Arrange
        WriteSolution("A", "B");

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName, "--AllProjects");

        // Act
        List<string> projectNames = await context.ProjectNames.GetFullProjectNames();

        // Assert
        CollectionAssert.AreEquivalent(new[] { ProjectFile("A"), ProjectFile("B"), ProjectFile("C") }, projectNames);
    }

    [TestMethod]
    public async Task GetFullProjectNames_WithASolutionWithoutProjects_DoesNotFallBackToScanningTheBasePath()
    {
        // Arrange
        WriteSolution();

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        List<string> projectNames = await context.ProjectNames.GetFullProjectNames();

        // Assert
        Assert.AreEqual(0, projectNames.Count);
    }

    [TestMethod]
    public async Task GetFullProjectNames_WithASolutionThatHoldsOneProject_DoesNotFallBackToScanningTheBasePath()
    {
        // Arrange
        WriteSolution("A");

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        List<string> projectNames = await context.ProjectNames.GetFullProjectNames();

        // Assert
        CollectionAssert.AreEquivalent(new[] { ProjectFile("A") }, projectNames);
    }

    [TestMethod]
    public async Task GetFullProjectNames_WithAllProjectsSwitch_ReturnsRootedPaths()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        List<string> projectNames = await context.ProjectNames.GetFullProjectNames();

        // Assert
        Assert.IsTrue(projectNames.All(Path.IsPathRooted), "expected every project name to be a full path");
    }

    [TestMethod]
    public async Task GetFullProjectNames_WithASolutionFile_ReturnsRootedPaths()
    {
        // Arrange
        WriteSolution("A", "B");

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        List<string> projectNames = await context.ProjectNames.GetFullProjectNames();

        // Assert
        Assert.IsTrue(projectNames.All(Path.IsPathRooted), "expected every project name to be a full path");
    }

    private string ProjectFile(string projectName)
    {
        return Path.Combine(_basePath, projectName, $"{projectName}.csproj");
    }

    private void WriteProject(string projectName)
    {
        string projectFolder = Path.Combine(_basePath, projectName);

        Directory.CreateDirectory(projectFolder);

        string project = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
</Project>";

        File.WriteAllText(Path.Combine(projectFolder, $"{projectName}.csproj"), project);
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

        SolutionReader solutionReader = new(applicationConfiguration);

        return new TestContext(new ProjectNames(solutionReader, applicationConfiguration));
    }

    private sealed class TestContext
    {
        public ProjectNames ProjectNames { get; }

        public TestContext(ProjectNames projectNames)
        {
            ProjectNames = projectNames;
        }
    }
}
