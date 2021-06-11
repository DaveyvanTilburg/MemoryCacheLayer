using System;
using MemoryCacheLayer.Expressions;

namespace MemoryCacheLayer.Customers.Expressions
{
    public class ByCustomerType : IExpression<Customer>
    {
        private readonly CustomerType _customerType;

        public ByCustomerType(CustomerType customerType)
            => _customerType = customerType;

        public Func<Customer, bool> Lambda()
            => c => c.CustomerType().Equals(_customerType);
    }
}