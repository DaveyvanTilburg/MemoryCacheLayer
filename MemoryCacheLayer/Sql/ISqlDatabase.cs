using System.Collections.Generic;

namespace MemoryCacheLayer.Sql
{
    public interface ISqlDatabase<T> where T : class, IDatabaseItem<T>, new()
    {
        void Save(T value);

        IEnumerable<T> Get();
    }
}