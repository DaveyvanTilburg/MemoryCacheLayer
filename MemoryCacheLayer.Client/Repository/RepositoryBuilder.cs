using MemoryCacheLayer.Client.Security;
using MemoryCacheLayer.Domain.Cache;
using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Client.Repository
{
    public class RepositoryBuilder
    {
        private readonly bool _cacheEnabled;

        public RepositoryBuilder(bool cacheEnabled)
        {
            _cacheEnabled = cacheEnabled;
        }

        internal IRepository<T> Build<T>(IRepository<T> child, Role role) where T : IRepositoryItem
        {
            IRepository<T> result = child;

            if (_cacheEnabled)
                result = new CachedRepositoryWrapper<T>(result);

            switch (role)
            {
                case Role.WriteOnly:
                    result = new WriteOnlyRepositoryProxy<T>(result);
                    break;
                case Role.ReadOnly:
                    result = new ReadOnlyRepositoryProxy<T>(result);
                    break;
                case Role.FullAccess:
                case Role.None:
                default:
                    break;
            }

            return result;
        }
    }
}