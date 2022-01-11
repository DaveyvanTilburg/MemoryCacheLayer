using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Cache
{
    public class Cache<TDatabaseItem, TClone> : ICache<TDatabaseItem, TClone> where TDatabaseItem : class, IDatabaseItem<TClone> where TClone : struct, ICloneItem
    {
        private readonly ISqlDatabase<TDatabaseItem, TClone> _sqlDatabase;
        private readonly Func<TClone, TDatabaseItem> _createDatabaseItem;

        private readonly MemoryCache _cache;

        public Cache(ISqlDatabase<TDatabaseItem, TClone> sqlDatabase, Func<TClone, TDatabaseItem> createDatabaseItem)
        {
            _sqlDatabase = sqlDatabase;
            _createDatabaseItem = createDatabaseItem;

            _cache = MemoryCache.Default;
        }

        void ICache<TDatabaseItem, TClone>.Save(string key, TClone value)
        {
            List<TDatabaseItem> list = List(key);
            TDatabaseItem cachedItem = list.FirstOrDefault(i => i.Id() == value.Id());

            if (cachedItem == null)
            {
                list.Add(_createDatabaseItem(value));
                _sqlDatabase.Save(key, value);
                return;
            }

            if (value.Equals(cachedItem))
                return;

            list.Remove(cachedItem);
            list.Add(_createDatabaseItem(value));

            _sqlDatabase.Save(key, value);
        }

        IEnumerable<TClone> ICache<TDatabaseItem, TClone>.Where(string key, Func<IEnumerable<TDatabaseItem>, IEnumerable<TDatabaseItem>> filter)
        {
            List<TDatabaseItem> items = List(key);
            IEnumerable<TDatabaseItem> result = new List<TDatabaseItem>(items);
            IEnumerable<TClone> clones = filter(result).Select(i => i.Clone());

            return clones;
        }

        TClone ICache<TDatabaseItem, TClone>.One(string key, Func<IEnumerable<TDatabaseItem>, TDatabaseItem> filter)
        {
            List<TDatabaseItem> items = List(key);
            IEnumerable<TDatabaseItem> sourceItems = new List<TDatabaseItem>(items);
            TDatabaseItem result = filter(sourceItems);

            return result?.Clone() ?? new TClone();
        }

        void ICache<TDatabaseItem, TClone>.Clear(string key)
            => _cache.Remove(CacheKey(key));

        int ICache<TDatabaseItem, TClone>.InCacheCount(string key)
            => ((List<TDatabaseItem>)_cache.Get(CacheKey(key)))?.Count ?? 0;

        private List<TDatabaseItem> List(string key)
        {
            string cacheKey = CacheKey(key);
            List<TDatabaseItem> source = (List<TDatabaseItem>)_cache.Get(cacheKey);

            if (source != null)
                return source;

            source = _sqlDatabase.Get(key).ToList();
            _cache.Add(cacheKey, source, DateTimeOffset.MaxValue);

            return source;
        }

        private string CacheKey(string key)
            => $"ListOf({typeof(TDatabaseItem).Name})-Key({key})";
    }
}