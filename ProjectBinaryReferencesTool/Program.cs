using System;
using System.IO;
using System.Linq;
using ToolModules.ExtensionMethods;

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
                    "not found : solution file".WriteLine();
                }
            }
            else
            {
                "no argument: solution file".WriteLine();
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
