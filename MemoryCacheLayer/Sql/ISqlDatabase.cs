using System.Collections.Generic;

namespace MemoryCacheLayer.Sql
{
    public interface ISqlDatabase<out TDatabaseItem, in TClone> where TDatabaseItem : class where TClone : struct, ICloneItem
    {
        void Save(string key, TClone value);

        IEnumerable<TDatabaseItem> Get(string key);
    }
}