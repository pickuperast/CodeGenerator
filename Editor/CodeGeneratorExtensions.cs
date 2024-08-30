using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanat.CodeGenerator.Extensions
{
    public static class CodeGeneratorExtensions
    {
        public static string RemoveLastNRows(string input, int n)
        {
            if (string.IsNullOrEmpty(input) || n <= 0)
            {
                return input;
            }

            string[] lines = input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (lines.Length <= n)
            {
                return string.Empty;
            }

            return string.Join(Environment.NewLine, lines.Take(lines.Length - n));
        }

        public static string RemoveComments(string code)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//.*?$";
            var strings = @"""(?:\\.|[^""\\])*""";
            var verbatimStrings = @"@""(?:""""|[^""])*""";

            string noComments = Regex.Replace(code,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me => {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Return the matched string literal unchanged
                    return me.Value;
                },
                RegexOptions.Multiline);

            return noComments;
        }
    }
}