using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Localizer.Net
{
    public static class JsonLocalizationBuilderExtensions
    {
        public static LocalizationBuilder UseJsonFiles(this LocalizationBuilder builder, string path)
        {
            builder.WithLocaleGenerator(x => JsonBuilderFunc(x, path));
            return builder;
        }

        private static IEnumerable<Locale> JsonBuilderFunc(LocalizationBuilder builder, string path)
        {
            var locales = new List<Locale>();

            foreach (var fileName in Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories))
            {
                using var file = File.Open(fileName, FileMode.Open);
                using var doc = JsonDocument.Parse(file);

                var valuePairs = new Dictionary<string, string>();
                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    FlattenJsonFile(builder, property, valuePairs);
                }

                var localeTag = Path.GetFileNameWithoutExtension(fileName);
                var locale = new Locale(localeTag, valuePairs);

                locales.Add(locale);
            }

            return locales;
        }

        private static void FlattenJsonFile(LocalizationBuilder builder, JsonProperty jsonProperty, Dictionary<string, string> valuePairs, string keyPrefix = "")
        {
            switch (jsonProperty.Value.ValueKind)
            {
                case JsonValueKind.String:
                    if (keyPrefix == "")
                    {
                        valuePairs[keyPrefix + jsonProperty.Name] = jsonProperty.Value.GetString();
                    }
                    else
                    {
                        valuePairs[string.Concat(keyPrefix + builder.PathSeparator + jsonProperty.Name)] = jsonProperty.Value.GetString();
                    }
                    break;
                case JsonValueKind.Object:
                    foreach (var innerProperty in jsonProperty.Value.EnumerateObject())
                    {
                        FlattenJsonFile(builder, innerProperty, valuePairs, keyPrefix + jsonProperty.Name);
                    }
                    break;
            }
        }
    }
}
