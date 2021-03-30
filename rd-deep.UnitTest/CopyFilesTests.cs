using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace rd_deep.UnitTest
{
    [TestClass]
    public class CopyFilesTests
    {
        private static string _testProjectFolder = string.Empty;

        [ClassInitialize]
        public static void ClassInitialize([NotNull] TestContext testContext)
        {
            var deployment = testContext.DeploymentDirectory.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).TrimList();

            var deploymentParts = deployment.TakeWhile(y => y != "bin").ToList();

            _testProjectFolder = string.Join(Path.DirectorySeparatorChar, deploymentParts);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var totalFiles = CopyFilePresenter.CopyFiles(Path.Combine(_testProjectFolder, "Test"), Path.Combine(_testProjectFolder, "Test"), "Test");
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
