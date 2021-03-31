using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolClasses;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ProjectBinaryReferencesTool
{
    class Program
    {
        static void Main([NotNull] string[] args)
        {
            var (solutionFile, basePath) = ArgumentParser.ParseArguments(args);

            ArgumentParser.ValidateArguments(args, basePath, solutionFile);

            new BinaryReferencesEngine()
                .Execute(solutionFile, basePath);
        }
    }
}
