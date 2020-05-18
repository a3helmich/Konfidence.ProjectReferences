using System;
using System.Collections.Generic;
using System.Text;

namespace ToolModules
{
    public class ProjectType
    {
        public string ProjectGuidString { get; }
        public Guid ProjectGuid { get; }

        public string Name { get; }

        public ProjectType(string name, string projectGuidString)
        {
            ProjectGuidString = projectGuidString;

            Name = name;

            if (Guid.TryParse(projectGuidString, out var projectGuid))
            {
                ProjectGuid = projectGuid;

                return;
            }

            throw new FormatException($"{name}:{projectGuidString}");
        }
    }
}
