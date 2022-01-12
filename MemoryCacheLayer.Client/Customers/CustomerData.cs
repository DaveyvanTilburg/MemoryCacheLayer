using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Client.Customers
{
    public class CustomerData : IRepositoryItem<Customer>
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LocationName { get; set; }
        public DateTime BirthDate { get; set; }
        public CustomerType CustomerType { get; set; }

        public CustomerData() : this(0, "Unknown", "Unknown", "Unknown", new DateTime(1900, 1, 1), Customers.CustomerType.Normal)
        {
        }

        public CustomerData(int id, string firstName, string lastName, string location, DateTime birthDate, CustomerType customerType)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            LocationName = location;
            BirthDate = birthDate;
            CustomerType = customerType;
        }

        int IRepositoryItem<Customer>.Id()
            => Id;

        bool IRepositoryItem<Customer>.Equals(ICloneItem other)
            => other.Hash().Equals(Hash());

        private string Hash()
            => $"{Id}_{FirstName}_{LastName}_{LocationName}_{BirthDate}_{CustomerType}";

        Customer IRepositoryItem<Customer>.Clone() 
            => new(Id, FirstName, LastName, LocationName, BirthDate, CustomerType);
    }
}