using Microsoft.CodeAnalysis.Scripting;

namespace Localizer.Net
{
    internal class ParseResult
    {
        public int EndIndex { get; }

        public string Text { get; }

        public Script<object> Script { get; internal set; }

        public ParseResultType Type { get; } // todo

        public ParseResult(int endIndex, string text)
        {
            EndIndex = endIndex;
            Text = text;
        }
    }

    internal enum ParseResultType
    {
        Simple,
        Complex
    }
}
