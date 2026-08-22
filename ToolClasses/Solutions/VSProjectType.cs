using System;

namespace ToolClasses.Solutions;

public class VSProjectType
{
    public string ProjectTypeGuid { get; }

    public Guid ProjectTypeId { get; }

    public string Name { get; }

    public VSProjectType(string name, string projectTypeGuid)
    {
        ProjectTypeGuid = projectTypeGuid.ToUpperInvariant();

        Name = name;

        if (!Guid.TryParse(projectTypeGuid, out Guid projectGuid))
        {
            throw new FormatException($"{name}:{projectTypeGuid}");
        }

        ProjectTypeId = projectGuid;
    }
}