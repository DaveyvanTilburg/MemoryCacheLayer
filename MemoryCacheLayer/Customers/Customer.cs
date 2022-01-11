using System;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.Customers
{
    public struct Customer : ICloneItem
    {
        private readonly int _id;
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

        string ICloneItem.Hash()
            => $"{_id}_{_firstName}_{_lastName}_{_locationName}_{_birthDate}_{_customerType}";

        public string DisplayName()
            => $"Name: {_firstName} - {_lastName}";

        public string LocationName()
            => _locationName;

        public DateTime BirthDate()
            => _birthDate;

        public CustomerType CustomerType()
            => _customerType;

        public void CustomerType(CustomerType customerType)
            => _customerType = customerType;

        public CustomerData CreateData()
            => new(
                _id,
                _firstName,
                _lastName,
                _locationName,
                _birthDate,
                _customerType);
    }
}