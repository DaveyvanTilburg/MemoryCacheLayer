using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Client.Repository
{
    public class FakeRepository<TDatabaseItem> : IRepository<TDatabaseItem> where TDatabaseItem : class
    {
        private readonly Dictionary<string, Func<IEnumerable<TDatabaseItem>>> _sources;

        private int _callCount;

        public FakeRepository(params (string, Func<IEnumerable<TDatabaseItem>>)[] sources)
        {
            _sources = sources.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            _callCount = 0;
        }

        void IRepository<TDatabaseItem>.Delete(string key, int id)
            => _callCount++;

        void IRepository<TDatabaseItem>.Upsert(string key, TDatabaseItem value)
        {
            _callCount++;

            Thread.Sleep(100);
        }

        IEnumerable<TDatabaseItem> IRepository<TDatabaseItem>.Get(string key)
        {
            _callCount++;

            Thread.Sleep(500);

            return _sources.ContainsKey(key) ? 
                _sources[key]() : 
                new List<TDatabaseItem>();
        }

        public int CallCount()
            => _callCount;

        public void Reset()
            => _callCount = 0;
    }
}