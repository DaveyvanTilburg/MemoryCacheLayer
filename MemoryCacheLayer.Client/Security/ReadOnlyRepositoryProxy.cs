using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Client.Security
{
    internal class ReadOnlyRepositoryProxy<T> : IRepository<T> where T : IRepositoryItem
    {
        private readonly IRepository<T> _child;

        public ReadOnlyRepositoryProxy(IRepository<T> child)
        {
            _child = child;
        }

        public void Delete(string key, int id)
        {

        }

        public void Upsert(string key, T value)
        {

        }

        public IEnumerable<T> Get(string key)
            => _child.Get(key);
    }
}