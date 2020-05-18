using System;
using System.Collections.Generic;
using System.Text;

namespace ToolInterfaces
{
    public interface ISolution
    {
        string Name { get; }

        List<string> SolutionLines { get; set; }
    }
}
