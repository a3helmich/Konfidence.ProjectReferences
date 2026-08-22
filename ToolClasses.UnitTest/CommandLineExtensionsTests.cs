using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses.UnitTest;

[TestClass]
public class CommandLineExtensionsTests
{
    [TestMethod]
    public void ExpandSwitchArguments_WithValuelessSwitch_AppendsTrueValue()
    {
        // Arrange
        string[] args = ["--AllProjects"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.AllProjects);

        // Assert
        CollectionAssert.AreEqual(new[] { "--AllProjects=true" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithSlashPrefixedSwitch_AppendsTrueValue()
    {
        // Arrange
        string[] args = ["/AllProjects"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.AllProjects);

        // Assert
        CollectionAssert.AreEqual(new[] { "/AllProjects=true" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithDifferentlyCasedSwitch_AppendsTrueValue()
    {
        // Arrange
        string[] args = ["--allprojects"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.AllProjects);

        // Assert
        CollectionAssert.AreEqual(new[] { "--allprojects=true" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithSwitchThatAlreadyHasAValue_LeavesArgumentUnchanged()
    {
        // Arrange
        string[] args = ["--AllProjects=false"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.AllProjects);

        // Assert
        CollectionAssert.AreEqual(new[] { "--AllProjects=false" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithUnknownSwitch_LeavesArgumentUnchanged()
    {
        // Arrange
        string[] args = ["--Verbose"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.AllProjects);

        // Assert
        CollectionAssert.AreEqual(new[] { "--Verbose" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithValueArguments_LeavesArgumentsUnchanged()
    {
        // Arrange
        string[] args = ["--BasePath", @"C:\Projects\AllProjects", "--solution", "ProjectReferences"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.AllProjects);

        // Assert
        CollectionAssert.AreEqual(args, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithSwitchBetweenValueArguments_ExpandsOnlyTheSwitch()
    {
        // Arrange
        string[] args = ["--BasePath", @"C:\Projects\X", "--AllProjects", "--solution", "ProjectReferences"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.AllProjects);

        // Assert
        CollectionAssert.AreEqual(new[] { "--BasePath", @"C:\Projects\X", "--AllProjects=true", "--solution", "ProjectReferences" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithTheToolsOwnSwitches_ExpandsEveryOneOfThem()
    {
        // Arrange
        string[] args = ["--BasePath", @"C:\Projects\X", "--AllProjects", "--Help", "--solution", "ProjectReferences"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(CommandLineExtensions.SwitchArguments);

        // Assert
        CollectionAssert.AreEqual(new[] { "--BasePath", @"C:\Projects\X", "--AllProjects=true", "--Help=true", "--solution", "ProjectReferences" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithoutArguments_ReturnsEmptyResult()
    {
        // Arrange
        string[] args = [];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.AllProjects);

        // Assert
        Assert.AreEqual(0, expanded.Length);
    }
}
