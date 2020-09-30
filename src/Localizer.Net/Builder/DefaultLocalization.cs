using System;
using System.Collections.Generic;

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
            // todo: exceptions
            var localeImpl = _locales[locale];
            // todo: find, cache and execute scripts in the localisation string returned by the localeImpl.
            return localeImpl.Get(path);
        }
    }
}
