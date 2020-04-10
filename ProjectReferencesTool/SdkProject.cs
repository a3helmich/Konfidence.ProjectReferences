using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectReferencesTool
{
    public class SdkProject
    {
        public string Name { get; }
        public bool SubReferencesResolved { get; set; } = false;

        public List<SdkProject> ReferencedProjects { get; set; } = new List<SdkProject>();
        public List<SdkProject> SubReferencedProjects { get; set; } = new List<SdkProject>();
        public List<SdkProject> RedundantReferencedProjects { get; set; } = new List<SdkProject>();

        public SdkProject(string name)
        {
            Name = name;
        }
    }
}
