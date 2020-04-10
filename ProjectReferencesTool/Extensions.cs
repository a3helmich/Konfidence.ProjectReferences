namespace ProjectReferencesTool
{
    internal static class Extensions
    {
        internal static string TrimStart(this string text, string trimStart)
        {
            if (text.StartsWith(trimStart))
            {
                return text[trimStart.Length..].TrimStart();
            }

            return text;
        }
        internal static string TrimEnd(this string text, string trimEnd)
        {
            if (text.EndsWith(trimEnd))
            {
                return text[..^trimEnd.Length].TrimEnd();
            }

            return text;
        }
    }
}
