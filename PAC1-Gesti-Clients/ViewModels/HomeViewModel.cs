
/* ════════════════════════════════════════════
 * HOMEVIEWMODEL
 * ════════════════════════════════════════════ */

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using WPF_MVVM_SPA_Template.Models;
using WPF_MVVM_SPA_Template.Services;

namespace WPF_MVVM_SPA_Template.ViewModels
{
    class HomeViewModel : INotifyPropertyChanged
    {
        /* ════════════════════════════════════════════════════
         * Reference to the main ViewModel and ClientService
         * ════════════════════════════════════════════════════ */

        private readonly MainViewModel _mainViewModel;
        private readonly ClientService _clientService;

        /* ════════════════════════════════════════════════
         * Total income across all orders ever recorded.
         * ════════════════════════════════════════════════ */

        private string _totalIncome = string.Empty;
        public string TotalIncome
        {
            get => _totalIncome;
            set { _totalIncome = value; OnPropertyChanged(nameof(TotalIncome)); }
        }

        /* ═════════════════════════════════════════
         * Total income across the current month.
         * ═════════════════════════════════════════ */

        private string _incomeMonth = string.Empty;
        public string IncomeMonth
        {
            get => _incomeMonth;
            set { _incomeMonth = value; OnPropertyChanged(nameof(IncomeMonth)); }
        }

        /* ═══════════════════════════════════════════════════════════════════
         * Name of the month (within the last 12) with the highest income.
         * ═══════════════════════════════════════════════════════════════════ */

        private string _bestSellingMonth = string.Empty;
        public string BestSellingMonth
        {
            get => _bestSellingMonth;
            set { _bestSellingMonth = value; OnPropertyChanged(nameof(BestSellingMonth)); }
        }

        /* ════════════════════════════════════════════════
         * Total income value for the best-selling month.
         * ════════════════════════════════════════════════ */

        private double _incomeOfbestSellingMonth;
        public double IncomeOfbestSellingMonth
        {
            get => _incomeOfbestSellingMonth;
            set { _incomeOfbestSellingMonth = value; OnPropertyChanged(nameof(IncomeOfbestSellingMonth)); }
        }

        /* ════════════════════════════════════════════════
         * Total number of clients in the data source.
         * ════════════════════════════════════════════════ */

        private int _totalClients;
        public int TotalClients
        {
            get => _totalClients;
            set { _totalClients = value; OnPropertyChanged(nameof(TotalClients)); }
        }

        /* ════════════════════════════════════════════════════════════════════════════
         * Observable collection of clients, notifies the view when the list changes
         * ════════════════════════════════════════════════════════════════════════════ */

        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();

        /* ═══════════════════════════════════════════════════════
         * OxyPlot model bound to the chart control in the View.
         * ═══════════════════════════════════════════════════════ */

