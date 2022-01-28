using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Domain.Security
{
    internal class WriteOnlyRepositoryProxy<T> : IRepository<T> where T : struct, IRepositoryItem
    {
        private readonly IRepository<T> _child;

        public WriteOnlyRepositoryProxy(IRepository<T> child)
        {
            _child = child;
        }

        void IRepository<T>.Delete(string key, int id)
            => _child.Delete(key, id);

        void IRepository<T>.Update(string key, T value)
            => _child.Update(key, value);

        int IRepository<T>.Insert(string key, T value)
            => _child.Insert(key, value);

        IEnumerable<T> IRepository<T>.Get(string key)
            => new List<T>();
    }
}