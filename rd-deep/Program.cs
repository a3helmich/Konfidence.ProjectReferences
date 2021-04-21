using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace rd_deep
{
    class Program
    {
        static void Main([NotNull] string[] args)
        {

            if (!args.Any())
            {
                Console.WriteLine("no arguments provided.");

                return;
            }

            if (args[0] == ".")
            {
                Console.WriteLine($"folder '.' will not be deleted, select a folder name.");

                return;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine($"folder {args[0]} not found.");

                return;
            }

            if (args[0].Contains("\\"))
            {
                Console.WriteLine("only topfolders are accepted.");

                return;
            }

            var deepDeleteFiles = new DeepFileDeleter(args.ToList());

            deepDeleteFiles.Execute();
        }
    }
}
