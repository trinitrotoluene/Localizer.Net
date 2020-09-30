using System.Collections.Generic;

namespace Localizer.Net
{
    public class Locale
    {
        public string Tag { get; private set; }

        private readonly IDictionary<string, string> _values;

        public Locale(string tag, IDictionary<string, string> values)
        {
            Tag = tag;
            _values = values;
        }

        public string Get(string path)
        {
            return _values[path];
        }
    }
}
