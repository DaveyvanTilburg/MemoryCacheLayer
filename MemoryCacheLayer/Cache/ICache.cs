using System.Collections.Generic;
using MemoryCacheLayer.Expressions;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Cache
{
    public interface ICache<T> : ISqlDatabase<T> where T : class, IDatabaseItem<T>, new()
    {
        IEnumerable<T> Where(params IExpression<T>[] expression);
        T First(params IExpression<T>[] expressions);
    }
}