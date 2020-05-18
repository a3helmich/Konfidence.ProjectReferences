using System;
using System.IO;
using System.Linq;

namespace ProjectBinaryReferencesTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionFile = string.Empty;

            if (args.Any())
            {
                if (File.Exists(args[0]))
                {
                    solutionFile = args[0];
                }
                else
                {
                    Console.WriteLine("not found : solution file");
                }
            }
            else
            {
                Console.WriteLine("no argument: solution file");
            }

            if (string.IsNullOrWhiteSpace(solutionFile))
            {
                Environment.Exit(-1);
            }

            var solutionBinaryReferencesEngine = new SolutionBinaryReferencesEngine();

            solutionBinaryReferencesEngine.Execute(solutionFile);
        }
    }
}
