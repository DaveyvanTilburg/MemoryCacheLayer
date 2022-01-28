using MemoryCacheLayer.Domain.Repository;
using System.Runtime.Caching;

namespace MemoryCacheLayer.Domain.Cache
{
    public class CachedRepositoryWrapper<T> : IRepository<T> where T : struct, IRepositoryItem
    {
        private readonly IRepository<T> _child;

        private readonly MemoryCache _cache;

        public CachedRepositoryWrapper(IRepository<T> child)
        {
            _child = child;

            _cache = MemoryCache.Default;
        }

        void IRepository<T>.Delete(string key, int id)
        {
            List<T> list = List(key);
            T cachedItem = list.FirstOrDefault(i => i.Id() == id);

            if (cachedItem.Id() == 0)
                return;

            list.Remove(cachedItem);
            _child.Delete(key, id);
        }

        int IRepository<T>.Insert(string key, T value)
        {
            if (value.Id() != 0)
                return value.Id();

            int newId = _child.Insert(key, value);
            T updatedValue = (T)value.CloneWithId(newId);
            List<T> list = List(key);
            list.Add(updatedValue);

            return newId;
        }

        void IRepository<T>.Update(string key, T value)
        {
            List<T> list = List(key);
            T cachedItem = list.FirstOrDefault(i => i.Id() == value.Id());

            if (cachedItem.Id() == 0)
            {
                list.Add(value);
                _child.Update(key, value);
                return;
            }

            if (cachedItem.Equals(value))
                return;

            list.Remove(cachedItem);
            list.Add(value);

            _child.Update(key, value);
        }

        IEnumerable<T> IRepository<T>.Get(string key)
            => List(key);

        private List<T> List(string key)
        {
            string cacheKey = CacheKey(key);
            object? source = _cache.Get(cacheKey);

            if (source != null)
            {
                List<T> list = source as List<T> ?? new List<T>();
                return list;
            }

            List<T> newList = _child.Get(key).ToList();
            _cache.Add(cacheKey, newList, DateTimeOffset.MaxValue);

            return newList;
        }

        private static string CacheKey(string key)
            => $"ListOf({typeof(T).Name})-Key({key})";

        public static void Clear(string key)
            => MemoryCache.Default.Remove(CacheKey(key));
    }
}