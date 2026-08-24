using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Konfidence.Base;
using ToolClasses.ExtensionMethods;
using ToolInterfaces;

namespace ToolClasses;

public class RedundancyReport
{
    private const string PackageExtension = ".nupkg";

    private const string ReportFileName = "redundant.txt";

    private static readonly string Tab = new(' ', 4);

    private readonly ApplicationConfiguration _applicationConfiguration;

    public RedundancyReport(ApplicationConfiguration applicationConfiguration)
    {
        _applicationConfiguration = applicationConfiguration;
    }

    public async Task Write(List<IDotNetProject> projectsWithRedundantReferences)
    {
        if (!projectsWithRedundantReferences.Any())
        {
            "No redundant project/package references found.".WriteLine();

            return;
        }

        await using StreamWriter reportFile = new(ReportFileName);

        await WriteLine(reportFile, $"Redundant project/package references{GetSolutionText()}");

        foreach (IDotNetProject projectWithRedundantReferences in projectsWithRedundantReferences)
        {
            await WriteProject(reportFile, projectWithRedundantReferences);
        }

        $"See => '{ReportFileName}'".WriteLine();
    }

    private async Task WriteProject(StreamWriter reportFile, IDotNetProject projectWithRedundantReferences)
    {
        await WriteLine(reportFile, TrimBasePath(projectWithRedundantReferences.FileName));

        foreach (IDotNetProject redundantReferencedProject in projectWithRedundantReferences.RedundantReferencedProjects)
        {
            await WriteLine(reportFile, $"{Tab} - {TrimBasePath(redundantReferencedProject.FileName)}");
        }

        foreach (string redundantPackageReference in projectWithRedundantReferences.RedundantPackageReferences)
        {
            await WriteLine(reportFile, $"{Tab} + {redundantPackageReference}{PackageExtension}");
        }
    }

    private static async Task WriteLine(StreamWriter reportFile, string line)
    {
        await reportFile.WriteLineAsync(line.WriteLine());
    }

    private string GetSolutionText()
    {
        return _applicationConfiguration.SolutionFile.IsAssigned()
            ? $" in solution '{_applicationConfiguration.SolutionFile}': "
            : ": ";
    }

    private string TrimBasePath(string fileName)
    {
        return fileName.TrimStartIgnoreCase(_applicationConfiguration.BasePath);
    }
}
