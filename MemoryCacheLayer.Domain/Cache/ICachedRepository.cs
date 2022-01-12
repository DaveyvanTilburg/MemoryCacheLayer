using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Domain.Cache
{
    public interface ICachedRepository<TDatabaseItem, TClone> where TDatabaseItem : class, IRepositoryItem<TClone> where TClone : struct, ICloneItem
    {
        void Delete(string key, int id);
        void Upsert(string key, TClone value);
        IEnumerable<TClone> Where(string key, Func<IEnumerable<TDatabaseItem>, IEnumerable<TDatabaseItem>> filter);
        TClone One(string key, Func<IEnumerable<TDatabaseItem>, TDatabaseItem> filter);
        void Clear(string key);
        int InCacheCount(string key);
    }
}