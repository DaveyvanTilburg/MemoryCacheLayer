using System;
using System.Collections.Generic;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Cache
{
    public interface ICache<T> : ISqlDatabase<T> where T : class, IDatabaseItem<T>, new()
    {
        IEnumerable<T> Where(string key, Func<IEnumerable<T>, IEnumerable<T>> filter);
        T One(string key, Func<IEnumerable<T>, T> filter);
        void Clear(string key);
        int InCacheCount(string key);
    }
}