using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ToolModules.ExtensionMethods
{
    public static class LinesExtensions
    {
        public static int FindMatchingEnd(this List<string> lines, int fromIndex, string blockEnd)
        {
            if (fromIndex < 0)
            {
                return -1;
            }

            return lines.FindIndex(fromIndex, x => x.TrimEnd().EndsWith(blockEnd));
        }

        public static void RemoveRangeBetween([NotNull] this List<string> lines, int startIndex, int endIndex)
        {
            lines.RemoveRange(startIndex, endIndex + 1 - startIndex);
        }
        public static void RemoveLinesFromTo([NotNull] this List<string> lines, int index, int endIndex)
        {
            var startIndex = index;

            if (endIndex < 0)
            {
                return;
            }

            lines.RemoveRangeBetween(startIndex, endIndex);
        }

        public static void SplitEndIf(this List<string> lines, int index)
        {
            const string endIf = "{$ENDIF}";

            if (lines[index].StartsWith(endIf) && lines[index] != endIf)
            {
                lines[index + 1] = lines[index].TrimStart(endIf);

                lines[index] = endIf;
            }
        }

        [NotNull]
        public static List<string> TrimList([NotNull] this IEnumerable<string> lines)
        {
            return lines.Select(x => x.Trim()).ToList();
        }

        [CanBeNull]
        public static string InitLowerCase(this string cWord)
        {
            if (!string.IsNullOrWhiteSpace(cWord))
            {
                cWord = $"{char.ToLowerInvariant(cWord[0])}{cWord.Substring(1)}";
            }

            return cWord;
        }

        [NotNull]
        public static List<string> GetLinesInBetween([NotNull] this List<string> lines, [NotNull] string startMarker, [NotNull] string endMarker, ref int index)
        {
            var startIndex = index;

            var endIndex = lines.FindMatchingEnd(startIndex, endMarker);

            var line = string.Join(" ", lines.GetRange(startIndex, endIndex + 1 - startIndex)).TrimStart(startMarker).TrimEnd(endMarker);

            index = endIndex;

            return line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).TrimList();
        }

        public static void CommentOutLine([NotNull] this List<string> lines, int index)
        {
            if (!lines[index].StartsWith("//"))
            {
                lines[index] = $"// {lines[index]}";
            }
        }

        public static void SplitLineBeforeWord([NotNull] this List<string> delphiCode, [NotNull] string word, ref int index)
        {
            var beforeWordLine = delphiCode[index].RemoveEndingComment(out var endingComment).TrimEnd(word);
            var wordLine = delphiCode[index].TrimStart(beforeWordLine);

            delphiCode[index] = beforeWordLine;

            delphiCode.Insert(index + 1, $"{wordLine} {endingComment}".TrimEnd());
        }

        public static void SplitLineAfterWord([NotNull] this List<string> delphiCode, [NotNull] string word, ref int index)
        {
            var afterWordLine = delphiCode[index].TrimStart(word);
            var wordLine = delphiCode[index].TrimEnd(afterWordLine);

            delphiCode[index] = wordLine;

            delphiCode.Insert(index + 1, $"{afterWordLine}".TrimEnd());
        }

        public static void ClearLineAt([NotNull] this List<string> lines, int index)
        {
            lines[index] = string.Empty;
        }

        public static void JoinLinesToFullLineAt([NotNull] this List<string> delphiCode, int index)
        {
            while (!delphiCode[index].RemoveEndingComment(out _).EndsWith(";") && index < delphiCode.Count)
            {
                delphiCode[index] += $" {delphiCode[index + 1]}";

                delphiCode.RemoveAt(index + 1);
            }
        }

        public static void JoinAttributeOnLineAt([NotNull] this List<string> lines, int index)
        {
            while (lines[index].Count(x => x == '[') > lines[index].Count(x => x == ']') && index < lines.Count)
            {
                lines[index] += " " + lines[index + 1];

                lines.RemoveAt(index + 1);
            }
        }

        public static void JoinParametersOnLineAt([NotNull] this List<string> lines, int index)
        {
            while (lines[index].Count(x => x == '(') > lines[index].Count(x => x == ')') && index < lines.Count
                   || lines[index].Count(x => x == '(') == 0 && !lines[index].RemoveEndingComment(out _).EndsWith(";"))
            {
                lines[index] += " " + lines[index + 1];

                lines.RemoveAt(index + 1);
            }
        }

        public static void JoinOverloadersOnLineAt([NotNull] this List<string> lines, int index)
        {
            while (lines[index + 1].ToLowerInvariant().StartsWith("overload;")
                   || lines[index + 1].ToLowerInvariant().StartsWith("override;")
                   || lines[index + 1].ToLowerInvariant().StartsWith("virtual;")
                   || lines[index + 1].ToLowerInvariant().StartsWith("external;")
                   || lines[index + 1].ToLowerInvariant().StartsWith("dynamic;")
                   || lines[index + 1].ToLowerInvariant().StartsWith("reintroduce;")
                   || lines[index + 1].ToLowerInvariant().StartsWith("abstract;"))
            {
                var linePart1 = lines[index].RemoveEndingComment(out var endingComment);
                var linePart2 = lines[index + 1].RemoveEndingComment(out var endingComment2).ToLowerInvariant();

                endingComment2 = endingComment2.TrimStart("//");

                lines[index] = $"{linePart1} {linePart2} {endingComment} {endingComment2}".TrimEnd();

                lines.RemoveAt(index + 1);
            }
        }
    }
}