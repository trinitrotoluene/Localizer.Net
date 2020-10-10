using System;
using System.Collections.Generic;

namespace Localizer.Net
{
    public sealed class LocalizationBuilder
    {
        public ILocaleLoader LocaleLoader { get; private set; }

        public string PathSeparator { get; private set; } = ".";

        public string DefaultLocale { get; private set; } = null;

        public LocalizationBuilder()
        {
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

        public LocalizationBuilder WithLocaleLoader(ILocaleLoader localeLoader)
        {
            LocaleLoader = localeLoader;
            return this;
        }

        public ILocalization Build()
        {
            if (DefaultLocale != null && !LocaleLoader.Supports(DefaultLocale))
            {
                throw new LocalizerException($"The specified default locale \"{DefaultLocale}\" does not exist.");
            }

            return new DefaultLocalization(LocaleLoader, DefaultLocale);
        }
    }
}
