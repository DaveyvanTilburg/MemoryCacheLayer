using System.Collections.Generic;
using System.Linq;

namespace MemoryCacheLayer.Sql
{
    public class FakeSqlDatabase<T> : ISqlDatabase<T> where T : IDatabaseItem<T>
    {
        private readonly List<T> _items;

        private int _saveCount;
        private int _getCount;

        public FakeSqlDatabase(List<T> items)
        {
            _items = items;

            _saveCount = 0;
            _getCount = 0;
        }

        void ISqlDatabase<T>.Save(T value)
        {
            _saveCount++;

            T item = _items.FirstOrDefault(i => i.Id().Equals(value.Id()));

            if (item != null)
                _items.Remove(item);

            _items.Add(item);
        }

        IEnumerable<T> ISqlDatabase<T>.Get()
        {
            _getCount++;

            return _items;
        }

        public int SaveCount()
            => _saveCount;

        public int GetCount()
            => _getCount;
    }
}