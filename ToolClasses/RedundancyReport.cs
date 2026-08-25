using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public async Task Write(List<IDotNetProject> projectsWithRedundantReferences, int projectsWithoutPackageReferences, int nonSdkProjects)
    {
        List<string> notes = GetNotes(projectsWithoutPackageReferences, nonSdkProjects);

        if (!projectsWithRedundantReferences.Any())
        {
            WriteNotes(notes);

            "No redundant project/package references found.".WriteLine();

            RemoveReportFile();

            return;
        }

        await using StreamWriter reportFile = new(ReportFileName);

        await WriteNotes(reportFile, notes);

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

    [ExcludeFromCodeCoverage]
    private static void RemoveReportFile()
    {
        if (!File.Exists(ReportFileName))
        {
            return;
        }

        try
        {
            File.Delete(ReportFileName);
        }
        catch (IOException ioException)
        {
            $"could not remove '{ReportFileName}' : {ioException.Message}".WriteLine();

            return;
        }

        $"removed => '{ReportFileName}'".WriteLine();
    }

    private static List<string> GetNotes(int projectsWithoutPackageReferences, int nonSdkProjects)
    {
        List<string> notes = [];

        if (projectsWithoutPackageReferences > 0)
        {
            notes.Add($"note : {projectsWithoutPackageReferences} project(s) have no restore output, package dependencies were not checked for them");
        }

        if (nonSdkProjects > 0)
        {
            notes.Add($"note : {nonSdkProjects} project(s) are not SDK style and were skipped");
        }

        return notes;
    }

    private static void WriteNotes(List<string> notes)
    {
        foreach (string note in notes)
        {
            note.WriteLine();
        }
    }

    private static async Task WriteNotes(StreamWriter reportFile, List<string> notes)
    {
        foreach (string note in notes)
        {
            await WriteLine(reportFile, note);
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
