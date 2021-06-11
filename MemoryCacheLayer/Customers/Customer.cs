using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Customers
{
    public class Customer : IDatabaseItem<Customer>
    {
        private readonly int _id;
        private readonly string _firstName;
        private readonly string _lastName;
        private readonly string _location;
        private CustomerType _customerType;

        public Customer() : this(0, "Unknown", "Unknown", "Unknown", Customers.CustomerType.Normal)
        {
        }

        public Customer(int id, string firstName, string lastName, string location, CustomerType customerType)
        {
            _id = id;
            _firstName = firstName;
            _lastName = lastName;
            _location = location;
            _customerType = customerType;
        }

        int IDatabaseItem<Customer>.Id()
            => _id;

        public bool Equals(Customer other)
            =>
                other._id.Equals(_id) &&
                other._customerType.Equals(_customerType) &&
                other._firstName.Equals(_firstName) &&
                other._lastName.Equals(_lastName) &&
                other._location.Equals(_location);

        public Customer Clone()
            => (Customer)MemberwiseClone();

        public string DisplayName()
            => $"Name: {_firstName} - {_lastName}";

        public string Location()
            => _location;

        public CustomerType CustomerType()
            => _customerType;

        public void CustomerType(CustomerType customerType)
            => _customerType = customerType;
    }
}