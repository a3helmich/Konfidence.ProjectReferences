using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using Konfidence.Base;

namespace ToolClasses.ExtensionMethods
{
    public static class LineExtensions
    {
        public static string WriteLine(this string line)
        {
            Console.WriteLine(line);
            Debug.WriteLine($"=> {line}");

            return line;
        }

        [NotNull]
        public static string TrimQuotes([NotNull] this string text)
        {
            return text.TrimStart("\"").TrimEnd("\"");
        }
    }
}