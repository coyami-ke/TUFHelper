using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ADOFAIModdingHelper.Utilities
{
    public static class IdentifierUtils
    {
        private static readonly HashSet<string> CSharpKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
            "checked", "class", "const", "continue", "decimal", "default", "delegate", "do",
            "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed",
            "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
            "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public", "readonly", "ref",
            "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
        };

        public static string Sanitize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "_Mod";

            string sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_]", "");

            if (Regex.IsMatch(sanitized, @"^\d"))
                sanitized = "_" + sanitized;

            if (string.IsNullOrEmpty(sanitized))
                sanitized = "_Mod";

            if (CSharpKeywords.Contains(sanitized))
                sanitized = "_" + sanitized;

            return sanitized;
        }
    }
}