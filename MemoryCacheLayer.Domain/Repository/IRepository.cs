namespace MemoryCacheLayer.Domain.Repository
{
    public interface IRepository<T> where T : struct, IRepositoryItem
    {
        void Delete(string key, int id);
        void Update(string key, T value);
        int Insert(string key, T value);

        IEnumerable<T> Get(string key);
    }
}