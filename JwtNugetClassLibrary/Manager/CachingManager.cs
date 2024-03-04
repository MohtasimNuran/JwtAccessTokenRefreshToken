using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtNugetClassLibrary.Manager
{
    public class CachingManager
    {
        private readonly IMemoryCache _cache;
        private readonly HashSet<string> _keys;

        public CachingManager(IMemoryCache cache)
        {
            _cache = cache;
            _keys = new HashSet<string>();
        }

        public void Set<TItem>(object key, TItem value)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                //SlidingExpiration = TimeSpan.FromMinutes(5),
                Size = 1024,
            };

            _cache.Set(key, value, cacheEntryOptions);

            _keys.Add((string)key);
        }

        public bool IsExists(object key, string token)
        {
            if (_cache.TryGetValue(key, out string outToken))
            {
                return token == outToken;
            }
            else
            {
                return false;
            }
        }

        public object Get(object key)
        {
            return _cache.Get(key)!;
        }

        public void RemoveBykey(object key)
        {
            _cache.Remove(key);

            _keys.Remove((string)(key));
        }

        public void RemoveItemsByKeyContains(string substring)
        {
            foreach (var key in _keys.ToArray())
            {
                if (key.Contains(substring))
                {
                    _cache.Remove(key); // Remove item from cache
                    _keys.Remove(key); // Remove key from the list
                }
            }
        }
    }
}
