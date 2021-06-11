using System;
using MemoryCacheLayer.Expressions;

namespace MemoryCacheLayer.Customers.Expressions
{
    public class ByName : IExpression<Customer>
    {
        private readonly string _name;

        public ByName(string location)
            => _name = location;

        public Func<Customer, bool> Lambda()
            => c => c.DisplayName().IndexOf(_name, StringComparison.OrdinalIgnoreCase) > 0;
    }
}