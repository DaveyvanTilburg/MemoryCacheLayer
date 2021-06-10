using System.Collections.Generic;

namespace MemoryCacheLayer.Sql
{
    public interface ISqlDatabase<T> where T : IDatabaseItem<T>
    {
        void Save(T value);

        IEnumerable<T> Get();
    }
}