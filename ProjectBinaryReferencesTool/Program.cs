using JetBrains.Annotations;
using ToolClasses;

namespace ProjectBinaryReferencesTool
{
    class Program
    {
        static void Main([NotNull] string[] args)
        {
            (string solutionFile, string basePath) = ArgumentParser.ParseArguments(args);

            ArgumentParser.ValidateArguments(args, basePath, solutionFile);

            new BinaryReferencesEngine()
                .Execute(solutionFile, basePath);
        }
    }
}
