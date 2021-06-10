using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MemoryCacheLayer.Cache;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer
{
    public class CustomerRepository : Cache<Customer>
    {
        public CustomerRepository(ISqlDatabase<Customer> sqlDatabase) : base(sqlDatabase)
        {
        }

        public IEnumerable<Customer> By(params Expression<Func<Customer, bool>>[] expressions)
            => ((ICache<Customer>)this).Get(expressions);

        public Expression<Func<Customer, bool>> ByLocation(string location)
            => c => c.Location().Equals(location, StringComparison.OrdinalIgnoreCase);

        public Expression<Func<Customer, bool>> ByName(string name)
            => c => c.DisplayName().IndexOf(name, StringComparison.OrdinalIgnoreCase) > 0;

        public Expression<Func<Customer, bool>> ByType(CustomerType customerType)
            => c => c.CustomerType().Equals(customerType);
    }
}