using System.Collections.Generic;
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
        private ProjectReader _projectReader;
        private SolutionReader _solutionReader;

        public void Execute([NotNull] string solutionFile, [NotNull] string basePath)
        {
            if (solutionFile.IsAssigned())
            {
                _solutionReader = new SolutionReader(Path.Combine(basePath, solutionFile));

                _solutionReader.Execute();
            }

            _projectReader = new ProjectReader(basePath);

            var projectNames = _solutionReader.IsAssigned()
                ? _solutionReader.GetFullProjectNames()
                : _projectReader.GetFullProjectNames();

            _projectReader
                .Execute(projectNames)
                .ExtendProjectsWithSolutionProjects(_solutionReader?.Solution?.SolutionProjects)
                .ExtendProjectsWithBinaryReferences();

            if (!_projectReader.SdkProjects.Any(x => x.BinaryReferencedProjects.Any()))
            {
                "All clear.".WriteLine();

                return;
            }

            "Projects referencing other projects as a binary file:".WriteLine();

            foreach (var sdkProject in _projectReader.SdkProjects)
            {
                foreach (var binaryReferencedProject in sdkProject.BinaryReferencedProjects)
                {
                    $"\tproject = {sdkProject.FileName}".WriteLine();
                    $"\t\treference = {binaryReferencedProject.AssemblyName}.dll".WriteLine();
                }
            }
        }
    }
}
