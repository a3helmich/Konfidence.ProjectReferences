using System;
using System.Linq;

namespace ProjectReferencesTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var basePath = ".";

            if (args.Any())
            {
                basePath = args[0];
            }

            var projectReferencesEngine = new ProjectReferencesEngine();

            projectReferencesEngine.Execute(basePath);
        }
    }
}
