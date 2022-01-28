using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MemoryCacheLayer.Client.Customers;
using MemoryCacheLayer.Client.Repository;
using MemoryCacheLayer.Domain.Cache;
using MemoryCacheLayer.Domain.Repository;
using MemoryCacheLayer.Domain.Security;

namespace MemoryCacheLayer.WPF
{
    public partial class MainWindow : Window
    {
        private FakeRepository<Customer> _fakeRepository;
        private IRepository<Customer> _wrappedRepository;

        private Role _role;
        private bool _cacheEnabled;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            _role = Role.FullAccess;
            _cacheEnabled = true;

            UpdateRepository();
            UpdateButtonColors();
        }

        private static IEnumerable<Customer> MockDatabaseSet(int startIndex, int range)
        {
            for (int i = startIndex; i < range; i++)
                yield return new Customer(i+1, $"Test{i}", $"Test{i}", $"Test{i}", new DateTime(1900, 1, 1).AddYears(i % 1000), i % 50 == 0 ? CustomerType.Gold : CustomerType.Normal);
        }

        private void BtnGo_OnClick(object sender, RoutedEventArgs e)
        {
            int.TryParse(TxtYear.Text, out int year);
            string key = TxtKey.Text;

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Customer> result = _wrappedRepository.Get(key).Where(i =>
                (string.IsNullOrWhiteSpace(TxtDisplayName.Text) || i.FirstName.IndexOf(TxtDisplayName.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrWhiteSpace(TxtDisplayName.Text) || i.LastName.IndexOf(TxtDisplayName.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrWhiteSpace(TxtLocationName.Text) || i.LocationName.IndexOf(TxtLocationName.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (string.IsNullOrWhiteSpace(TxtType.Text) || i.CustomerType.ToString().IndexOf(TxtType.Text, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (year == 0 || i.BirthDate.Year == year))
                .ToList();
            stopwatch.Stop();

            UpdateStats(stopwatch.Elapsed, result.Count, key);
        }

        private void BtnGetById_OnClick(object sender, RoutedEventArgs e)
        {
            string key = TxtKey.Text;
            int.TryParse(TxtGetId.Text, out int id);

            Stopwatch stopwatch = Stopwatch.StartNew();
            Customer result = _wrappedRepository.Get(key).FirstOrDefault(i => i.Id == id);
            stopwatch.Stop();

            LoadCustomer(result);

            UpdateStats(stopwatch.Elapsed, result.Id > 0 ? 1 : 0, key);
        }

        private void LoadCustomer(Customer customer)
        {
            TxtGetId.Text = customer.Id.ToString();
            TxtEditFirstName.Text = customer.FirstName;
            TxtEditLastName.Text = customer.LastName;
            TxtEditLocationName.Text = customer.LocationName;
            TxtEditCustomerType.Text = customer.CustomerType.ToString();
            TxtEditYear.Text = customer.BirthDate.Year.ToString();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            string key = TxtKey.Text;
            int.TryParse(TxtGetId.Text, out int id);
            int.TryParse(TxtEditYear.Text, out int year);
            Enum.TryParse(typeof(CustomerType), TxtEditCustomerType.Text, true, out object? customerType);
            customerType ??= CustomerType.Normal;

            Customer customer = new Customer(
                id,
                TxtEditFirstName.Text,
                TxtEditLastName.Text,
                TxtEditLocationName.Text,
                new DateTime(year, 1, 1),
                (CustomerType)customerType
            );

            Stopwatch stopwatch = Stopwatch.StartNew();

            if (customer.Id == 0)
            {
                int newId = _wrappedRepository.Insert(key, customer);
                LoadCustomer(_wrappedRepository.Get(key).FirstOrDefault(c => c.Id == newId));
            }
            else
                _wrappedRepository.Update(key, customer);

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

            BorderMemory.BorderBrush = new SolidColorBrush(Color(1));
            BorderDatabase.BorderBrush = new SolidColorBrush(Color(_fakeRepository.CallCount() + _fakeRepository.CallCount()));
            _fakeRepository.ResetCount();

            LabelMemoryUsage.Content = FormatSize(GetUsedPhys());
        }

        private void UpdateRepository()
        {
            (_fakeRepository, _wrappedRepository) = FakeRepository<Customer>.CreateFake(
                new RepositoryBuilder(_cacheEnabled),
                _role,
                ("0", MockDatabaseSet(0, 100000).ToList()),
                ("1", MockDatabaseSet(0, 2000).ToList()),
                ("2", MockDatabaseSet(0, 800).ToList()),
                ("3", MockDatabaseSet(0, 50000).ToList()
                ));

            CachedRepositoryWrapper<Customer>.Clear("0");
            CachedRepositoryWrapper<Customer>.Clear("1");
            CachedRepositoryWrapper<Customer>.Clear("2");
            CachedRepositoryWrapper<Customer>.Clear("3");
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

        private void Role_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (button.Name == "ButtonFullAccess")
                _role = Role.FullAccess;

            if (button.Name == "ButtonReadOnly")
                _role = Role.ReadOnly;

            if (button.Name == "ButtonWriteOnly")
                _role = Role.WriteOnly;

            if (button.Name == "ButtonEnableCache")
                _cacheEnabled = true;

            if (button.Name == "ButtonDisableCache")
                _cacheEnabled = false;

            UpdateRepository();
            UpdateButtonColors();
        }

        private void UpdateButtonColors()
        {
            ButtonFullAccess.BorderBrush = new SolidColorBrush(_role == Role.FullAccess ? Colors.Red : Colors.Gray);
            ButtonReadOnly.BorderBrush = new SolidColorBrush(_role == Role.ReadOnly ? Colors.Red : Colors.Gray);
            ButtonWriteOnly.BorderBrush = new SolidColorBrush(_role == Role.WriteOnly ? Colors.Red : Colors.Gray);
            ButtonEnableCache.BorderBrush = new SolidColorBrush(_cacheEnabled ? Colors.Red : Colors.Gray);
            ButtonDisableCache.BorderBrush = new SolidColorBrush(!_cacheEnabled ? Colors.Red : Colors.Gray);
        }
    }
}