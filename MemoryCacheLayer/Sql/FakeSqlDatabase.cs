using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MemoryCacheLayer.Sql
{
    public class FakeSqlDatabase<T> : ISqlDatabase<T> where T : class, IDatabaseItem<T>, new()
    {
        private readonly Dictionary<string, Func<IEnumerable<T>>> _sources;

        private int _saveCount;
        private int _getCount;

        public FakeSqlDatabase(params (string, Func<IEnumerable<T>>)[] sources)
        {
            _sources = sources.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            _saveCount = 0;
            _getCount = 0;
        }

        void ISqlDatabase<T>.Save(string key, T value)
        {
            _saveCount++;

            Thread.Sleep(100);
        }

        IEnumerable<T> ISqlDatabase<T>.Get(string key)
        {
            _getCount++;

            Thread.Sleep(500);

            return _sources.ContainsKey(key) ? 
                _sources[key]() : 
                new List<T>();
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