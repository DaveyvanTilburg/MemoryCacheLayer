using FluentAssertions;
using MemoryCacheLayer.Client.Customers;
using MemoryCacheLayer.Client.Repository;
using MemoryCacheLayer.Domain.Cache;
using MemoryCacheLayer.Domain.Repository;
using MemoryCacheLayer.Domain.Security;
using Xunit;

namespace MemoryCacheLayer.UnitTests
{
    public class MemoryLayerTests
    {
        private (FakeRepository<Customer>, IRepository<Customer>) Repository(string key)
        {
            CachedRepositoryWrapper<Customer>.Clear(key);

            return FakeRepository<Customer>.CreateFake(
                    new RepositoryBuilder(true),
                    Role.FullAccess,
                    (key, new List<Customer>(GenerateRandoms(100000))
                    {
                        new(1, "Davey", "Tilburg", "Gouda", new DateTime(1991, 9, 11), CustomerType.Gold),
                        new(2, "Joey", "Tilburg", "Gouda", new DateTime(1987, 9, 7), CustomerType.Gold),
                        new(3, "Some", "Body", "Reeuwijk", new DateTime(1992, 11, 1), CustomerType.Gold),
                        new(4, "Some", "Other", "Rotterdam", new DateTime(1995, 1, 10), CustomerType.Gold)
                    }
                ));
        }

        [Fact]
        public void GetTest()
        {
            string key = "Test";
            (FakeRepository<Customer> unwrapped, IRepository<Customer> wrapped) = Repository(key);

            List<Customer> result = wrapped.Get(key).Where(
                i => i.LocationName.Equals("Gouda") && 
                                    i.LastName.Equals("Tilburg") && 
                                    i.CustomerType == CustomerType.Gold
            ).ToList();

            result.Count.Should().Be(2);
            unwrapped.CallCount().Should().Be(1);

            List<Customer> normals = wrapped.Get(key).Where(i =>
                i.FirstName.Contains("Test") &&
                i.CustomerType == CustomerType.Normal
            ).ToList();

            normals.Count.Should().Be(99996);
            unwrapped.CallCount().Should().Be(1);

            Customer unknown = wrapped.Get(key)
                .FirstOrDefault(i => i.FirstName.Contains("YYY"));

            unknown.Should().NotBeNull();
            unknown.Id.Should().Be(0);
        }

        [Fact]
        public void UpdateTest()
        {
            string key = "Test";
            (FakeRepository<Customer> unwrapped, IRepository<Customer> wrapped) = Repository(key);

            Customer davey = wrapped.Get(key)
                .FirstOrDefault(i => i.FirstName.Contains("Davey"));

            davey.Should().NotBeNull();
            unwrapped.CallCount().Should().Be(1);

            wrapped.Update(key, davey);
            unwrapped.CallCount().Should().Be(1);

            Customer updatedDavey = davey.CloneWithCustomerType(CustomerType.Normal);

            Customer davey2 = wrapped.Get(key)
                .FirstOrDefault(i => i.FirstName.Contains("Davey"));

            davey2.CustomerType.Should().Be(CustomerType.Gold);

            wrapped.Update(key, updatedDavey);
            unwrapped.CallCount().Should().Be(2);

            Customer davey3 = wrapped.Get(key)
                .FirstOrDefault(i => i.FirstName.Contains("Davey"));
            davey3.CustomerType.Should().Be(CustomerType.Normal);

            davey2.CustomerType.Should().Be(CustomerType.Gold);

            davey3.Equals(updatedDavey).Should().Be(true);
            davey3.Equals(davey2).Should().Be(false);
        }

        [Fact]
        public void InsertTest()
        {
            string key = "Test";
            (FakeRepository<Customer> unwrapped, IRepository<Customer> wrapped) = Repository(key);

            var newCustomer = new Customer(0, "new", "new", "new", DateTime.MinValue, CustomerType.Normal);
            var newId = wrapped.Insert(key, newCustomer);
            var newEntry = wrapped.Get(key).FirstOrDefault(e => e.Id == newId);
            newEntry.Id.Should().Be(newId);

            unwrapped.CallCount().Should().Be(2);
        }

        private IEnumerable<Customer> GenerateRandoms(int count)
        {
            for (int i = 5; i <= count; i++)
                yield return new Customer(i, $"Test{i}", $"Test{i}", $"Test{i}", new DateTime(1900, 1, 1).AddYears(i % 1000), CustomerType.Normal);
        }
    }
}