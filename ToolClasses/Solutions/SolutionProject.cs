using ToolInterfaces;

namespace ToolClasses.Solutions
{
    public class SolutionProject : ISolutionProject
    {
        public string ProjectTypeId { get; set; }

        public string ProjectName { get; set; }
        
        public string ProjectFileName { get; set; }
        
        public string ProjectId { get; set; }
    }
}
