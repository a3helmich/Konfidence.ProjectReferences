using System;
using System.Diagnostics;

namespace ToolClasses.ExtensionMethods;

public static class LineExtensions
{
    extension(string line)
    {
        public string WriteLine()
        {
            Console.WriteLine(line);
            Debug.WriteLine($"=> {line}");

            return line;
        }
    }
}