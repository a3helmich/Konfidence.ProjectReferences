using ToolInterfaces;

namespace ToolClasses.Solutions;

public class SolutionProject : ISolutionProject
{
    public string ProjectTypeId { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string ProjectFileName { get; set; } = string.Empty;

    public string ProjectId { get; set; } = string.Empty;
}