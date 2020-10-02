namespace Localizer.Net
{
    internal class ParseResult
    {
        public string FormatString { get; }

        public string[] Scripts { get; }

        public ParseResult(string formatString, string[] scripts)
        {
            FormatString = formatString;
            Scripts = scripts;
        }
    }
}
