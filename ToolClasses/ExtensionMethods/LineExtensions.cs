using System;
using System.Diagnostics;
using Konfidence.Base;

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

        public string TrimQuotes()
        {
            return line.StartsWith("\"")
                ? line.TrimStart("\"").TrimEnd("\"")
                : line;
        }
    }
}