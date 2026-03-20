
/* ════════════════════════════════════════════
 * GRAPHICVIEWMODEL
 * ════════════════════════════════════════════ */

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WPF_MVVM_SPA_Template.ViewModels
{
    /* ══════════════════════════════════════
     * Defines the available chart models.
     * ══════════════════════════════════════ */

    public enum ChartType { Line, Bar }

    internal class GraphicViewModel : INotifyPropertyChanged
    {
        /* ════════════════════════════════════════════════════
         * Reference to the main ViewModel
         * ════════════════════════════════════════════════════ */

        private readonly MainViewModel _mainViewModel;


        /* ══════════════════════════════════════════════════════════════════════
         * Backing field and property for the OxyPlot model bound to the View.
         * ══════════════════════════════════════════════════════════════════════ */

        private PlotModel _myModel;
        public PlotModel MyModel
        {
            get => _myModel;
            private set { _myModel = value; OnPropertyChanged(); }
        }

        /* ═════════════════════════════════════════════════════════════════════════════════
         * Currently selected chart type (Line or Bar). Triggers a plot refresh on change.
         * ═════════════════════════════════════════════════════════════════════════════════ */

        private ChartType _selectedChartType = ChartType.Line;
        public ChartType SelectedChartType
        {
            get => _selectedChartType;
            set
            {
                _selectedChartType = value;
                OnPropertyChanged();
                UpdatePlot();
            }
        }

        /* ═══════════════════════════════════════════════════════════════════════════════════════════
         * Exposes all ChartType values so the View can bind them to a ComboBox or similar control.
         * ═══════════════════════════════════════════════════════════════════════════════════════════ */
        public IEnumerable<ChartType> ChartTypes => Enum.GetValues(typeof(ChartType)).Cast<ChartType>();

        /* ════════════════════════════════════════
         * RelayCommands bound to the view buttons
         * ════════════════════════════════════════ */
        public RelayCommand BackCommand { get; set; }

        /* ════════════════════════════════
         * Return to client view
         * ════════════════════════════════ */
        private void Back()
        {
            _mainViewModel.SelectedView = "Client";
        }

        /* ════════════════════════════════════════
         * Detecte when the Dark theme is active
         * ════════════════════════════════════════ */
        private static bool IsDarkTheme()
        { 
            var dicts = Application.Current.Resources.MergedDictionaries;
            return dicts.Any(d => d.Source != null && 
                d.Source.OriginalString.Contains("DarkTheme.xaml"));
        }

        /* ════════════════════════════════════════════
         * Apply OxiPlot colors according to the theme
         * ════════════════════════════════════════════ */
        public static void ApplyThemeToModel(PlotModel model)
        {
            if (IsDarkTheme())
            {
                model.Background = OxyColor.FromArgb(0, 0, 0, 0);   // transparente
                model.PlotAreaBackground = OxyColor.FromArgb(0, 0, 0, 0);
                model.TextColor = OxyColors.White;
                model.TitleColor = OxyColors.White;
                model.SubtitleColor = OxyColor.FromRgb(155, 163, 190);
                model.PlotAreaBorderColor = OxyColor.FromRgb(44, 47, 60);
            }
            else
            {
                model.Background = OxyColor.FromArgb(0, 0, 0, 0);
                model.PlotAreaBackground = OxyColor.FromArgb(0, 0, 0, 0);
                model.TextColor = OxyColor.FromRgb(26, 29, 46);
                model.TitleColor = OxyColor.FromRgb(26, 29, 46);
                model.SubtitleColor = OxyColor.FromRgb(107, 112, 128);
                model.PlotAreaBorderColor = OxyColor.FromRgb(210, 212, 222);
            }
        }

        /* ════════════════════════════════════════════
         * Apply colors to an Axis based on the theme
         * ════════════════════════════════════════════ */
        private static void ApplyThemeToAxis(Axis axis)
        {
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

        /* ═══════════════════════════════════════════════════════════════════════════════════
         * Rebuilds the chart from scratch using the currently selected client's order data.
         * ═══════════════════════════════════════════════════════════════════════════════════ */
        private void UpdatePlot()
        {
            // Clear previous series and axes before rebuilding.
            MyModel.Series.Clear();
            MyModel.Axes.Clear();

            /* ════════════════════════════════════════════
             * Reapply the theme each time it is updated
             * ════════════════════════════════════════════ */
            ApplyThemeToModel(MyModel);

            var client = _mainViewModel.CrudVM.SelectedClient;

            // Update the chart title to reflect the selected client (or a default if none).
            if (client != null)
                MyModel.Title = $"Monthly expenses: {client.Name}";
            else
                MyModel.Title = "Monthly expenses";

            // Nothing to plot if no client is selected or no orders exist.
            if (client == null || _mainViewModel.Orders == null)
            {
                MyModel.InvalidatePlot(true);
                return;
            }

            // Aggregate the client's orders by year and month.
            var monthlyData = _mainViewModel.Orders
                .Where(o => o.IdCli == client.IdCli)
                .GroupBy(o => new { o.Date.Year, o.Date.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(x => x.Price)
                })
                .ToList();

            // Elige el color según el tema actual.
            OxyColor accentColor = IsDarkTheme() 
                ? OxyColor.FromRgb(124, 106, 245) 
                : OxyColor.FromRgb(91, 78, 232);

            // Build a continuous 12-month window ending at the current month.
            var allMonths = Enumerable.Range(0, 12)
                .Select(i => DateTime.Now.AddMonths(-11 + i))
                .Select(d => new
                {
                    d.Year,
                    d.Month,
                    Total = monthlyData
                        // Months with no orders default to 0.
                        .FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0.0
                })
                .ToList();

            // If chart model is Line
            if (SelectedChartType == ChartType.Line)
            {
                // Axes X (Months)
                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Month",
                    Angle = 45
                };

                foreach (var item in allMonths)
                {
                    var date = new DateTime(item.Year, item.Month, 1);
                    categoryAxis.Labels.Add(date.ToString("MMM yyyy", new CultureInfo("en")));
                }

                MyModel.Axes.Add(categoryAxis);

                // Axes Y (Total spend)
                MyModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Total spend (€)",
                    Minimum = 0,
                    MinimumPadding = 0,
                    MaximumPadding = 0.2
                });

                // Line series: one data point per month.
                var lineSeries = new LineSeries
                {
                    Title = $"Expense of {client.Name}",
                    Color = OxyColors.SteelBlue,
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 5,
                    MarkerFill = OxyColors.SteelBlue,
                    MarkerStroke = OxyColors.White,
                    MarkerStrokeThickness = 1.5
                };

                for (int i = 0; i < allMonths.Count; i++)
                    lineSeries.Points.Add(new DataPoint(i, allMonths[i].Total));

                MyModel.Series.Add(lineSeries);
            }
            else // If the model is bar
            {
                // Axes Y (Total spend)
                MyModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Total spend (€)",
                    Minimum = 0,
                    MinimumPadding = 0,
                    MaximumPadding = 0.2,
                    Key = "value"
                });

                // Axes X (Month)
                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Month",
                    Angle = 45,
                    Key = "category"
                };

                foreach (var item in allMonths)
                {
                    var date = new DateTime(item.Year, item.Month, 1);
                    categoryAxis.Labels.Add(date.ToString("MMM yyyy", new CultureInfo("en")));
                }

                MyModel.Axes.Add(categoryAxis);

                // Bar series: one bar per month.
                var barSeries = new BarSeries
                {
                    Title = $"Expense of {client.Name}",
                    FillColor = OxyColors.SteelBlue,
                    StrokeColor = OxyColors.Black,
                    StrokeThickness = 1,
                    BarWidth = 0.5,
                    XAxisKey = "value",
                    YAxisKey = "category"
                };

                foreach (var item in allMonths)
                    barSeries.Items.Add(new BarItem(item.Total));

                MyModel.Series.Add(barSeries);
            }

            // Notify OxyPlot to re-render with the updated data.
            MyModel.InvalidatePlot(true);
        }

        /* ════════════════════════════════════════════════════════════════════
         * Constructor: initializes services, loads data and sets up commands
         * ════════════════════════════════════════════════════════════════════ */

        public GraphicViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _myModel = new PlotModel { Title = "Monthly allowances" };

            _mainViewModel.CrudVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(CRUDViewModel.SelectedClient))
                    UpdatePlot();
            };

            BackCommand = new RelayCommand(x => Back());
        }

        /* ════════════════════════════════════════════════════════════════════════════════
         * Required by INotifyPropertyChanged to notify the view when a property changes
         * ════════════════════════════════════════════════════════════════════════════════ */

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}