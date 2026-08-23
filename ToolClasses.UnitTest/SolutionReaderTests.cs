using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolClasses.ExtensionMethods;
using ToolClasses.Solutions;

namespace ToolClasses.UnitTest;

[TestClass]
public class SolutionReaderTests
{
    private const string SolutionName = "TestSolution";

    private const string CSharpProjectTypeId = "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";

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
    public void Constructor_WithASolutionFile_ReadsTheProjectsInThatSolution()
    {
        // Arrange
        WriteSolution("A", "B");

        // Act
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", SolutionName);

        // Assert
        List<string> projectNames = context.SolutionReader.GetFullProjectNames();

        CollectionAssert.AreEquivalent(new[] { ProjectFile("A"), ProjectFile("B") }, projectNames);
    }

    [TestMethod]
    public void Constructor_WithAMissingSolutionFile_DoesNotThrow()
    {
        // Arrange
        WriteSolution("A");

        // Act
        TestContext context = CreateContext("--BasePath", _basePath, "--solution", "Missing.sln");

        // Assert
        Assert.IsNotNull(context.SolutionReader);
    }

    [TestMethod]
    public void Constructor_WithAMissingSolutionFile_ReadsNoProjects()
    {
        // Arrange
        WriteSolution("A");

        TestContext context = CreateContext("--BasePath", _basePath, "--solution", "Missing.sln");

        // Act
        List<string> projectNames = context.SolutionReader.GetFullProjectNames();

        // Assert
        Assert.AreEqual(0, projectNames.Count);
    }

    [TestMethod]
    public void Constructor_WithoutASolutionFile_ReadsNoProjects()
    {
        // Arrange
        TestContext context = CreateContext("--BasePath", _basePath);

        // Act
        List<string> projectNames = context.SolutionReader.GetFullProjectNames();

        // Assert
        Assert.AreEqual(0, projectNames.Count);
    }

    private string ProjectFile(string projectName)
    {
        return Path.Combine(_basePath, projectName, $"{projectName}.csproj");
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

        return new TestContext(new SolutionReader(new Solution(applicationConfiguration), applicationConfiguration));
    }

    private sealed class TestContext
    {
        public SolutionReader SolutionReader { get; }

        public TestContext(SolutionReader solutionReader)
        {
            SolutionReader = solutionReader;
        }
    }
}
