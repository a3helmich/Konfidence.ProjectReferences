using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace rd_deep
{
    public class DeepFileDeleter
    {
        private readonly List<Task> _deleteTasks = [];

        private readonly List<string> _args;

        public DeepFileDeleter(List<string> args)
        {
            _args = args;
        }

        public void Execute()
        {

            //var totalFiles = CopyFilePresenter.CopyFiles(@"C:\Projects\Producten\ProjectReferences\rd-deep\Test", @"C:\Projects\Producten\ProjectReferences\rd-deep\Test", "Test");

            DateTime start = DateTime.Now;

            DeleteAllFolders(_args.First(), out int totalFiles);

            Task.WaitAll(_deleteTasks);

            DateTime end = DateTime.Now;

            TimeSpan duration = end - start;

            Console.WriteLine($"it took {duration.TotalMilliseconds} ms to delete {totalFiles} files");
            Debug.WriteLine($"it took {duration.TotalMilliseconds} ms to delete {totalFiles} files");
        }

        private List<string> DeleteAllFolders([NotNull] string folderName, out int totalFiles)
        {
            List<string> folders = [.. Directory.GetDirectories(folderName)];

            totalFiles = folders.Count;

            foreach (string folder in folders)
            {
                List<string> subFolders = DeleteAllFolders(folder, out int totalSubFiles);

                totalFiles += totalSubFiles;

                if (!subFolders.Any())
                {
                    Task deleteTask = new(() =>
                    {
                        Directory.Delete(folder, recursive: true);
                    });

                    deleteTask.Start();

                    //deleteTask.Wait();

                    _deleteTasks.Add(deleteTask);
                }
            }

            return folders;
        }
    }
}
