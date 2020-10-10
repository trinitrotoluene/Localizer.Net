using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Localizer.Net.Json;

namespace Localizer.Net
{
    public static class JsonLocalizationBuilderExtensions
    {
        public static LocalizationBuilder UseJsonFiles(this LocalizationBuilder builder, string path)
        {
            builder.WithLocaleLoader(new JsonLocaleLoader(builder, path));
            return builder;
        }
    }
}
