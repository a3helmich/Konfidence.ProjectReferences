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
        string[] args = ["--Verbose"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.Verbose);

        // Assert
        CollectionAssert.AreEqual(new[] { "--Verbose=true" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithSlashPrefixedSwitch_AppendsTrueValue()
    {
        // Arrange
        string[] args = ["/Verbose"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.Verbose);

        // Assert
        CollectionAssert.AreEqual(new[] { "/Verbose=true" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithDifferentlyCasedSwitch_AppendsTrueValue()
    {
        // Arrange
        string[] args = ["--verbose"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.Verbose);

        // Assert
        CollectionAssert.AreEqual(new[] { "--verbose=true" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithSwitchThatAlreadyHasAValue_LeavesArgumentUnchanged()
    {
        // Arrange
        string[] args = ["--Verbose=false"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.Verbose);

        // Assert
        CollectionAssert.AreEqual(new[] { "--Verbose=false" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithValueArguments_LeavesArgumentsUnchanged()
    {
        // Arrange
        string[] args = ["--BasePath", @"C:\Projects\Verbose", "--solution", "ProjectReferences"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.Verbose);

        // Assert
        CollectionAssert.AreEqual(args, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithSwitchBetweenValueArguments_ExpandsOnlyTheSwitch()
    {
        // Arrange
        string[] args = ["--BasePath", @"C:\Projects\X", "--Verbose", "--solution", "ProjectReferences"];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.Verbose);

        // Assert
        CollectionAssert.AreEqual(new[] { "--BasePath", @"C:\Projects\X", "--Verbose=true", "--solution", "ProjectReferences" }, expanded);
    }

    [TestMethod]
    public void ExpandSwitchArguments_WithoutArguments_ReturnsEmptyResult()
    {
        // Arrange
        string[] args = [];

        // Act
        string[] expanded = args.ExpandSwitchArguments(Arguments.Verbose);

        // Assert
        Assert.AreEqual(0, expanded.Length);
    }
}
