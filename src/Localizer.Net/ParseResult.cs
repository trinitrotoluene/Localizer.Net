namespace Localizer.Net
{
    internal class ParseResult
    {
        public int EndIndex { get; }

        public string Text { get; }

        public ParseResult(int endIndex, string text)
        {
            EndIndex = endIndex;
            Text = text;
        }
    }
}
