using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses.UnitTest;

[TestClass]
public class ApplicationConfigurationTests
{
    private const string SolutionName = "TestSolution";

    private string _basePath = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _basePath = Path.Combine(Path.GetTempPath(), $"ProjectReferences.{Guid.NewGuid():N}");

        Directory.CreateDirectory(_basePath);

        File.WriteAllText(Path.Combine(_basePath, $"{SolutionName}.sln"), string.Empty);
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
    public void Constructor_WithSolutionArgumentBeforeVerboseSwitch_ResolvesSolutionFile()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName, "--Verbose");

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{SolutionName}.sln", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithVerboseSwitchBeforeSolutionArgument_ResolvesSolutionFile()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--Verbose", "--solution", SolutionName);

        // Act
        string solutionFile = context.ApplicationConfiguration.SolutionFile;

        // Assert
        Assert.AreEqual($"{SolutionName}.sln", solutionFile);
    }

    [TestMethod]
    public void Constructor_WithVerboseSwitchBeforeBasePathArgument_ResolvesBasePath()
    {
        // Arrange
        TestContext context = CreateContext("--Verbose", "--BasePath", _basePath);

        // Act
        string basePath = context.ApplicationConfiguration.BasePath;

        // Assert
        Assert.AreEqual(_basePath, basePath);
    }

    [TestMethod]
    public void Constructor_WithVerboseSwitchBeforeSolutionArgument_SetsVerbose()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--Verbose", "--solution", SolutionName);

        // Act
        bool verbose = context.ApplicationConfiguration.Verbose;

        // Assert
        Assert.IsTrue(verbose);
    }

    [TestMethod]
    public void Constructor_WithTrailingVerboseSwitch_SetsVerbose()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName, "--Verbose");

        // Act
        bool verbose = context.ApplicationConfiguration.Verbose;

        // Assert
        Assert.IsTrue(verbose);
    }

    [TestMethod]
    public void Constructor_WithoutVerboseSwitch_LeavesVerboseUnset()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        bool verbose = context.ApplicationConfiguration.Verbose;

        // Assert
        Assert.IsFalse(verbose);
    }

    [TestMethod]
    public void Constructor_WithExplicitlyDisabledVerboseFlag_LeavesVerboseUnset()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--Verbose=false", "--solution", SolutionName);

        // Act
        bool verbose = context.ApplicationConfiguration.Verbose;

        // Assert
        Assert.IsFalse(verbose);
    }

    private static TestContext CreateContext(params string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddCommandLine(args.ExpandSwitchArguments(Arguments.Verbose))
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
