using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.Client.Customers
{
    public record struct Customer(int Id, string FirstName, string LastName, string LocationName, DateTime BirthDate, CustomerType CustomerType) : IRepositoryItem
    {
        int IRepositoryItem.Id()
            => Id;

        IRepositoryItem IRepositoryItem.CloneWithId(int value)
            => this with { Id = value };


        public Customer CloneWithCustomerType(CustomerType customerType)
            => this with { CustomerType = customerType };

        public string DisplayName()
            => $"{FirstName} - {LastName}";
    }
}