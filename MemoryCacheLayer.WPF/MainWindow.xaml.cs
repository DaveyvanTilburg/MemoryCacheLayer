using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MemoryCacheLayer.Client.Customers;
using MemoryCacheLayer.Client.Repository;
using MemoryCacheLayer.Domain.Cache;
using MemoryCacheLayer.Domain.Repository;

namespace MemoryCacheLayer.WPF
{
    public partial class MainWindow : Window
    {
        private readonly FakeRepository<Customer> _repository;
        private readonly IRepository<Customer> _wrappedRepository;
        private readonly CachedRepositoryWrapper<Customer> _cache;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            _repository = new FakeRepository<Customer>(
                ("0", () => MockDatabaseSet(0, 100000)),
                ("1", () => MockDatabaseSet(0, 2000)),
                ("2", () => MockDatabaseSet(0, 800)),
                ("3", () => MockDatabaseSet(0, 50000))

            );

            _cache = new CachedRepositoryWrapper<Customer>(
                _repository
            );
            _wrappedRepository = _cache;
        }

        private static IEnumerable<Customer> MockDatabaseSet(int startIndex, int range)
        {
            for (int i = startIndex; i < range; i++)
                yield return new Customer(i, $"Test{i}", $"Test{i}", $"Test{i}", new DateTime(1900, 1, 1).AddYears(i % 1000), i % 50 == 0 ? CustomerType.Gold : CustomerType.Normal);
        }

        private void BtnGo_OnClick(object sender, RoutedEventArgs e)
        {
            int.TryParse(TxtYear.Text, out int year);
            string key = TxtKey.Text;

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Customer> result = _wrappedRepository.Get(key).Where(i =>
                (string.IsNullOrWhiteSpace(TxtDisplayName.Text) || i.FirstName().IndexOf(TxtDisplayName.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrWhiteSpace(TxtDisplayName.Text) || i.LastName().IndexOf(TxtDisplayName.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrWhiteSpace(TxtLocationName.Text) || i.LocationName().IndexOf(TxtLocationName.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrWhiteSpace(TxtType.Text) || i.CustomerType().ToString().IndexOf(TxtType.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (year == 0 || i.BirthDate().Year == year))
                .ToList();
            stopwatch.Stop();

            UpdateStats(stopwatch.Elapsed, result.Count, key);
        }

        private void BtnResetCache_OnClick(object sender, RoutedEventArgs e)
        {
            string key = TxtKey.Text;

            _repository.Reset();
            _cache.Clear(key);

            UpdateStats(new TimeSpan(0), 0, key);
        }

        private void BtnGetById_OnClick(object sender, RoutedEventArgs e)
        {
            string key = TxtKey.Text;
            int.TryParse(TxtGetId.Text, out int id);

            Stopwatch stopwatch = Stopwatch.StartNew();
            Customer result = _wrappedRepository.Get(key).FirstOrDefault(i => i.Id() == id);
            stopwatch.Stop();

            TxtEditDisplayName.Text = result.DisplayName();
            TxtEditLocationName.Text = result.LocationName();
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
            _wrappedRepository.Upsert(key, customer);
            stopwatch.Stop();

            UpdateStats(stopwatch.Elapsed, 1, key);
        }
        
        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            string key = TxtKey.Text;
            int.TryParse(TxtGetId.Text, out int id);

            Stopwatch stopwatch = Stopwatch.StartNew();
            _wrappedRepository.Delete(key, id);
            stopwatch.Stop();

            UpdateStats(stopwatch.Elapsed, 1, key);
        }

        private void UpdateStats(TimeSpan timeElapsed, int resultCount, string key)
        {
            LabelResultCount.Content = $"Result count: {resultCount}";
            LabelTimeElapsed.Content = $"Time elapsed: {timeElapsed:mm':'ss':'ffffff}";
            LabelCacheCount.Content = $"Cached count: {_cache.InCacheCount(key)}";

            BorderMemory.BorderBrush = new SolidColorBrush(Color(1));
            BorderDatabase.BorderBrush = new SolidColorBrush(Color(_repository.CallCount() + _repository.CallCount()));
            _repository.Reset();

            LabelMemoryUsage.Content = FormatSize(GetUsedPhys());
        }

        private Color Color(int count)
            => count == 0 ? Colors.Red : Colors.ForestGreen;

        public long GetUsedPhys()
        {
            Process proc = Process.GetCurrentProcess();
            return proc.PrivateMemorySize64;
        }

        private string FormatSize(double size)
        {
            double d = size;
            int i = 0;
            while (d > 1024 && i < 5)
            {
                d /= 1024;
                i++;
            }
            string[] unit = { "B", "KB", "MB", "GB", "TB" };
            return $"{Math.Round(d, 2)} {unit[i]}";
        }
    }
}