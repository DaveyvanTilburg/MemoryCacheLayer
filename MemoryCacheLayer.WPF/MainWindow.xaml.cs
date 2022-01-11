using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using MemoryCacheLayer.Cache;
using MemoryCacheLayer.Customers;
using MemoryCacheLayer.Sql;

namespace MemoryCacheLayer.WPF
{
    public partial class MainWindow : Window
    {
        private readonly FakeSqlDatabase<Customer> _database;
        private readonly ICache<Customer> _cache;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            _database = new FakeSqlDatabase<Customer>(
                ("0", () => MockDatabaseSet(0, 100000)),
                ("1", () => MockDatabaseSet(0, 2000)),
                ("2", () => MockDatabaseSet(0, 800)),
                ("3", () => MockDatabaseSet(0, 50000))

            );
            _cache = new Cache<Customer>(
                _database
            );
        }

        private void BtnGo_OnClick(object sender, RoutedEventArgs e)
        {
            int.TryParse(TxtYear.Text, out int year);

            IEnumerable<Customer> Filter(IEnumerable<Customer> items) => 
                items.Where(i => 
                    (string.IsNullOrWhiteSpace(TxtDisplayName.Text) || i.DisplayName().IndexOf(TxtDisplayName.Text, StringComparison.OrdinalIgnoreCase) >= 0) && 
                    (string.IsNullOrWhiteSpace(TxtLocationName.Text) || i.Location().IndexOf(TxtLocationName.Text, StringComparison.OrdinalIgnoreCase) >= 0) && 
                    (string.IsNullOrWhiteSpace(TxtType.Text) || i.CustomerType().ToString().IndexOf(TxtType.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (year == 0 || i.BirthDate().Year == year)
                ).ToList();

            string key = TxtKey.Text;

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Customer> result = _cache.Where(key, Filter).ToList();
            stopwatch.Stop();

            UpdateStats(stopwatch.Elapsed, result.Count, key);
        }

        private void BtnResetCache_OnClick(object sender, RoutedEventArgs e)
        {
            string key = TxtKey.Text;

            _database.Reset();
            _cache.Clear(key);

            UpdateStats(new TimeSpan(0), 0, key);
        }

        private void UpdateStats(TimeSpan timeElapsed, int resultCount, string key)
        {
            LabelResultCount.Content = $"Result count: {resultCount}";
            LabelTimeElapsed.Content = $"Time elapsed: {timeElapsed:mm':'ss':'ffffff}";
            LabelCacheCount.Content = $"Cached count: {_cache.InCacheCount(key)}";

            BorderMemory.BorderBrush = new SolidColorBrush(Color(1));
            BorderDatabase.BorderBrush = new SolidColorBrush(Color(_database.GetCount() + _database.SaveCount()));
            _database.Reset();
        }

        private Color Color(int count)
            => count == 0 ? Colors.Red : Colors.ForestGreen;

        private static IEnumerable<Customer> MockDatabaseSet(int startIndex, int range)
        {
            for (int i = startIndex; i < range; i++)
                yield return new Customer(i, $"Test{i}", $"Test{i}", $"Test{i}", new DateTime(1900, 1, 1).AddYears(i % 1000), i % 50 == 0 ? CustomerType.Gold : CustomerType.Normal);
        }

        private void BtnGetById_OnClick(object sender, RoutedEventArgs e)
        {
            string key = TxtKey.Text;
            int.TryParse(TxtGetId.Text, out int id);

            Stopwatch stopwatch = Stopwatch.StartNew();
            Customer result = _cache.One(key, items => items.FirstOrDefault(i => i.Id() == id));
            stopwatch.Stop();

            TxtEditDisplayName.Text = result.DisplayName();
            TxtEditLocationName.Text = result.Location();
            TxtEditCustomerType.Text = result.CustomerType().ToString();
            TxtEditYear.Text = result.BirthDate().Year.ToString();

            UpdateStats(stopwatch.Elapsed, result.Id() > 0 ? 1 : 0, key);
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            string key = TxtKey.Text;
            int.TryParse(TxtGetId.Text, out int id);
            int.TryParse(TxtEditYear.Text, out int year);
            Enum.TryParse(typeof(CustomerType), TxtEditCustomerType.Text, true, out object? customerType);
            customerType ??= CustomerType.Normal;

            string[] nameParts = TxtEditDisplayName.Text.Split("-");

            Customer customer = new Customer(
                id,
                nameParts[0],
                nameParts[1],
                TxtEditLocationName.Text,
                new DateTime(year, 1, 1),
                (CustomerType)customerType
            );

            Stopwatch stopwatch = Stopwatch.StartNew();
            _cache.Save(key, customer);
            stopwatch.Stop();

            UpdateStats(stopwatch.Elapsed, 1, key);
        }
    }
}