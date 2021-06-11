using System;
using MemoryCacheLayer.Expressions;

namespace MemoryCacheLayer.Customers.Expressions
{
    public class ByLocation : IExpression<Customer>
    {
        private readonly string _location;

        public ByLocation(string location)
            => _location = location;

        public Func<Customer, bool> Lambda()
            => c => c.Location().Equals(_location, StringComparison.OrdinalIgnoreCase);
    }
}