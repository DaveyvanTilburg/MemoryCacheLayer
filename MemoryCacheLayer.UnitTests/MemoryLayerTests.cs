﻿using FluentAssertions;
using MemoryCacheLayer.Client.Customers;
using MemoryCacheLayer.Client.Repository;
using MemoryCacheLayer.Domain.Cache;
using MemoryCacheLayer.Domain.Repository;
using Xunit;

namespace MemoryCacheLayer.UnitTests
{
    public class MemoryLayerTests
    {
        [Fact]
        public void Test()
        {
            string key = "Test";

            var sqlDatabase = new FakeRepository<Customer>(
                (key, () => new List<Customer>(GenerateRandoms())
                    {
                        new(1, "Davey", "Tilburg", "Gouda", new DateTime(1991, 9, 11), CustomerType.Gold),
                        new(2, "Joey", "Tilburg", "Gouda", new DateTime(1987, 9, 7), CustomerType.Gold),
                        new(3, "Some", "Body", "Reeuwijk", new DateTime(1992, 11, 1), CustomerType.Gold),
                        new(4, "Some", "Other", "Rotterdam", new DateTime(1995, 1, 10), CustomerType.Gold)
                    }
                )
            );

            IRepository<Customer> customerCache = new CachedRepositoryWrapper<Customer>(sqlDatabase);

            List<Customer> result = customerCache.Get(key).Where(
                i => i.LocationName().Equals("Gouda") && 
                                    i.LastName().Equals("Tilburg") && 
                                    i.CustomerType() == CustomerType.Gold
            ).ToList();

            result.Count.Should().Be(2);
            sqlDatabase.CallCount().Should().Be(1);

            List<Customer> normals = customerCache.Get(key).Where(i =>
                i.FirstName().Contains("Test") &&
                i.CustomerType() == CustomerType.Normal
            ).ToList();

            normals.Count.Should().Be(99996);
            sqlDatabase.CallCount().Should().Be(1);

            Customer unknown = customerCache.Get(key)
                .FirstOrDefault(i => i.FirstName().Contains("YYY"));

            unknown.Should().NotBeNull();
            unknown.Id().Should().Be(0);

            Customer davey = customerCache.Get(key)
                .FirstOrDefault(i => i.FirstName().Contains("Davey"));

            davey.Should().NotBeNull();
            sqlDatabase.CallCount().Should().Be(1);

            customerCache.Upsert(key, davey);
            sqlDatabase.CallCount().Should().Be(1);

            davey.CustomerType(CustomerType.Normal);
            customerCache.Upsert(key, davey);
            sqlDatabase.CallCount().Should().Be(2);
        }

        private IEnumerable<Customer> GenerateRandoms()
        {
            for (int i = 5; i <= 100000; i++)
                yield return new Customer(i, $"Test{i}", $"Test{i}", $"Test{i}", new DateTime(1900, 1, 1).AddYears(i % 1000), CustomerType.Normal);
        }
    }
}