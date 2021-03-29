using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolClasses.Solutions;

namespace ToolClasses
{
    public class BinaryReferencesEngine
    {
        private SolutionReader _solutionReader;

        public void Execute([NotNull] string solutionFile)
        {
            _solutionReader = new SolutionReader(solutionFile);

            _solutionReader.Execute();

            var projectNames = _solutionReader.GetFullProjectNames();

            var _projectReader = new ProjectReader(_solutionReader.SolutionPath);

            _projectReader
                .Execute(projectNames)
                .ExtendProjectsWithSolutionProjects(_solutionReader.Solution.SolutionProjects)
                .ExtendProjectsWithBinaryReferences();

            if (_projectReader.SdkProjects.Any(x => x.BinaryReferencedProjects.Any()))
            {
                "Projects referencing other projects as a binary file:".WriteLine();

                foreach (var sdkProject in _projectReader.SdkProjects)
                {
                    foreach (var binaryReferencedProject in sdkProject.BinaryReferencedProjects)
                    {
                        $"\tproject = {sdkProject.FileName}".WriteLine();
                        $"\t\treference = {binaryReferencedProject.AssemblyName}.dll".WriteLine();
                    }
                }

                return;
            }

            "All clear.".WriteLine();
        }
    }
}
