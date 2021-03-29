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
            var basePath = ".";

            if (args.Any())
            {
                basePath = args[0];
            }

            if (!Directory.Exists(basePath))
            {
                $@"Path not found: {basePath}".WriteLine();

                return;
            }

            var projectReferencesEngine = new ProjectReferencesEngine();

            projectReferencesEngine.Execute(basePath);
        }
    }
}
