using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Client.Customers
{
    public struct Customer : IRepositoryItem
    {
        private int _id;
        private readonly string _firstName;
        private readonly string _lastName;
        private readonly string _locationName;
        private readonly DateTime _birthDate;
        private CustomerType _customerType;

        public Customer(int id, string firstName, string lastName, string locationName, DateTime birthDate, CustomerType customerType)
        {
            _id = id;
            _firstName = firstName;
            _lastName = lastName;
            _locationName = locationName;
            _birthDate = birthDate;
            _customerType = customerType;
        }

        public int Id()
            => _id;

        void IRepositoryItem.Id(int value)
            => _id = value;

        public string FirstName()
            => _firstName;

        public string LastName()
            => _lastName;

        public string LocationName()
            => _locationName;

        public DateTime BirthDate()
            => _birthDate;

        public CustomerType CustomerType()
            => _customerType;


        public void CustomerType(CustomerType customerType)
            => _customerType = customerType;
        public string DisplayName()
            => $"{_firstName} - {_lastName}";
    }
}