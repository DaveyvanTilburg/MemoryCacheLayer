using System;
using System.Collections.Generic;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Cache
{
    public interface ICache<T> : ISqlDatabase<T> where T : class, IDatabaseItem<T>, new()
    {
        IEnumerable<T> Where(Func<IEnumerable<T>, IEnumerable<T>> filter);
        T One(Func<IEnumerable<T>, T> filter);
    }
}