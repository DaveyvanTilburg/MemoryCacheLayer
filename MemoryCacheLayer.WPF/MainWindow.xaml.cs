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
            InitializeComponent();

            _database = new FakeSqlDatabase<Customer>(
                MockDatabaseGet
            );
            _cache = new Cache<Customer>(
                _database
            );
        }

        private void BtnGo_OnClick(object sender, RoutedEventArgs e)
        {
            IEnumerable<Customer> Filter(IEnumerable<Customer> items) => 
                items.Where(i => 
                    (string.IsNullOrWhiteSpace(TxtDisplayName.Text) || i.DisplayName().IndexOf(TxtDisplayName.Text, StringComparison.OrdinalIgnoreCase) >= 0) && 
                    (string.IsNullOrWhiteSpace(TxtLocationName.Text) || i.Location().IndexOf(TxtLocationName.Text, StringComparison.OrdinalIgnoreCase) >= 0) && 
                    (string.IsNullOrWhiteSpace(TxtType.Text) || i.CustomerType().ToString().IndexOf(TxtType.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Customer> result = _cache.Where(Filter).ToList();
            stopwatch.Stop();

            UpdateStats(stopwatch.Elapsed, result.Count);
        }

        private void BtnResetCache_OnClick(object sender, RoutedEventArgs e)
        {
            _database.Reset();
            _cache.Clear();

            UpdateStats(new TimeSpan(0), 0);
        }

        private void UpdateStats(TimeSpan timeElapsed, int resultCount)
        {
            LabelResultCount.Content = $"Result count: {resultCount}";
            LabelTimeElapsed.Content = $"Time elapsed: {timeElapsed:mm':'ss':'ffffff}";
            LabelCacheCount.Content = $"Cached count: {_cache.InCacheCount()}";

            BorderMemory.BorderBrush = new SolidColorBrush(Color(1));
            BorderDatabase.BorderBrush = new SolidColorBrush(Color(_database.GetCount()));
            _database.Reset();
        }

        private Color Color(int count)
            => count == 0 ? Colors.Red : Colors.ForestGreen;

        private static IEnumerable<Customer> MockDatabaseGet()
        {
            Thread.Sleep(500);

            yield return new Customer(1, "Davey", "Tilburg", "Gouda", CustomerType.Gold);
            yield return new Customer(2, "Joey", "Tilburg", "Gouda", CustomerType.Gold);
            yield return new Customer(3, "Some", "Body", "Reeuwijk", CustomerType.Gold);
            yield return new Customer(4, "Some", "Other", "Rotterdam", CustomerType.Gold);

            for (int i = 5; i <= 100000; i++)
                yield return new Customer(i, $"Test{i}", $"Test{i}", $"Test{i}", CustomerType.Normal);
        }
    }
}