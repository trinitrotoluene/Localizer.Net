using System;
using System.Collections.Generic;

namespace Localizer.Net
{
    public sealed class LocalizationBuilder
    {
        public string PathSeparator { get; private set; } = ".";

        private readonly IDictionary<string, Locale> _locales;

        public LocalizationBuilder()
        {
            _locales = new Dictionary<string, Locale>();
        }

        public LocalizationBuilder WithPathSeparator(string pathSeparator)
        {
            if (string.IsNullOrEmpty(pathSeparator))
            {
                throw new ArgumentException($"The ${nameof(pathSeparator)} of a localization may not be null or empty.");
            }

            PathSeparator = pathSeparator;

            return this;
        }

        public LocalizationBuilder AddLocale(string key, Locale locale)
        {
            if (_locales.ContainsKey(key))
            {
                throw new InvalidOperationException("A locale already exists with this key.");
            }

            _locales[key] = locale;
            return this;
        }

        public ILocalization Build()
        {
            return new DefaultLocalization(_locales);
        }
    }
}
