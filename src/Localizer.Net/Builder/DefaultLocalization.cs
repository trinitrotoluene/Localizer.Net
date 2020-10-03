using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Localizer.Net
{
    internal class DefaultLocalization : ILocalization
    {
        private readonly IDictionary<string, Locale> _locales;

        public DefaultLocalization(IDictionary<string, Locale> locales)
        {
            _locales = locales;
        }

        public string Resolve(string locale, string path, params (string name, object value)[] context)
        {
            var localeImpl = _locales[locale];
            if (!localeImpl.TryGet(path, out var locString))
            {
                throw new LocalizerException($"Locale {localeImpl.Tag} has no value for path {path}!");
            }

            var parseResults = new List<ParseResult>();
            var sb = new StringBuilder();
            ParseResult lastResult = null;

            foreach (var result in Parse(sb, locString))
            {
                lastResult = result;
                parseResults.Add(result);
            }

            if (parseResults.Count > 0)
            {
                if (lastResult?.EndIndex > 0 && lastResult.EndIndex != (locString.Length - 1))
                {
                    var startIndex = lastResult.EndIndex + 1;
                    sb.Append(locString.Substring(startIndex, locString.Length - startIndex));
                }

                var args = new Dictionary<string, object>();

                foreach (var contextItem in context)
                {
                    args[contextItem.name] = contextItem.value;
                }

                var globals = new Globals(args);
                var injectionString = CreateInjectionString(globals);

                var results = new object[parseResults.Count];
                for (int i = 0; i < parseResults.Count; i++)
                {
                    var result = CSharpScript.EvaluateAsync(
                        injectionString + parseResults[i].Text,
                        ScriptOptions.Default.WithImports("System"),
                        globals
                    )
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                    results[i] = result;
                }

                var formatString = sb.ToString();
                var formattedString = string.Format(formatString, results);

                return formattedString;
            }
            else
            {
                return locString;
            }
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

        private IEnumerable<ParseResult> Parse(StringBuilder output, string value)
        {
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
                yield return lastResult;
            }
        }
    }

    public class Globals
    {
        public Dictionary<string, object> Args { get; private set; }

        public Globals(Dictionary<string, object> args)
        {
            Args = args;
        }
    }
}
