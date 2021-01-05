using System;

namespace Localizer.Net
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LocalizeAttribute : Attribute
    {
        public string Path { get; }

        /// <summary>
        /// Tags a field as being a valid target for injecting a localised string. USE ONLY ON FIELDS WITH TYPE <see cref="LocalizedString"/>
        /// </summary>
        /// <param name="path">The unique path of the string to search locales for.</param>
        /// <exception cref="ArgumentException">Thrown when the input string is null or whitespace.</exception>
        public LocalizeAttribute(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"{nameof(path)} cannot be null or empty.");
            }

            Path = path;
        }
    }
}
