namespace MemoryCacheLayer.Domain.Repository
{
    public interface IRepository<T> where T : IRepositoryItem
    {
        void Delete(string key, int id);
        void Upsert(string key, T value);

        IEnumerable<T> Get(string key);
    }
}