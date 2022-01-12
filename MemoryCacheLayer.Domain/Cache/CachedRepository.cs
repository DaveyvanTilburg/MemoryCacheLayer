using MemoryCacheLayer.Domain.Repository;
using System.Runtime.Caching;

namespace MemoryCacheLayer.Domain.Cache
{
    public class CachedRepository<TDatabaseItem, TClone> : ICachedRepository<TDatabaseItem, TClone> where TDatabaseItem : class, IRepositoryItem<TClone> where TClone : struct, ICloneItem
    {
        private readonly IRepository<TDatabaseItem> _repository;
        private readonly Func<TClone, TDatabaseItem> _createDatabaseItem;
        private readonly Func<TClone> _createNullCloneObject;

        private readonly MemoryCache _cache;

        public CachedRepository(IRepository<TDatabaseItem> repository, Func<TClone, TDatabaseItem> createDatabaseItem, Func<TClone> createNullCloneObject)
        {
            _repository = repository;
            _createDatabaseItem = createDatabaseItem;
            _createNullCloneObject = createNullCloneObject;

            _cache = MemoryCache.Default;
        }

        void ICachedRepository<TDatabaseItem, TClone>.Delete(string key, int id)
        {
            List<TDatabaseItem> list = List(key);
            TDatabaseItem cachedItem = list.FirstOrDefault(i => i.Id() == id);

            if (cachedItem == null)
                return;

            list.Remove(cachedItem);
            _repository.Delete(key, id);
        }

        void ICachedRepository<TDatabaseItem, TClone>.Upsert(string key, TClone value)
        {
            List<TDatabaseItem> list = List(key);
            TDatabaseItem cachedItem = list.FirstOrDefault(i => i.Id() == value.Id());

            if (cachedItem == null)
            {
                TDatabaseItem databaseItem = _createDatabaseItem(value);
                list.Add(databaseItem);
                _repository.Upsert(key, databaseItem);
                return;
            }

            if (cachedItem.Equals(value))
                return;

            list.Remove(cachedItem);
            list.Add(_createDatabaseItem(value));


            _repository.Upsert(key, _createDatabaseItem(value));
        }

        IEnumerable<TClone> ICachedRepository<TDatabaseItem, TClone>.Where(string key, Func<IEnumerable<TDatabaseItem>, IEnumerable<TDatabaseItem>> predicate)
        {
            List<TDatabaseItem> items = List(key);
            IEnumerable<TDatabaseItem> result = new List<TDatabaseItem>(items);
            IEnumerable<TClone> clones = predicate(result).Select(i => i.Clone());

            return clones;
        }

        TClone ICachedRepository<TDatabaseItem, TClone>.One(string key, Func<IEnumerable<TDatabaseItem>, TDatabaseItem> filter)
        {
            List<TDatabaseItem> items = List(key);
            IEnumerable<TDatabaseItem> sourceItems = new List<TDatabaseItem>(items);
            TDatabaseItem result = filter(sourceItems);

            return result?.Clone() ?? _createNullCloneObject();
        }

        void ICachedRepository<TDatabaseItem, TClone>.Clear(string key)
            => _cache.Remove(CacheKey(key));

        int ICachedRepository<TDatabaseItem, TClone>.InCacheCount(string key)
            => ((List<TDatabaseItem>)_cache.Get(CacheKey(key)))?.Count ?? 0;

        private List<TDatabaseItem> List(string key)
        {
            string cacheKey = CacheKey(key);
            List<TDatabaseItem> source = (List<TDatabaseItem>)_cache.Get(cacheKey);

            if (source != null)
                return source;

            source = _repository.Get(key).ToList();
            _cache.Add(cacheKey, source, DateTimeOffset.MaxValue);

            return source;
        }

        private string CacheKey(string key)
            => $"ListOf({typeof(TDatabaseItem).Name})-Key({key})";
    }
}