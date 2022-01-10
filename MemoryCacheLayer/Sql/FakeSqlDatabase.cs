using System;
using System.Collections.Generic;

namespace MemoryCacheLayer.Sql
{
    public class FakeSqlDatabase<T> : ISqlDatabase<T> where T : class, IDatabaseItem<T>, new()
    {
        private readonly Func<IEnumerable<T>> _source;

        private int _saveCount;
        private int _getCount;

        public FakeSqlDatabase(Func<IEnumerable<T>> source)
        {
            _source = source;

            _saveCount = 0;
            _getCount = 0;
        }

        void ISqlDatabase<T>.Save(T value)
        {
            _saveCount++;
        }

        IEnumerable<T> ISqlDatabase<T>.Get()
        {
            _getCount++;

            return _source();
        }

        public int SaveCount()
            => _saveCount;

        public int GetCount()
            => _getCount;
    }
}