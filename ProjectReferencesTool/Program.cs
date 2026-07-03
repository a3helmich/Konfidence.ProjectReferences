using JetBrains.Annotations;
using ToolClasses;

namespace ProjectReferencesTool
{
    class Program
    {
        static void Main([NotNull] string[] args)
        {
            (string solutionFile, string basePath) = ArgumentParser.ParseArguments(args);

            ArgumentParser.ValidateArguments(args, basePath, solutionFile);

            new ProjectReferencesEngine()
                .Execute(solutionFile, basePath);
        }
    }
}