        private Client? _selectedClient;
        public Client? SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(); }
        }

        /* ═══════════════════════════════════════
        * Currently selected client in the view
        * ════════════════════════════════════════ */

        private PlotModel _plotModel = new PlotModel();
        public PlotModel PlotModel
        {
            get => _plotModel;
            set { _plotModel = value; OnPropertyChanged(nameof(PlotModel)); }
        }

        /* ════════════════════════════════════════════════
         * Stores the raw JSON data loaded from the file
         * ════════════════════════════════════════════════ */

        private dynamic? myArray;

        /* ═════════════════════════
         * Load all the resources
         * ═════════════════════════ */
        public void loadResources()
        {
            Clients.Clear();
            myArray = _clientService.LoadClientsData();

            // Explicit cast to IEnumerable<dynamic> to avoid LINQ binding errors with dynamic types.
            IEnumerable<dynamic> clients = myArray.clients;
            IEnumerable<dynamic> clientsOrders = myArray.clients_orders;

            // Sum all order prices per client ID into a dictionary.
            var orderTotalsByClient = clientsOrders
                .Where(ord => ord["idCli"] != null && ord["price"] != null)
                .GroupBy(ord => (int)ord["idCli"])
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum((Func<dynamic, double>)(ord => (double)ord["price"]))
                );

            // Pick the top 5 clients by their total spend; default to 0 if they have no orders.
            var top5 = clients
                .Where(cli => cli["IdCli"] != null)
                .Select(cli =>
                {
                    int id = (int)cli["IdCli"];
                    double total = orderTotalsByClient.TryGetValue(id, out var t) ? t : 0;
                    return (total, cli);
                })
                .OrderByDescending(x => x.total)
                .Take(5);

            // Populate the ObservableCollection so the View updates automatically.
            foreach (var entry in top5)
            {
                Clients.Add(new Client
                {
                    IdCli = (int)entry.cli["IdCli"],
                    Name = (string)entry.cli["Name"],
                    Lastname = (string)entry.cli["Lastname"]
                });
            }

            TotalClients = myArray?.clients?.Count ?? 0;

            TotalIncome = GetTotalIncome(myArray) + "€";
            IncomeMonth = GetIncomeMonth(myArray) + "€";

            string bestMonth;
            double incomeBest;
            GetBestSellingMonth(myArray, out bestMonth, out incomeBest);
            BestSellingMonth = bestMonth;
            IncomeOfbestSellingMonth = incomeBest;

            // Rebuild the chart with the freshly loaded data.
            PlotModel = CreatePlotModel(myArray);
        }

        /* ═════════════════════════════════════════════════════════════════
         * Sums the price of every order across all clients and all time.
         * ═════════════════════════════════════════════════════════════════ */
        public decimal GetTotalIncome(dynamic data)
        {
            decimal total = 0;
            foreach (var order in data.clients_orders)
                total += (decimal)order.price;
            return total;
        }

        /* ═════════════════════════════════════════════════════════════════
         * Sums order prices for the current calendar month and year only.
         * ═════════════════════════════════════════════════════════════════ */
        public decimal GetIncomeMonth(dynamic data)
        {
            decimal income = 0;
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;
            foreach (var order in data.clients_orders)
            {
                DateTime d = DateTime.Parse((string)order.date);
                if (d.Month == month && d.Year == year)
                    income += (decimal)order.price;
            }
            return income;
        }

        /* ═══════════════════════════════════════════════════════════════════════════
         * Finds the month with the highest total income within the last 12 months.
         * ═══════════════════════════════════════════════════════════════════════════ */
        public void GetBestSellingMonth(dynamic data, out string bestMonth, out double incomeOfBestMonth)
        {
            bestMonth = "";
            incomeOfBestMonth = 0;
            double best = 0;
            DateTime now = DateTime.Now;

            // Iterate over each of the last 12 months and sum their orders.
            for (int i = 0; i < 12; i++)
            {
                DateTime target = now.AddMonths(-i);
                double income = 0;
                foreach (var order in data.clients_orders)
                {
                    DateTime d = DateTime.Parse((string)order.date);
                    if (d.Month == target.Month && d.Year == target.Year)
                        income += (double)order.price;
                }

                // Keep track of the month with the highest income found so far.
                if (income > best)
                {
                    best = Math.Round(income, 2);
                    incomeOfBestMonth = best;
                    bestMonth = target.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                }
            }
        }

        /* ═══════════════════════════════════════════════════════════════════════════
         * Builds a bar chart showing total income for each of the last 12 months.
         * ═══════════════════════════════════════════════════════════════════════════ */
        private PlotModel CreatePlotModel(dynamic data)
        {
            bool dark = IsDarkTheme();

            // Accent color for the bars, adapted to the active theme.
            OxyColor accentColor = dark
                ? OxyColor.FromRgb(124, 106, 245)
                : OxyColor.FromRgb(91, 78, 232);

            var model = new PlotModel { Title = "Monthly incomes (last 12 months)" };

            // Apply background and text colors from GraphicViewModel's shared helper.
            GraphicViewModel.ApplyThemeToModel(model);

            // Axis Y (Income)
            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Income (€)",
                Minimum = 0,
                MinimumPadding = 0,
                MaximumPadding = 0.2,
                Key = "value"
            };
            ApplyThemeToAxis(valueAxis);

            // Axis X (Month)
            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Month",
                Angle = 45,
                Key = "category"
            };
            ApplyThemeToAxis(categoryAxis);

            // Bar series: one bar per month.
            var barSeries = new BarSeries
            {
                Title = "Incomes",
                FillColor = accentColor,
                StrokeColor = OxyColor.FromArgb(60, 0, 0, 0),
                StrokeThickness = 1,
                XAxisKey = "value",
                YAxisKey = "category"
            };

            // Iterate from 11 months ago up to the current month(chronological order).
            DateTime now = DateTime.Now;
            for (int i = 11; i >= 0; i--)
            {
                DateTime target = now.AddMonths(-i);
                double income = 0;

                foreach (var order in data.clients_orders)
                {
                    DateTime d = DateTime.Parse((string)order.date);
                    if (d.Month == target.Month && d.Year == target.Year)
                        income += (double)order.price;
                }

                barSeries.Items.Add(new BarItem { Value = income });
                categoryAxis.Labels.Add(target.ToString("MMM yy", CultureInfo.InvariantCulture));
            }

            model.Series.Add(barSeries);
            model.Axes.Add(valueAxis);
            model.Axes.Add(categoryAxis);
            model.InvalidatePlot(true);
            return model;
        }

        /* ═══════════════════
         * Helpers de tema
         * ═══════════════════ */
        private static bool IsDarkTheme() =>
            // Returns true if the active ResourceDictionary is the dark theme
            Application.Current.Resources.MergedDictionaries
                .Any(d => d.Source != null &&
                          d.Source.OriginalString.Contains("DarkTheme"));

        private static void ApplyThemeToAxis(Axis axis)
        {
            // Applies axis colors (labels, grid lines, tick marks) based on the active theme.
            if (IsDarkTheme())
            {
                axis.TextColor = OxyColors.White;
                axis.TitleColor = OxyColor.FromRgb(155, 163, 190);
                axis.TicklineColor = OxyColor.FromRgb(44, 47, 60);
                axis.MajorGridlineColor = OxyColor.FromArgb(40, 255, 255, 255);
                axis.MinorGridlineColor = OxyColor.FromArgb(20, 255, 255, 255);
                axis.AxislineColor = OxyColor.FromRgb(44, 47, 60);
            }
            else
            {
                axis.TextColor = OxyColor.FromRgb(26, 29, 46);
                axis.TitleColor = OxyColor.FromRgb(107, 112, 128);
                axis.TicklineColor = OxyColor.FromRgb(210, 212, 222);
                axis.MajorGridlineColor = OxyColor.FromArgb(60, 0, 0, 0);
                axis.MinorGridlineColor = OxyColor.FromArgb(30, 0, 0, 0);
                axis.AxislineColor = OxyColor.FromRgb(210, 212, 222);
            }
        }

        /* ════════════════════════════════════════════════════════════════════
         * Constructor: initializes services, loads data and sets up commands
         * ════════════════════════════════════════════════════════════════════ */
        public HomeViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _clientService = new ClientService();

            dynamic data = _clientService.LoadClientsData();

            TotalIncome = GetTotalIncome(data) + "€";
            IncomeMonth = GetIncomeMonth(data) + "€";

            string bestMonth;
            double incomeBest;
            GetBestSellingMonth(data, out bestMonth, out incomeBest);
            BestSellingMonth = bestMonth;
            IncomeOfbestSellingMonth = incomeBest;

            TotalClients = data?.clients?.Count ?? 0;

            PlotModel = CreatePlotModel(data);
            loadResources();
        }

        /* ════════════════════════════════════════════════════════════════════════════════
         * Required by INotifyPropertyChanged to notify the view when a property changes
         * ════════════════════════════════════════════════════════════════════════════════ */

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}