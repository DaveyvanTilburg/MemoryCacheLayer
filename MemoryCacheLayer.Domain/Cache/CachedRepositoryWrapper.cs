using MemoryCacheLayer.Domain.Repository;
using System.Runtime.Caching;

namespace MemoryCacheLayer.Domain.Cache
{
    public class CachedRepositoryWrapper<T> : IRepository<T> where T : IRepositoryItem
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

            if (cachedItem == null)
                return;

            list.Remove(cachedItem);
            _child.Delete(key, id);
        }

        void IRepository<T>.Upsert(string key, T value)
        {
            List<T> list = List(key);
            T cachedItem = list.FirstOrDefault(i => i.Id() == value.Id());

            if (cachedItem == null)
            {
                list.Add(value);
                _child.Upsert(key, value);
                return;
            }

            if (cachedItem.Equals(value))
                return;

            list.Remove(cachedItem);
            list.Add(value);


            _child.Upsert(key, value);
        }

        IEnumerable<T> IRepository<T>.Get(string key)
            => List(key);

        private List<T> List(string key)
        {
            string cacheKey = CacheKey(key);
            List<T> source = (List<T>)_cache.Get(cacheKey);

            if (source != null)
                return source;

            source = _child.Get(key).ToList();
            _cache.Add(cacheKey, source, DateTimeOffset.MaxValue);

            return source;
        }

        private string CacheKey(string key)
            => $"ListOf({typeof(T).Name})-Key({key})";


        public void Clear(string key)
            => _cache.Remove(CacheKey(key));

        public int InCacheCount(string key)
            => ((List<T>)_cache.Get(CacheKey(key)))?.Count ?? 0;
    }
}