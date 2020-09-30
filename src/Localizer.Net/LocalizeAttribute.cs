using System;

namespace Localizer.Net
{
    [AttributeUsage(AttributeTargets.Field)]
    public class LocalizeAttribute : Attribute
    {
        private readonly string _path;

        public string Path { get => _path; }

        public LocalizeAttribute(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"{nameof(path)} cannot be null or empty.");
            }

            _path = path;
        }
    }
}
