using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Client.Repository
{
    public class FakeRepository<T> : IRepository<T> where T : IRepositoryItem
    {
        private readonly Dictionary<string, Func<IEnumerable<T>>> _sources;

        private int _callCount;

        public FakeRepository(params (string, Func<IEnumerable<T>>)[] sources)
        {
            _sources = sources.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            _callCount = 0;
        }

        void IRepository<T>.Delete(string key, int id)
            => _callCount++;

        void IRepository<T>.Upsert(string key, T value)
        {
            _callCount++;

            Thread.Sleep(100);
        }

        IEnumerable<T> IRepository<T>.Get(string key)
        {
            _callCount++;

            Thread.Sleep(500);

            return _sources.ContainsKey(key) ? 
                _sources[key]() : 
                new List<T>();
        }

        public int CallCount()
            => _callCount;

        public void Reset()
            => _callCount = 0;
    }
}