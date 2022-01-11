using System.Collections.Generic;

namespace MemoryCacheLayer.Sql
{
    public interface ISqlDatabase<T> where T : class, IDatabaseItem<T>, new()
    {
        void Save(string key, T value);

        IEnumerable<T> Get(string key);
    }
}