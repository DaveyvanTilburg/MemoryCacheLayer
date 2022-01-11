using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MemoryCacheLayer.Cache;
using MemoryCacheLayer.Customers;
using MemoryCacheLayer.Sql;
using Xunit;

namespace MemoryCacheLayer
{
    public class MemoryLayerTests
    {
        [Fact]
        public void Test()
        {
            string key = "Test";

            var sqlDatabase = new FakeSqlDatabase<CustomerData, Customer>(
                (key, () => new List<CustomerData>(GenerateRandoms())
                    {
                        new(1, "Davey", "Tilburg", "Gouda", new DateTime(1991, 9, 11), CustomerType.Gold),
                        new(2, "Joey", "Tilburg", "Gouda", new DateTime(1987, 9, 7), CustomerType.Gold),
                        new(3, "Some", "Body", "Reeuwijk", new DateTime(1992, 11, 1), CustomerType.Gold),
                        new(4, "Some", "Other", "Rotterdam", new DateTime(1995, 1, 10), CustomerType.Gold)
                    }
                )
            );

            ICache<CustomerData, Customer> customerCache = new Cache<CustomerData, Customer>(
                sqlDatabase,
                clone => clone.CreateData()
            );

            List<Customer> result = customerCache.Where(
                key,
                items => items
                    .Where(i => i.LocationName.Equals("Gouda") && 
                                i.LastName.Equals("Tilburg") && 
                                i.CustomerType == CustomerType.Gold)
            ).ToList();

            result.Count.Should().Be(2);
            sqlDatabase.GetCount().Should().Be(1);

            List<Customer> normals = customerCache.Where(
                key,
            items => items.Where(i =>
                    i.FirstName.Contains("Test") &&
                    i.CustomerType == CustomerType.Normal
                )
            ).ToList();

            normals.Count.Should().Be(99996);
            sqlDatabase.GetCount().Should().Be(1);

            Customer unknown = customerCache.One(
                key,
                items => items.FirstOrDefault(i => i.FirstName.Contains("YYY")
                )
            );

            unknown.Should().NotBeNull();
            unknown.LocationName().Should().Be("Unknown");

            Customer davey = customerCache.One(
                key,
                items => items.First(i => i.FirstName.Contains("Davey"))
            );

            davey.Should().NotBeNull();
            sqlDatabase.GetCount().Should().Be(1);

            customerCache.Save(key, davey);
            sqlDatabase.SaveCount().Should().Be(0);

            davey.CustomerType(CustomerType.Normal);
            customerCache.Save(key, davey);
            sqlDatabase.SaveCount().Should().Be(1);
        }

        private IEnumerable<CustomerData> GenerateRandoms()
        {
            for (int i = 5; i <= 100000; i++)
                yield return new CustomerData(i, $"Test{i}", $"Test{i}", $"Test{i}", new DateTime(1900, 1, 1).AddYears(i % 1000), CustomerType.Normal);
        }
    }
}