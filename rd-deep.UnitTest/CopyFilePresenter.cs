using System.Collections.Generic;
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
            List<string> folders = Directory.GetDirectories(sourceFolder).ToList();

            int totalFiles = 0;

            foreach (string folder in folders)
            {
                string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

                totalFiles += files.Length;

                foreach (string file in files)
                {

                    string sourceFile = file.TrimStart(sourceRoot).TrimStart(@"\");

                    string targetFile = Path.Combine(targetFolder, sourceFile);

                    string targetSubFolder = Path.GetDirectoryName(targetFile);

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
