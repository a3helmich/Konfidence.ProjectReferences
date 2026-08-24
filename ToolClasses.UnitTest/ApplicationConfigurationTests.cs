using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolClasses.ExtensionMethods;

namespace ToolClasses.UnitTest;

[TestClass]
public class ApplicationConfigurationTests
{
    private const string SolutionName = "TestSolution";

    private string _basePath = string.Empty;

    private string _currentDirectory = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _basePath = Path.Combine(Path.GetTempPath(), $"ProjectReferences.{Guid.NewGuid():N}");

        Directory.CreateDirectory(_basePath);

        File.WriteAllText(Path.Combine(_basePath, $"{SolutionName}.sln"), string.Empty);

        _currentDirectory = Directory.GetCurrentDirectory();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        Directory.SetCurrentDirectory(_currentDirectory);

        if (Directory.Exists(_basePath))
        {
            Directory.Delete(_basePath, recursive: true);
        }
    }

    [TestMethod]
    public void Constructor_WithAllProjectsSwitchBeforeBasePathArgument_ResolvesBasePath()
    {
        // Arrange
        TestContext context = CreateContext("--AllProjects", "--BasePath", _basePath);

        // Act
        string basePath = context.ApplicationConfiguration.BasePath;

        // Assert
        Assert.AreEqual(_basePath, basePath);
    }

    [TestMethod]
    public void Constructor_WithAllProjectsSwitchBetweenValueArguments_ResolvesBasePath()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects", "--solution", SolutionName);

        // Act
        string basePath = context.ApplicationConfiguration.BasePath;

        // Assert
        Assert.AreEqual(_basePath, basePath);
    }

    [TestMethod]
    public void Constructor_WithAllProjectsSwitchBetweenValueArguments_SetsAllProjects()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects", "--solution", SolutionName);

        // Act
        bool allProjects = context.ApplicationConfiguration.AllProjects;

        // Assert
        Assert.IsTrue(allProjects);
    }

    [TestMethod]
    public void Constructor_WithTrailingAllProjectsSwitch_SetsAllProjects()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName, "--AllProjects");

        // Act
        bool allProjects = context.ApplicationConfiguration.AllProjects;

        // Assert
        Assert.IsTrue(allProjects);
    }

    [TestMethod]
    public void Constructor_WithoutAllProjectsSwitch_LeavesAllProjectsUnset()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        bool allProjects = context.ApplicationConfiguration.AllProjects;

        // Assert
        Assert.IsFalse(allProjects);
    }

    [TestMethod]
    public void Constructor_WithExplicitlyDisabledAllProjectsFlag_LeavesAllProjectsUnset()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects=false", "--solution", SolutionName);

        // Act
        bool allProjects = context.ApplicationConfiguration.AllProjects;

        // Assert
        Assert.IsFalse(allProjects);
    }

    [TestMethod]
    public void Constructor_WithAllProjectsSwitch_BypassesAnExplicitlyNamedSolution()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects", "--solution", SolutionName);

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual(string.Empty, solutionFile);
    }

    [TestMethod]
    public void Constructor_WithoutSolutionArgument_DerivesTheSolutionNameFromTheFolderName()
    {
        // Arrange
        WriteFolderNamedSolution();

        TestContext context = CreateContext("--BasePath", _basePath);

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{Path.GetFileName(_basePath)}.sln", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithATrailingSeparatorOnTheBasePath_DerivesTheSameSolutionName()
    {
        // Arrange
        WriteFolderNamedSolution();

        TestContext context = CreateContext("--BasePath", $"{_basePath}{Path.DirectorySeparatorChar}");

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{Path.GetFileName(_basePath)}.sln", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithATrailingSeparatorOnTheBasePath_TrimsItFromTheBasePath()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", $"{_basePath}{Path.DirectorySeparatorChar}");

        // Act
        string basePath = context.ApplicationConfiguration.BasePath;

        // Assert
        Assert.AreEqual(_basePath, basePath);
    }

    [TestMethod]
    public void Constructor_WithARelativeBasePath_ResolvesItToAFullPath()
    {
        // Arrange
        Directory.SetCurrentDirectory(Path.GetDirectoryName(_basePath)!);

        TestContext context = CreateContext("--BasePath", Path.GetFileName(_basePath));

        // Act
        string basePath = context.ApplicationConfiguration.BasePath;

        // Assert
        Assert.AreEqual(_basePath, basePath);
    }

    [TestMethod]
    public void Constructor_WithoutBasePathArgument_FallsBackToTheCurrentDirectory()
    {
        // Arrange
        Directory.SetCurrentDirectory(_basePath);

        string currentDirectory = Directory.GetCurrentDirectory();

        TestContext context = CreateContext("--solution", SolutionName);

        // Act
        string basePath = context.ApplicationConfiguration.BasePath;

        // Assert
        Assert.AreEqual(currentDirectory, basePath);
    }

    [TestMethod]
    public void Constructor_WithoutAllProjectsSwitch_ResolvesAnExplicitlyNamedSolution()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{SolutionName}.sln", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithAnExplicitlyNamedSolutionEndingInSln_ResolvesThatSolution()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", $"{SolutionName}.sln");

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{SolutionName}.sln", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithAMissingSolutionEndingInSln_KeepsItSoTheParserCanReportIt()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", "Missing.sln");

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual("Missing.sln", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithAMissingSolutionWithoutExtension_FallsThroughToTheSlnx()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", "Missing");

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual("Missing.slnx", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithAnExplicitlyNamedSolutionEndingInSlnx_ResolvesThatSolution()
    {
        // Arrange
        WriteSlnxSolution();

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", $"{SolutionName}.slnx");

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{SolutionName}.slnx", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithASolutionNameAndOnlyASlnxPresent_ResolvesTheSlnx()
    {
        // Arrange
        File.Delete(Path.Combine(_basePath, $"{SolutionName}.sln"));

        WriteSlnxSolution();

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{SolutionName}.slnx", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithBothSolutionFormatsPresent_PrefersTheSln()
    {
        // Arrange
        WriteSlnxSolution();

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{SolutionName}.sln", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithoutSolutionArgumentAndOnlyASlnxPresent_DerivesTheSlnxFromTheFolderName()
    {
        // Arrange
        File.Delete(Path.Combine(_basePath, $"{SolutionName}.sln"));

        string folderName = Path.GetFileName(_basePath);

        File.WriteAllText(Path.Combine(_basePath, $"{folderName}.slnx"), string.Empty);

        TestContext context = CreateContext("--BasePath", _basePath);

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{folderName}.slnx", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithHelpSwitchBetweenValueArguments_SetsHelp()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--Help", "--solution", SolutionName);

        // Act
        bool help = context.ApplicationConfiguration.Help;

        // Assert
        Assert.IsTrue(help);
    }

    [TestMethod]
    public void Constructor_WithoutHelpSwitch_LeavesHelpUnset()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath);

        // Act
        bool help = context.ApplicationConfiguration.Help;

        // Assert
        Assert.IsFalse(help);
    }

    private void WriteFolderNamedSolution()
    {
        File.WriteAllText(Path.Combine(_basePath, $"{Path.GetFileName(_basePath)}.sln"), string.Empty);
    }

    private void WriteSlnxSolution()
    {
        File.WriteAllText(Path.Combine(_basePath, $"{SolutionName}.slnx"), string.Empty);
    }

    private static TestContext CreateContext(params string[] args)
    {
        string[] expandedArguments = args.ExpandSwitchArguments(CommandLineExtensions.SwitchArguments);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddCommandLine(expandedArguments)
            .Build();

        return new TestContext(new ApplicationConfiguration(configuration, expandedArguments));
    }

    private sealed class TestContext
    {
        public ApplicationConfiguration ApplicationConfiguration { get; }

        public TestContext(ApplicationConfiguration applicationConfiguration)
        {
            ApplicationConfiguration = applicationConfiguration;
        }
    }
}
