using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Cache
{
    public interface ICache<T> : ISqlDatabase<T> where T : IDatabaseItem<T>
    {
        IEnumerable<T> Get(params Expression<Func<T, bool>>[] expression);
    }
}