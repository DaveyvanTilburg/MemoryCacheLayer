using System;

namespace MemoryCacheLayer.Expressions
{
    public interface IExpression<in T>
    {
        Func<T, bool> Lambda();
    }
}