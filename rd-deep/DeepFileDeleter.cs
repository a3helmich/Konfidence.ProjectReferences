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
        private readonly List<Task> _deleteTasks = new();

        private readonly List<string> _args;

        public DeepFileDeleter(List<string> args)
        {
            _args = args;
        }

        public void Execute()
        {

            //var totalFiles = CopyFilePresenter.CopyFiles(@"C:\Projects\Producten\ProjectReferences\rd-deep\Test", @"C:\Projects\Producten\ProjectReferences\rd-deep\Test", "Test");

            var start = DateTime.Now;

            DeleteAllFolders(_args.First(), out var totalFiles);

            Task.WaitAll(_deleteTasks.ToArray());

            var end = DateTime.Now;

            var duration = end - start;

            Console.WriteLine($"it took {duration.TotalMilliseconds} ms to delete {totalFiles} files");
            Debug.WriteLine($"it took {duration.TotalMilliseconds} ms to delete {totalFiles} files");
        }

        private List<string> DeleteAllFolders([NotNull] string folderName, out int totalFiles)
        {
            var folders = Directory.GetDirectories(folderName).ToList();

            totalFiles = folders.Count;

            foreach (var folder in folders)
            {
                var subFolders = DeleteAllFolders(folder, out var totalSubFiles);

                totalFiles += totalSubFiles;

                if (!subFolders.Any())
                {
                    var deleteTask = new Task(() =>
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
