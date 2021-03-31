using System.IO;
using System.Linq;
using JetBrains.Annotations;
using ToolClasses;
using ToolClasses.ExtensionMethods;

namespace ProjectReferencesTool
{
    class Program
    {
        static void Main([NotNull] string[] args)
        {
            var (solutionFile, basePath) = ArgumentParser.ParseArguments(args);

            ArgumentParser.ValidateArguments(args, basePath, solutionFile);

            new ProjectReferencesEngine()
                .Execute(solutionFile, basePath);
        }
    }
}
