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

        public void Save(T value)
        {
            List<T> list = List();
            T cachedItem = list.FirstOrDefault(i => i.Id() == value.Id());

            if (cachedItem == null)
            {
                list.Add(value);
                _sqlDatabase.Save(value);
                return;
            }

            if (value.Equals(cachedItem))
                return;

            list.Remove(cachedItem);
            list.Add(value);

            _sqlDatabase.Save(value);
        }

        IEnumerable<T> ISqlDatabase<T>.Get()
            => List().Select(i => i.Clone());

        public IEnumerable<T> All()
            => ((ISqlDatabase<T>)this).Get();

        IEnumerable<T> ICache<T>.Where(Func<IEnumerable<T>, IEnumerable<T>> filter)
        {
            List<T> items = List();
            IEnumerable<T> result = new List<T>(items);
            result = filter(result).Select(i => i.Clone());

            return result;
        }

        T ICache<T>.One(Func<IEnumerable<T>, T> filter)
        {
            List<T> items = List();
            IEnumerable<T> sourceItems = new List<T>(items);
            T result = filter(sourceItems);

            return result?.Clone() ?? new T();
        }

        void ICache<T>.Clear()
            => _cache.Remove(CacheKey());

        int ICache<T>.InCacheCount()
            => ((List<T>)_cache.Get(CacheKey()))?.Count ?? 0;

        private List<T> List()
        {
            string cacheKey = CacheKey();
            List<T> source = (List<T>)_cache.Get(cacheKey);

            if (source != null)
                return source;

            source = _sqlDatabase.Get().ToList();
            _cache.Add(cacheKey, source, DateTimeOffset.MaxValue);

            return source;
        }

        private string CacheKey()
            => $"ListOf({typeof(T).Name})";
    }
}