using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses
{
    public class ArgumentParser
    {

        public static (string solutionFile, string basePath) ParseArguments([NotNull] params string[] args)
        {
            if (!args.Any())
            {
                return (string.Empty, ".");
            }

            var basePath = ".";
            var solutionFile = string.Empty;

            if (args.TryParseArgument(Arguments.Solution, out var commandLineArgument))
            {
                solutionFile = commandLineArgument;
            }

            if (!solutionFile.IsAssigned() && args.TryParseArgument(Arguments.Path, out commandLineArgument))
            {
                basePath = commandLineArgument;
            }

            if (!commandLineArgument.IsAssigned())
            {
                basePath = args[0];

                if (!basePath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                {
                    return (solutionFile, basePath);
                }

                solutionFile = Path.GetFileName(basePath);
                basePath = Path.GetDirectoryName(basePath);
            }

            return (solutionFile, basePath);
        }

        public static void ValidateArguments([NotNull] string[] args, [NotNull] string basePath, [NotNull] string solutionFile)
        {
            if (!File.Exists(Path.Combine(basePath, solutionFile)) || !Directory.Exists(basePath) || !args.Any())
            {
                if (!File.Exists(Path.Combine(basePath, solutionFile)))
                {
                    $"not found : solution file - '{Path.Combine(basePath, solutionFile)}'".WriteLine();
                }

                if (!solutionFile.IsAssigned() && !Directory.Exists(basePath))
                {
                    $"not found : path - '{basePath}'".WriteLine();
                }

                new string('=', 78).WriteLine();

                $"valid arguments : [PathOrSolutionName] [--{Arguments.Path}=={Arguments.Path}] [--{Arguments.Solution}=={Arguments.Solution}]"
                    .WriteLine();
                "All arguments are mutually exclusive".WriteLine();

                "PathOrSolutionName : lets see where we get with just a Path or a SolutionName.".WriteLine();
                $"{Arguments.Path} : path where to look for .csproj files, recursively".WriteLine();
                $"{Arguments.Solution} : the solutionfile [with path] to parse to get the .csproj files".WriteLine();

                Environment.Exit(1);
            }
        }
    }
}
