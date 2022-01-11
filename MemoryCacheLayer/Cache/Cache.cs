using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Cache
{
    public class Cache<T> : ICache<T> where T : class, IDatabaseItem<T>, new()
    {
        private readonly ISqlDatabase<T> _sqlDatabase;
        private readonly MemoryCache _cache;

        public Cache(ISqlDatabase<T> sqlDatabase)
        {
            _sqlDatabase = sqlDatabase;
            _cache = MemoryCache.Default;
        }

        void ISqlDatabase<T>.Save(string key, T value)
        {
            List<T> list = List(key);
            T cachedItem = list.FirstOrDefault(i => i.Id() == value.Id());

            if (cachedItem == null)
            {
                list.Add(value);
                _sqlDatabase.Save(key, value);
                return;
            }

            if (value.Equals(cachedItem))
                return;

            list.Remove(cachedItem);
            list.Add(value);

            _sqlDatabase.Save(key, value);
        }

        IEnumerable<T> ISqlDatabase<T>.Get(string key)
            => List(key).Select(i => i.Clone());

        IEnumerable<T> ICache<T>.Where(string key, Func<IEnumerable<T>, IEnumerable<T>> filter)
        {
            List<T> items = List(key);
            IEnumerable<T> result = new List<T>(items);
            result = filter(result).Select(i => i.Clone());

            return result;
        }

        T ICache<T>.One(string key, Func<IEnumerable<T>, T> filter)
        {
            List<T> items = List(key);
            IEnumerable<T> sourceItems = new List<T>(items);
            T result = filter(sourceItems);

            return result?.Clone() ?? new T();
        }

        void ICache<T>.Clear(string key)
            => _cache.Remove(CacheKey(key));

        int ICache<T>.InCacheCount(string key)
            => ((List<T>)_cache.Get(CacheKey(key)))?.Count ?? 0;

        private List<T> List(string key)
        {
            string cacheKey = CacheKey(key);
            List<T> source = (List<T>)_cache.Get(cacheKey);

            if (source != null)
                return source;

            source = _sqlDatabase.Get(key).ToList();
            _cache.Add(cacheKey, source, DateTimeOffset.MaxValue);

            return source;
        }

        private string CacheKey(string key)
            => $"ListOf({typeof(T).Name})-Key({key})";
    }
}