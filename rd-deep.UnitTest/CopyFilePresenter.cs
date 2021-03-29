using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;

namespace rd_deep.UnitTest
{
    public class CopyFilePresenter
    {
        internal static int CopyFiles([NotNull] string sourceFolder, string sourceRoot, string targetFolder)
        {
            var folders = Directory.GetDirectories(sourceFolder).ToList();

            var totalFiles = 0;

            foreach (var folder in folders)
            {
                var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

                totalFiles += files.Length;

                foreach (var file in files)
                {

                    var sourceFile = file.TrimStart(sourceRoot).TrimStart(@"\");

                    var targetFile = Path.Combine(targetFolder, sourceFile);

                    var targetSubFolder = Path.GetDirectoryName(targetFile);

                    if (targetSubFolder.IsAssigned() && !Directory.Exists(targetSubFolder))
                    {
                        Directory.CreateDirectory(targetSubFolder);
                    }

                    if (!File.Exists(targetFile))
                    {
                        File.Copy(file, targetFile);
                    }
                }
            }

            return totalFiles;
        }
    }
}
