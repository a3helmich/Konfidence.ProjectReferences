using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses.UnitTest;

[TestClass]
public class ArgumentParserTests
{
    private const string SolutionName = "TestSolution";

    private string _basePath = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _basePath = Path.Combine(Path.GetTempPath(), $"ProjectReferences.{Guid.NewGuid():N}");

        Directory.CreateDirectory(_basePath);
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
    public void ValidateArguments_WithAnExistingBasePathAndNoSolution_ReturnsTrue()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath);

        // Act
        bool valid = context.ArgumentParser.ValidateArguments(context.ApplicationConfiguration);

        // Assert
        Assert.IsTrue(valid);
    }

    [TestMethod]
    public void ValidateArguments_WithAnExistingBasePathAndAnExistingSolution_ReturnsTrue()
    {
        // Arrange
        WriteSolution();

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Act
        bool valid = context.ArgumentParser.ValidateArguments(context.ApplicationConfiguration);

        // Assert
        Assert.IsTrue(valid);
    }

    [TestMethod]
    public void ValidateArguments_WithAnExistingBasePathAndAMissingSolution_ReturnsFalse()
    {
        // Arrange
        // the '.sln' extension short circuits the solution resolution, so a missing solution file survives it
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", "Missing.sln");

        // Act
        bool valid = context.ArgumentParser.ValidateArguments(context.ApplicationConfiguration);

        // Assert
        Assert.IsFalse(valid);
    }

    [TestMethod]
    public void ValidateArguments_WithAMissingBasePath_ReturnsFalse()
    {
        // Arrange
        // AllProjects returns before the solution scan, which is what keeps a missing base path constructable
        TestContext context = CreateContext("--BasePath", Path.Combine(_basePath, "Missing"), "--AllProjects");

        // Act
        bool valid = context.ArgumentParser.ValidateArguments(context.ApplicationConfiguration);

        // Assert
        Assert.IsFalse(valid);
    }

    [TestMethod]
    public void ValidateArguments_WithHelpSwitch_ReturnsFalseSoNothingIsScanned()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath, "--Help");

        // Act
        bool valid = context.ArgumentParser.ValidateArguments(context.ApplicationConfiguration);

        // Assert
        Assert.IsFalse(valid);
    }

    [TestMethod]
    public void ValidateArguments_WithHelpSwitchAndAMissingBasePath_ReturnsFalseWithoutTouchingTheFileSystem()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", Path.Combine(_basePath, "Missing"), "--Help");

        // Act
        bool valid = context.ArgumentParser.ValidateArguments(context.ApplicationConfiguration);

        // Assert
        Assert.IsFalse(valid);
    }

    private void WriteSolution()
    {
        File.WriteAllText(Path.Combine(_basePath, $"{SolutionName}.sln"), string.Empty);
    }

    private static TestContext CreateContext(params string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddCommandLine(args.ExpandSwitchArguments(CommandLineExtensions.SwitchArguments))
            .Build();

        return new TestContext(new ArgumentParser(), new ApplicationConfiguration(configuration));
    }

    private sealed class TestContext
    {
        public ArgumentParser ArgumentParser { get; }

        public ApplicationConfiguration ApplicationConfiguration { get; }

        public TestContext(ArgumentParser argumentParser, ApplicationConfiguration applicationConfiguration)
        {
            ArgumentParser = argumentParser;
            ApplicationConfiguration = applicationConfiguration;
        }
    }
}
