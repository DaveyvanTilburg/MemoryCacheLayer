using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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

            var customerRepository = new CustomerRepository(
                sqlDatabase
            );

            List<Customer> result = customerRepository.By(
                customerRepository.ByLocation("Gouda"),
                customerRepository.ByName("Tilburg"),
                customerRepository.ByType(CustomerType.Gold)
            ).ToList();

            result.Count.Should().Be(2);
            sqlDatabase.GetCount().Should().Be(1);

            List<Customer> normals = customerRepository.By(
                customerRepository.ByName("Test"),
                customerRepository.ByType(CustomerType.Normal)
            ).ToList();

            normals.Count.Should().Be(99996);
            sqlDatabase.GetCount().Should().Be(1);

            Customer davey = customerRepository.By(
                customerRepository.ByName("Davey")
            ).First();

            davey.Should().NotBe(null);
            sqlDatabase.GetCount().Should().Be(1);

            customerRepository.Save(davey);
            sqlDatabase.SaveCount().Should().Be(0);

            davey.CustomerType(CustomerType.Normal);
            customerRepository.Save(davey);
            sqlDatabase.SaveCount().Should().Be(1);
        }

        private IEnumerable<Customer> GenerateRandoms()
        {
            for (int i = 5; i <= 100000; i++)
                yield return new Customer(i, $"Test{i}", $"Test{i}", $"Test{i}", CustomerType.Normal);
        }
    }
}