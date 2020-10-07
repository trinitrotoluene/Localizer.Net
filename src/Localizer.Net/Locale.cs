using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Localizer.Net
{
    public class Locale
    {
        public string Tag { get; private set; }

        private readonly IDictionary<string, string> _values;

        private readonly ConcurrentDictionary<string, PathCacheItem> _pathCache;

        public Locale(string tag, IDictionary<string, string> values)
        {
            Tag = tag;
            _values = values;
            _pathCache = new ConcurrentDictionary<string, PathCacheItem>();
        }

        public bool TryGet(string path, out string value)
        {
            return _values.TryGetValue(path, out value);
        }

        internal bool TryGetFromCache(string path, out PathCacheItem value)
        {
            return _pathCache.TryGetValue(path, out value);
        }

        internal void TryAdd(string path, PathCacheItem value)
        {
            _pathCache.TryAdd(path, value);
        }
    }
}
