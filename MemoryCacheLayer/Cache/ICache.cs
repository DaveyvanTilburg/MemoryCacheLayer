using System;
using System.Collections.Generic;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Cache
{
    public interface ICache<TDatabaseItem, TClone> where TDatabaseItem : class, IDatabaseItem<TClone> where TClone : struct, ICloneItem
    {
        IEnumerable<TClone> Where(string key, Func<IEnumerable<TDatabaseItem>, IEnumerable<TDatabaseItem>> filter);
        TClone One(string key, Func<IEnumerable<TDatabaseItem>, TDatabaseItem> filter);
        void Clear(string key);
        int InCacheCount(string key);
        void Save(string key, TClone value);
    }
}