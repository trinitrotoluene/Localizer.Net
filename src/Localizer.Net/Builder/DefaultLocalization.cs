using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Localizer.Net
{
    internal class DefaultLocalization : ILocalization
    {
        private readonly IDictionary<string, Locale> _locales;

        private readonly Locale _defaultLocale = null;

        public DefaultLocalization(IDictionary<string, Locale> locales, string defaultLocale = null)
        {
            _locales = locales;
            if (defaultLocale != null)
            {
                _defaultLocale = locales[defaultLocale];
            }
        }

        public string Resolve(string locale, string path, params (string name, object value)[] context)
        {
            if (!_locales.TryGetValue(locale, out var localeImpl))
            {
                throw new LocalizerException($"No locale exists with tag {locale}!");
            }

            if (!localeImpl.TryGet(path, out var locString))
            {
                if (!(_defaultLocale?.TryGet(path, out locString) ?? false))
                {
                    throw new LocalizerException($"Locale {localeImpl.Tag} has no value for path {path}!");
                }

                localeImpl = _defaultLocale;
            }

            if (localeImpl.TryGetFromCache(path, out var cachedPathItem))
            {
                switch (cachedPathItem.Type)
                {
                    case CacheItemType.Literal:
                        return locString;
                    case CacheItemType.Formatted:
                        return ExecuteScriptsAndFormat(cachedPathItem, context);
                    default:
                        Debug.Fail("Invalid CacheItemType passed to PathCacheItem!");
                        throw new Exception();
                }
            }
            else
            {
                var sb = new StringBuilder();
                ParseResult lastResult = null;
                var parseResults = Parse(sb, locString).ToArray();
                if (parseResults.Length > 0)
                {
                    lastResult = parseResults[parseResults.Length - 1];
                }

                PathCacheItem cacheItem;
                if (parseResults.Length > 0)
                {
                    if (lastResult?.EndIndex > 0 && lastResult.EndIndex != (locString.Length - 1))
                    {
                        var startIndex = lastResult.EndIndex + 1;
                        sb.Append(locString.Substring(startIndex, locString.Length - startIndex));
                    }

                    cacheItem = new PathCacheItem(CacheItemType.Formatted, sb.ToString(), parseResults);
                    localeImpl.TryAdd(path, cacheItem);

                    var results = CreateAndExecuteScripts(cacheItem, context);

                    var formatString = sb.ToString();
                    var formattedString = string.Format(formatString, results);

                    return formattedString;
                }
                else
                {
                    cacheItem = new PathCacheItem(CacheItemType.Literal, locString, null);
                    localeImpl.TryAdd(path, cacheItem);
                    return locString;
                }
            }
        }

        private string ExecuteScriptsAndFormat(PathCacheItem item, (string name, object value)[] context)
        {
            var args = CreateGlobalDictionary(context);
            var globals = new Globals(args);

            var results = new object[item.ParseResults.Length];
            for (int i = 0; i < item.ParseResults.Length; i++)
            {
                var result = item.ParseResults[i].Script.RunAsync(globals)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                results[i] = result.ReturnValue;
            }

            return string.Format(item.FormatString, results);
        }

        private object[] CreateAndExecuteScripts(PathCacheItem item, (string name, object value)[] context)
        {
            var parseResults = item.ParseResults;

            var args = CreateGlobalDictionary(context);
            var globals = new Globals(args);
            var injectionString = CreateInjectionString(globals);

            var results = new object[parseResults.Length];
            for (int i = 0; i < parseResults.Length; i++)
            {
                var script = CSharpScript.Create(
                    injectionString + parseResults[i].Text,
                    ScriptOptions.Default.WithImports("System"),
                    typeof(Globals)
                );
                script.Compile();
                parseResults[i].Script = script;

                var result = script.RunAsync(globals)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                results[i] = result.ReturnValue;
            }

            return results;
        }

        private Dictionary<string, object> CreateGlobalDictionary((string name, object value)[] context)
        {
            var args = new Dictionary<string, object>();

            foreach (var contextItem in context)
            {
                args[contextItem.name] = contextItem.value;
            }

            return args;
        }

        private string CreateInjectionString(Globals globals)
        {
            var sb = new StringBuilder();
            foreach (var kvp in globals.Args)
            {
                var typeName = kvp.Value.GetType().Name;
                sb.AppendLine($"{typeName} {kvp.Key} = ({typeName}) Args[\"{kvp.Key}\"];");
            }
            return sb.ToString();
        }

        private List<ParseResult> Parse(StringBuilder output, string value)
        {
            var parseList = new List<ParseResult>();

            int number = 0;

            int index = -1;
            int closingIndex = 0;
            ParseResult lastResult = null;

            while (index < value.Length)
            {
                index = value.IndexOf('{', index + 1);

                if (index == -1)
                {
                    break;
                }

                closingIndex = value.IndexOf('}', closingIndex + 1);
                if (closingIndex == -1)
                {
                    throw new LocalizerException($"OpeningBraceIndex: {index}, missing closing brace! Source: {value}");
                }

                var length = closingIndex - (index + 1);
                if (length < 1)
                {
                    throw new LocalizerException($"Empty context at index {index}! Source: {value}");
                }

                string rawTextBetween;
                if (number == 0)
                {
                    rawTextBetween = value.Substring(0, index);
                }
                else
                {
                    var betweenIndex = lastResult.EndIndex + 1;
                    rawTextBetween = value.Substring(betweenIndex, index - betweenIndex);
                }

                output.Append(rawTextBetween);

                output.Append('{')
                    .Append(number++)
                    .Append('}');

                var scriptText = value.Substring(index + 1, length);
                scriptText = scriptText.Replace('\'', '\"');
                lastResult = new ParseResult(closingIndex, scriptText);

                parseList.Add(lastResult);
            }

            return parseList;
        }
    }
}
