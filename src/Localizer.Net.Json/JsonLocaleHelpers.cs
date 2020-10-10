using System.Collections.Generic;
using System.Text.Json;

namespace Localizer.Net.Json
{
    public static class JsonLocaleHelpers
    {
        public static void FlattenObject(string pathSeparator, JsonProperty jsonProperty, Dictionary<string, string> valuePairs, string keyPrefix = "")
        {
            switch (jsonProperty.Value.ValueKind)
            {
                case JsonValueKind.String:

                    if (keyPrefix == "")
                        valuePairs[keyPrefix + jsonProperty.Name] = jsonProperty.Value.GetString();
                    else
                        valuePairs[string.Concat(keyPrefix + pathSeparator + jsonProperty.Name)] = jsonProperty.Value.GetString();

                    break;
                case JsonValueKind.Object:

                    foreach (var innerProperty in jsonProperty.Value.EnumerateObject())
                        FlattenObject(pathSeparator, innerProperty, valuePairs, keyPrefix + jsonProperty.Name);

                    break;
            }
        }
    }
}
