namespace ToolInterfaces
{
    public interface ISolutionProject
    {
       string ProjectTypeId { get; set; }

       string ProjectName { get; set; }

       string ProjectFileName { get; set; }

       string ProjectId { get; set; }

    }
}
