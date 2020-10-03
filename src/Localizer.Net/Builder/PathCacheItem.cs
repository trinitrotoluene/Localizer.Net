using Microsoft.CodeAnalysis.Scripting;

namespace Localizer.Net
{
    internal class PathCacheItem
    {
        public CacheItemType Type { get; }
        public string FormatString { get; }
        public ParseResult[] ParseResults { get; }

        public PathCacheItem(CacheItemType type, string formatString, ParseResult[] parseResults)
        {
            Type = type;
            FormatString = formatString;
            ParseResults = parseResults;
        }
    }
}
