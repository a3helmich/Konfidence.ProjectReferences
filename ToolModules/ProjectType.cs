using System;
using System.Collections.Generic;
using System.Text;

namespace ToolModules
{
    public class ProjectType
    {
        public string ProjectTypeGuid { get; }
        public Guid ProjectTypeId { get; }

        public string Name { get; }

        public ProjectType(string name, string projectTypeGuid)
        {
            ProjectTypeGuid = projectTypeGuid;

            Name = name;

            if (Guid.TryParse(projectTypeGuid, out var projectGuid))
            {
                ProjectTypeId = projectGuid;

                return;
            }

            throw new FormatException($"{name}:{projectTypeGuid}");
        }
    }
}
