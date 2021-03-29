using System;
using JetBrains.Annotations;

namespace ToolClasses.Solutions
{
    public class VSProjectType
    {
        public string ProjectTypeGuid { get; }

        public Guid ProjectTypeId { get; }

        public string Name { get; }

        public VSProjectType(string name, [NotNull] string projectTypeGuid)
        {
            ProjectTypeGuid = projectTypeGuid.ToUpperInvariant();

            Name = name;

            if (!Guid.TryParse(projectTypeGuid, out var projectGuid))
            {
                throw new FormatException($"{name}:{projectTypeGuid}");
            }

            ProjectTypeId = projectGuid;
        }
    }
}
