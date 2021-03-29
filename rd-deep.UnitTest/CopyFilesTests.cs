using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace rd_deep.UnitTest
{
    [TestClass]
    public class CopyFilesTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var totalFiles = CopyFilePresenter.CopyFiles(@"C:\Projects\Producten\ProjectReferences\rd-deep\Test", @"C:\Projects\Producten\ProjectReferences\rd-deep\Test", "Test");
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
