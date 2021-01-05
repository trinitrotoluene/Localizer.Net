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
        private readonly ILocaleLoader _localeLoader;

        private readonly Locale _defaultLocale = null;

        private readonly ScriptOptions _scriptOptions = null;

        public DefaultLocalization(ILocaleLoader localeLoader, ScriptOptions scriptOptions, string defaultLocale = null)
        {
            _scriptOptions = scriptOptions;
            _localeLoader = localeLoader;
            if (defaultLocale != null)
            {
                _defaultLocale = _localeLoader.Load(defaultLocale);
            }
        }

        public string Resolve(string path, params (string name, object value)[] context)
        {
            if (_defaultLocale == null)
            {
                throw new InvalidOperationException(
                    "Cannot invoke default locale overloads without setting a default locale!");
            }

            return Resolve(_defaultLocale.Tag, path, context);
        }

        public string Resolve(string locale, string path, params (string name, object value)[] context)
        {
            if (!_localeLoader.Supports(locale))
            {
                throw new LocalizerException($"Cannot request an unsupported locale from a localisation! Tag: {locale}!");
            }

            var localeImpl = _localeLoader.Load(locale);
            if (!localeImpl.TryGet(path, out var locString))
            {
                if (!(_defaultLocale?.TryGet(path, out locString) ?? false))
                {
                    throw new LocalizerException($"Locale {localeImpl.Tag} has no value for path {path}!");
                }

                localeImpl = _defaultLocale;
            }

            // If we have this path cached take the fast paths
            if (localeImpl.TryGetFromCache(path, out var cachedPathItem))
            {
                switch (cachedPathItem.Type)
                {
                    // Return the string directly
                    case CacheItemType.Literal:
                        return locString;
                    // Execute embedded scripts and format them into the returned string
                    case CacheItemType.Formatted:
                        var args = CreateGlobalDictionary(context);
                        return ExecuteScriptsAndFormat(cachedPathItem, args);
                    default:
                        Debug.Fail("Invalid CacheItemType passed to PathCacheItem!");
                        throw new Exception();
                }
            }

            // Parse for scripts
            var sb = new StringBuilder();
            ParseResult lastResult = null;
            var parseResults = Parse(sb, locString).ToArray();
            if (parseResults.Length > 0)
            {
                lastResult = parseResults[parseResults.Length - 1];
            }

            // If there are scripts embedded in this string
            PathCacheItem cacheItem;
            if (parseResults.Length > 0)
            {
                // Add any trailing characters to the format string
                if (lastResult?.EndIndex > 0 && lastResult.EndIndex != (locString.Length - 1))
                {
                    var startIndex = lastResult.EndIndex + 1;
                    sb.Append(locString.Substring(startIndex, locString.Length - startIndex));
                }

                // Compile the scripts for each result from context
                var args = CreateGlobalDictionary(context);
                PopulateScripts(parseResults, args);

                cacheItem = new PathCacheItem(CacheItemType.Formatted, sb.ToString(), parseResults);
                localeImpl.TryAdd(path, cacheItem);

                var resultString = ExecuteScriptsAndFormat(cacheItem, args);
                return resultString;
            }

            // No scripts, no problem.
            cacheItem = new PathCacheItem(CacheItemType.Literal, locString, null);
            localeImpl.TryAdd(path, cacheItem);
            return locString;
        }

        private void PopulateScripts(ParseResult[] parseResults, Dictionary<string, object> args)
        {
            var globals = new Globals(args);
            var injectionString = CreateInjectionString(globals);

            for (int i = 0; i < parseResults.Length; i++)
            {
                switch (parseResults[i].Type)
                {
                    case ParseResultType.Simple:
                        continue;
                    case ParseResultType.Complex:
                        if (_scriptOptions == null)
                        {
                            throw new InvalidOperationException("Localization string contains a script replacement but script execution has been disabled by passing null ScriptOptions to the LocalizationBuilder.");
                        }
                        var script = CSharpScript.Create(
                            injectionString + parseResults[i].Text,
                            _scriptOptions,
                            typeof(Globals)
                        );
                        script.Compile();
                        parseResults[i].Script = script;
                        continue;
                    default:
                        Debug.Fail("Invalid ParseResultType passed to ParseResult!");
                        throw new Exception();
                }
            }
        }

        private string ExecuteScriptsAndFormat(PathCacheItem item, Dictionary<string, object> args)
        {
            var globals = new Globals(args);

            var results = new object[item.ParseResults.Length];
            for (int i = 0; i < item.ParseResults.Length; i++)
            {
                switch (item.ParseResults[i].Type)
                {
                    case ParseResultType.Simple:
                        results[i] = args[item.ParseResults[i].Text];
                        continue;
                    case ParseResultType.Complex:
                        results[i] = item.ParseResults[i].Script.RunAsync(globals)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult()
                            .ReturnValue;
                        continue;
                    default:
                        Debug.Fail("Invalid ParseResultType passed to ParseResult!");
                        throw new Exception();
                }
            }

            return string.Format(item.FormatString, results);
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
            var valueSpan = value.AsSpan();

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

                ReadOnlySpan<char> rawTextBetween;
                if (number == 0)
                {
                    rawTextBetween = valueSpan.Slice(0, index);
                }
                else
                {
                    var betweenIndex = lastResult.EndIndex + 1;
                    rawTextBetween = valueSpan.Slice(betweenIndex, index - betweenIndex);
                }

                output.Append(rawTextBetween);

                output.Append('{')
                    .Append(number++)
                    .Append('}');

                var scriptSpan = valueSpan.Slice(index + 1, length);
                var scriptBuilder = new StringBuilder();
                for (int i = 0; i < scriptSpan.Length; i++)
                {
                    char scriptChar = scriptSpan[i];

                    if (scriptChar == '\'')
                        scriptBuilder.Append('"');
                    else
                        scriptBuilder.Append(scriptChar);
                }

                var scriptText = scriptBuilder.ToString();
                var resultType = GetResultType(scriptText);
                lastResult = new ParseResult(resultType, closingIndex, scriptText);

                parseList.Add(lastResult);
            }

            return parseList;
        }

        private ParseResultType GetResultType(string script)
        {
            for (int i = 0; i < script.Length; i++)
            {
                if (!char.IsLetterOrDigit(script[i]))
                {
                    return ParseResultType.Complex;
                }
            }

            return ParseResultType.Simple;
        }
    }
}
