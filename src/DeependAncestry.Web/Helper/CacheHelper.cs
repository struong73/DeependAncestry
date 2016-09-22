using DeependAncestry.Web.Interface;
using System;
using System.Runtime.Caching;

namespace DeependAncestry.Web.Helper
{
    public class CacheHelper : ICacheHelper
    {
        private readonly ObjectCache _cache = MemoryCache.Default;

        public void Add(string key, object itemToCache)
        {
            Add(key, itemToCache, 2);
        }

        public void Add(string key, object itemToCache, int cacheTime)
        {
            if (itemToCache != null)
            {
                _cache.Add(key, itemToCache, DateTime.Now.AddMinutes(cacheTime));
            }
        }

        public T Get<T>(string key)
        {
            try
            {
                var cacheResult = _cache[key];
                return (T)cacheResult;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}