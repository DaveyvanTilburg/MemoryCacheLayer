using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Domain.Security
{
    internal class ReadOnlyRepositoryProxy<T> : IRepository<T> where T : struct, IRepositoryItem
    {
        private readonly IRepository<T> _child;

        public ReadOnlyRepositoryProxy(IRepository<T> child)
        {
            _child = child;
        }

        void IRepository<T>.Delete(string key, int id)
        {

        }

        void IRepository<T>.Update(string key, T value)
        {
            
        }

        int IRepository<T>.Insert(string key, T value)
            => 0;

        IEnumerable<T> IRepository<T>.Get(string key)
            => _child.Get(key);
    }
}