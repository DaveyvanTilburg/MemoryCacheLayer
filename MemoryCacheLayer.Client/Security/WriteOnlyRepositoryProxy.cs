using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Client.Security
{
    internal class WriteOnlyRepositoryProxy<T> : IRepository<T> where T : IRepositoryItem
    {
        private readonly IRepository<T> _child;

        public WriteOnlyRepositoryProxy(IRepository<T> child)
        {
            _child = child;
        }

        public void Delete(string key, int id)
            => _child.Delete(key, id);

        public void Upsert(string key, T value)
            => _child.Upsert(key, value);

        public IEnumerable<T> Get(string key)
            => new List<T>();
    }
}