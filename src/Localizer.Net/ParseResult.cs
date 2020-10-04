using Microsoft.CodeAnalysis.Scripting;

namespace Localizer.Net
{
    internal class ParseResult
    {
        public int EndIndex { get; }

        public string Text { get; }

        public Script<object> Script { get; internal set; }

        public ParseResultType Type { get; }

        public ParseResult(ParseResultType type, int endIndex, string text)
        {
            EndIndex = endIndex;
            Text = text;
            Type = type;
        }
    }
}
