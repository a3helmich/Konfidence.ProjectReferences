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
    public void Constructor_WithAllProjectsSwitch_BypassesTheSolutionInTheBasePath()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--AllProjects");

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual(string.Empty, solutionFile);
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
    public void Constructor_WithoutAllProjectsSwitch_ResolvesTheSolutionInTheBasePath()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath);

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{SolutionName}.sln", solutionFile);
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
    public void Constructor_WithAMissingSolutionWithoutExtension_KeepsItSoTheParserCanReportIt()
    {
        // Arrange
        // naming a solution never falls back to the one in the base path, whichever form the name takes
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", "Missing");

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual("Missing.sln", solutionFile);
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

    private static TestContext CreateContext(params string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddCommandLine(args.ExpandSwitchArguments(CommandLineExtensions.SwitchArguments))
            .Build();

        return new TestContext(new ApplicationConfiguration(configuration));
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
