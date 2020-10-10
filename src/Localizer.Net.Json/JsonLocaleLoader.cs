using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Localizer.Net.Json
{
    public class JsonLocaleLoader : ILocaleLoader
    {
        private string _path;
        private string _pathSeparator;
        private HashSet<string> _supportedLocales;
        private Dictionary<string, Locale> _localeCache;

        public JsonLocaleLoader(LocalizationBuilder builder, string path)
        {
            _path = path;
            _pathSeparator = builder.PathSeparator;

            _supportedLocales = new HashSet<string>();
            _localeCache = new Dictionary<string, Locale>();

            foreach (var file in Directory.EnumerateFiles(_path, "*.json"))
                _supportedLocales.Add(Path.GetFileNameWithoutExtension(file));
        }

        public Locale Load(string tag)
        {
            if (!Supports(tag))
                throw new LocalizerException($"An unsupported locale was requested from the ${nameof(JsonLocaleLoader)}.");

            lock (_localeCache)
            {
                if (_localeCache.TryGetValue(tag, out var cachedLocale))
                    return cachedLocale;
            }

            return LazyLoad(tag);
        }

        private Locale LazyLoad(string tag)
        {
            var fileName = Path.Combine(_path, tag) + ".json";

            using var file = File.Open(fileName, FileMode.Open);
            using var doc = JsonDocument.Parse(file);

            var valuePairs = new Dictionary<string, string>();
            foreach (var property in doc.RootElement.EnumerateObject())
                JsonLocaleHelpers.FlattenObject(_pathSeparator, property, valuePairs);

            var localeTag = Path.GetFileNameWithoutExtension(fileName);
            var locale = new Locale(localeTag, valuePairs);

            lock(_localeCache)
            {
                _localeCache[tag] = locale;
            }

            return locale;
        }

        public bool Supports(string tag)
        {
            return _supportedLocales.Contains(tag);
        }
    }
}
