using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using ToolClasses;
using ToolClasses.ExtensionMethods;

namespace ProjectBinaryReferencesTool
{
    class Program
    {
        static void Main([NotNull] string[] args)
        {
            if (!args.Any())
            {
                "no argument: solution file".WriteLine();

                return;
            }

            var solutionFile = args[0];

            if (!File.Exists(solutionFile) || string.IsNullOrWhiteSpace(solutionFile))
            {
                $"not found : solution file - '{solutionFile}'".WriteLine();

                return;
            }

            var solutionBinaryReferencesEngine = new BinaryReferencesEngine();
            
            solutionBinaryReferencesEngine.Execute(solutionFile);
        }
    }
}
