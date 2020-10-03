using System;
using System.Collections.Generic;

namespace Localizer.Net
{
    public sealed class LocalizationBuilder
    {
        public string PathSeparator { get; private set; } = ".";

        public string DefaultLocale { get; private set; } = null;

        private readonly List<Func<LocalizationBuilder, IEnumerable<Locale>>> _builders;

        public LocalizationBuilder()
        {
            _builders = new List<Func<LocalizationBuilder, IEnumerable<Locale>>>();
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

        public LocalizationBuilder WithDefaultLocale(string localeName)
        {
            if (string.IsNullOrEmpty(localeName))
            {
                throw new ArgumentException($"The name of a default locale may not be null or empty.");
            }

            DefaultLocale = localeName;

            return this;
        }

        public LocalizationBuilder WithLocaleGenerator(Func<LocalizationBuilder, IEnumerable<Locale>> generatorFunc)
        {
            _builders.Add(generatorFunc);
            return this;
        }

        public ILocalization Build()
        {
            var finalDict = new Dictionary<string, Locale>();
            foreach (var builder in _builders)
            {
                var locales = builder(this);
                foreach (var locale in locales)
                {
                    if (finalDict.ContainsKey(locale.Tag))
                    {
                        throw new LocalizerException($"A locale with tag {locale.Tag} already exists in this builder.");
                    }

                    finalDict.Add(locale.Tag, locale);
                }
            }

            return new DefaultLocalization(finalDict);
        }
    }
}
