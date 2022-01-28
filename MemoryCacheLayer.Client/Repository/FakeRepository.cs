using MemoryCacheLayer.Domain.Repository;
using MemoryCacheLayer.Domain.Security;

namespace MemoryCacheLayer.Client.Repository
{
    public class FakeRepository<T> : IRepository<T> where T : struct, IRepositoryItem
    {
        private readonly Dictionary<string, List<T>> _sources;

        private int _callCount;

        private FakeRepository(params (string, List<T>)[] sources)
        {
            _sources = sources.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            _callCount = 0;
        }

        void IRepository<T>.Delete(string key, int id)
            => _callCount++;

        int IRepository<T>.Insert(string key, T value)
        {
            _callCount++;
            Thread.Sleep(100);

            if (!_sources.ContainsKey(key))
                return 0;

            _sources[key].Add(value);
            return _sources[key].Count;
        }

        void IRepository<T>.Update(string key, T value)
        {
            _callCount++;
            Thread.Sleep(100);

            _sources[key][value.Id() - 1] = value;
        }

        IEnumerable<T> IRepository<T>.Get(string key)
        {
            _callCount++;

            Thread.Sleep(500);

            return _sources.ContainsKey(key) ? 
                _sources[key] : 
                new List<T>();
        }

        public int CallCount()
            => _callCount;

        public void ResetCount()
            => _callCount = 0;

        public static (FakeRepository<T>, IRepository<T>) CreateFake(RepositoryBuilder builder, Role role, params (string, List<T>)[] sources)
        {
            FakeRepository<T> unwrapped = new FakeRepository<T>(sources);
            IRepository<T> result = builder.Build(unwrapped, role);

            return (unwrapped, result);
        }

        public static IRepository<T> Create(RepositoryBuilder builder, Role role, params (string, List<T>)[] sources)
        {
            IRepository<T> result = builder.Build(new FakeRepository<T>(sources), role);

            return result;
        }
    }
}