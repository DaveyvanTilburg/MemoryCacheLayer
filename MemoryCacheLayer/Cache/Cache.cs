using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using MemoryCacheLayer.Expressions;
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

        IEnumerable<T> ICache<T>.Where(params IExpression<T>[] expressions)
            => WhereLambda(expressions).Select(i => i.Clone());

        T ICache<T>.First(params IExpression<T>[] expressions)
            => WhereLambda(expressions).FirstOrDefault()?.Clone() ?? new T();

        private IEnumerable<T> WhereLambda(params IExpression<T>[] expressions)
        {
            List<T> items = List();
            IEnumerable<T> result = new List<T>(items);

            foreach (IExpression<T> expression in expressions)
                result = result.Where(expression.Lambda());

            return result;
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