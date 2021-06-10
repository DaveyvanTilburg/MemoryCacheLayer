using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Cache
{
    public class Cache<T> : ICache<T> where T : IDatabaseItem<T>
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

        public IEnumerable<T> Get(params Expression<Func<T, bool>>[] expressions)
        {
            List<T> items = List();
            IEnumerable<T> result = new List<T>(items);

            foreach (Expression<Func<T, bool>> expression in expressions)
                result = result.Where(expression.Compile());

            return result.Select(i => i.Clone());
        }

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