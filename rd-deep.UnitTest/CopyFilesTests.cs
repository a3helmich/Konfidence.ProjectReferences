using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
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
            if (testContext.DeploymentDirectory != null)
            {
                List<string> deployment = testContext.DeploymentDirectory.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                List<string> deploymentParts = deployment.TakeWhile(y => y != "bin").ToList();

                _testProjectFolder = string.Join(Path.DirectorySeparatorChar, deploymentParts);
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            CopyFilePresenter.CopyFiles(Path.Combine(_testProjectFolder, "Test"), Path.Combine(_testProjectFolder, "Test"), "Test");
        }

        [Ignore("does not work right now")]
        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
