using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MemoryCacheLayer.Cache;
using MemoryCacheLayer.Customers;
using MemoryCacheLayer.Customers.Expressions;
using MemoryCacheLayer.Sql;
using Xunit;

namespace MemoryCacheLayer
{
    public class MemoryLayerTests
    {
        [Fact]
        public void Test()
        {
            var sqlDatabase = new FakeSqlDatabase<Customer>(
                new List<Customer>(GenerateRandoms())
                {
                    new(1, "Davey", "Tilburg", "Gouda", CustomerType.Gold),
                    new(2, "Joey", "Tilburg", "Gouda", CustomerType.Gold),
                    new(3, "Some", "Body", "Reeuwijk", CustomerType.Gold),
                    new(4, "Some", "Other", "Rotterdam", CustomerType.Gold)
                }
            );

            ICache<Customer> customerCache = new Cache<Customer>(
                sqlDatabase
            );

            List<Customer> result = customerCache.Where(
                new ByLocation("Gouda"),
                new ByName("Tilburg"),
                new ByCustomerType(CustomerType.Gold)
            ).ToList();

            result.Count.Should().Be(2);
            sqlDatabase.GetCount().Should().Be(1);

            List<Customer> normals = customerCache.Where(
                new ByName("Test"),
                new ByCustomerType(CustomerType.Normal)
            ).ToList();

            normals.Count.Should().Be(99996);
            sqlDatabase.GetCount().Should().Be(1);

            Customer unknown = customerCache.First(
                new ByName("YYY")
            );

            unknown.Should().NotBeNull();
            unknown.Location().Should().Be("Unknown");

            Customer davey = customerCache.First(
                new ByName("Davey")
            );

            davey.Should().NotBeNull();
            sqlDatabase.GetCount().Should().Be(1);

            customerCache.Save(davey);
            sqlDatabase.SaveCount().Should().Be(0);

            davey.CustomerType(CustomerType.Normal);
            customerCache.Save(davey);
            sqlDatabase.SaveCount().Should().Be(1);
        }

        private IEnumerable<Customer> GenerateRandoms()
        {
            for (int i = 5; i <= 100000; i++)
                yield return new Customer(i, $"Test{i}", $"Test{i}", $"Test{i}", CustomerType.Normal);
        }
    }
}