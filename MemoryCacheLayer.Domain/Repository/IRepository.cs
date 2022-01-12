namespace MemoryCacheLayer.Domain.Repository
{
    public interface IRepository<TDatabaseItem> where TDatabaseItem : class
    {
        void Delete(string key, int id);
        void Upsert(string key, TDatabaseItem value);

        IEnumerable<TDatabaseItem> Get(string key);
    }
}