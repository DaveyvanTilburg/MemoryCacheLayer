using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MemoryCacheLayer.Sql
{
    public class FakeSqlDatabase<TDatabaseItem, TClone> : ISqlDatabase<TDatabaseItem, TClone> where TDatabaseItem : class where TClone : struct, ICloneItem
    {
        private readonly Dictionary<string, Func<IEnumerable<TDatabaseItem>>> _sources;

        private int _saveCount;
        private int _getCount;

        public FakeSqlDatabase(params (string, Func<IEnumerable<TDatabaseItem>>)[] sources)
        {
            _sources = sources.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            _saveCount = 0;
            _getCount = 0;
        }

        void ISqlDatabase<TDatabaseItem, TClone>.Save(string key, TClone value)
        {
            _saveCount++;

            Thread.Sleep(100);
        }

        IEnumerable<TDatabaseItem> ISqlDatabase<TDatabaseItem, TClone>.Get(string key)
        {
            _getCount++;

            Thread.Sleep(500);

            return _sources.ContainsKey(key) ? 
                _sources[key]() : 
                new List<TDatabaseItem>();
        }

        public int SaveCount()
            => _saveCount;

        public int GetCount()
            => _getCount;

        public void Reset()
        {
            _saveCount = 0;
            _getCount = 0;
        }
    }
}