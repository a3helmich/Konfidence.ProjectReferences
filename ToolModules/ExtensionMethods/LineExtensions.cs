using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ToolModules.ExtensionMethods
{
    public static class LineExtensions
    {
        public static string TrimStart(this string line, [NotNull] string replacement, bool exact = false)
        {
            if (replacement.IsAssigned() && line.StartsWith(replacement))
            {
                line = line.Substring(replacement.Length);

                if (!exact)
                {
                    line = line.TrimStart();
                }
            }

            return line;
        }

        public static string SplitStart(this string line, [NotNull] string splitAt)
        {
            var searchLine = line;

            if (searchLine.Contains(splitAt))
            {
                searchLine = searchLine.Substring(0, searchLine.IndexOf(splitAt, StringComparison.InvariantCultureIgnoreCase)).TrimEnd();

                return searchLine;
            }

            searchLine = TrimEnd(searchLine, splitAt.TrimEnd());

            return searchLine;
        }

        public static string TrimEnd(this string line, [NotNull] string replacement, bool exact = false)
        {
            if (replacement.IsAssigned() && line.EndsWith(replacement))
            {
                line = line.Substring(0, line.Length - replacement.Length);

                if (!exact)
                {
                    line = line.TrimEnd();
                }
            }

            return line;
        }

        public static string RemoveStartingComment(this string line, [NotNull] out string startingComment)
        {
            startingComment = string.Empty;

            if (line.StartsWith("{") && line.Contains("}"))
            {
                startingComment = line.Substring(0, line.IndexOf("}", StringComparison.Ordinal) + 1).Trim();

                line = TrimStart(line, startingComment);

                return line;
            }

            return line;
        }

        public static string RemoveEndingComment(this string line, [NotNull] out string endingComment)
        {
            endingComment = string.Empty;

            if (!line.StartsWith("//") && line.Contains("//"))
            {
                endingComment = line.Substring(line.LastIndexOf("//", StringComparison.Ordinal)).Trim();

                line = TrimEnd(line, endingComment);

                return line;
            }

            if (line.EndsWith("}") && line.Contains("{"))
            {
                endingComment = line.Substring(line.LastIndexOf("{", StringComparison.Ordinal)).Trim();

                line = TrimEnd(line, endingComment);

                return line;
            }

            return line;
        }

        public static bool HasUnbalancedCommentCurleys(this string line)
        {
            line = line.ExtractQuotedStrings(out _);

            return line.Count(x => x == '{') > line.Count(x => x == '}');
        }

        public static bool IsNumeric(this string parameterPart)
        {
            var searchParameterPart = parameterPart;

            if (long.TryParse(searchParameterPart, out _)) return true;

            if (double.TryParse(searchParameterPart, out _)) return true;

            if (decimal.TryParse(searchParameterPart, out _)) return true;

            return false;
        }

        public static bool IsLiteral([NotNull] this string parameterPart)
        {
            return parameterPart.StartsWith("\"") && parameterPart.EndsWith("\"");
        }

        public static string ExtractQuotedStrings([NotNull] this string parameterLine, [NotNull] out Dictionary<string, string> quotedStrings)
        {
            quotedStrings = new Dictionary<string, string>();
            const string quote = "'";

            if (!parameterLine.Contains(quote))
            {
                return parameterLine;
            }

            var returnParameterLine = string.Empty;
            var searchParameterLine = parameterLine;
            var searchParameterLineLength = 0;

            while (searchParameterLine.Contains(quote) && searchParameterLineLength != searchParameterLine.Length)
            {
                searchParameterLineLength = searchParameterLine.Length;

                var beforeKey = searchParameterLine.Substring(0, searchParameterLine.IndexOf(quote, StringComparison.Ordinal));

                searchParameterLine = TrimStart(searchParameterLine, beforeKey);

                var key = Guid.NewGuid().ToString("N");
                var value = searchParameterLine.Substring(0, searchParameterLine.IndexOf(quote, 1, StringComparison.Ordinal) + 1);

                returnParameterLine += beforeKey + key;

                searchParameterLine = searchParameterLine.TrimStart(value, true);

                quotedStrings.Add(key, value);
            }

            if (searchParameterLine.IsAssigned() && searchParameterLine != returnParameterLine)
            {
                returnParameterLine += searchParameterLine;
            }

            return returnParameterLine;
        }

        public static string InjectExtractedStrings(this string cLine, [NotNull] Dictionary<string, string> literalDictionary)
        {
            var searchLine = cLine;

            foreach (var kvp in literalDictionary)
            {
                searchLine = searchLine.Replace(kvp.Key, kvp.Value);
            }

            return searchLine;
        }
    }
}