using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolClasses.ExtensionMethods;
using ToolClasses.Solutions;
using ToolInterfaces;

namespace ToolClasses.UnitTest;

[TestClass]
public class ProjectReferencesEngineTests
{
    private const string RedundantFileName = "redundant.txt";

    private string _basePath = string.Empty;

    private string _outputPath = string.Empty;

    private string _currentDirectory = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _basePath = CreateFolder("base");
        _outputPath = CreateFolder("output");

        // project A references B and C directly, while B already references C: A -> C is redundant
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
    public void Execute_WithoutSolutionFile_ReportsRedundantReferencesFoundByScanningTheBasePath()
    {
        // Arrange
        TestContext context = CreateContext();

        // Act
        context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        StringAssert.Contains(redundant, Path.Combine("A", "A.csproj"));
        StringAssert.Contains(redundant, Path.Combine("C", "C.csproj"));
    }

    [TestMethod]
    public void Execute_WithoutSolutionFile_DoesNotReportProjectsThatAreOnlyReferencedOnce()
    {
        // Arrange
        TestContext context = CreateContext();

        // Act
        context.ProjectReferencesEngine.Execute();

        // Assert
        string redundant = ReadRedundantFile();

        Assert.IsFalse(redundant.Contains(Path.Combine("B", "B.csproj"), StringComparison.OrdinalIgnoreCase));
    }

    private string ReadRedundantFile()
    {
        string redundantFile = Path.Combine(_outputPath, RedundantFileName);

        Assert.IsTrue(File.Exists(redundantFile), $"expected the engine to report redundant references in '{redundantFile}'");

        return File.ReadAllText(redundantFile);
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

    private TestContext CreateContext()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddCommandLine(new[] { "--BasePath", _basePath }.ExpandSwitchArguments(Arguments.Verbose))
            .Build();

        ApplicationConfiguration applicationConfiguration = new(configuration);

        SolutionReader solutionReader = new(new Solution(applicationConfiguration));

        return new TestContext(new ProjectReferencesEngine(applicationConfiguration, new ArgumentParser(), solutionReader));
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
