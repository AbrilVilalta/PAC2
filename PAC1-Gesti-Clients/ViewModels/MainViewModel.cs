
/* ════════════════════════════════════════════
 * MAINVIEWMODEL
 * ════════════════════════════════════════════ */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WPF_MVVM_SPA_Template.Models;
using WPF_MVVM_SPA_Template.Services;
using WPF_MVVM_SPA_Template.Views;

namespace WPF_MVVM_SPA_Template.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {

        /* ════════════════════════════════════════════════════
         * Reference to all the ViewModels and ClientService
         * ════════════════════════════════════════════════════ */

        private readonly HomeView _homeView;
        private readonly CRUDView _crudView;
        private readonly SettingView _settingView;
        private readonly Form _formView;
        private readonly GraphicView _graphicView;

        /* ════════════════════════════════════════════════════════════════
         * Child ViewModels
         * Each one is exposed as a public property so child VMs can
         * cross-reference each other through this parent if needed.
         * ════════════════════════════════════════════════════════════════ */
        public HomeViewModel HomeVM { get; set; }
        public CRUDViewModel CrudVM { get; set; }
        public GraphicViewModel? GraphicVM { get; set; }
        public SettingViewModel SettingVM { get; set; }
        public FormViewModel FormVM { get; set; }

        /* ══════════════════════════════════════
        * Shared service and data collections
        * ══════════════════════════════════════ */
        public ClientService _clientService { get; set; }
        public ObservableCollection<Client> Clients { get; set; }
        public ObservableCollection<Order> Orders { get; set; }

        /* ═════════════
         * Navigation 
         * ═════════════ */

        private object? _currentView;
        public object? CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        private string _selectedView = "Home";
        public string SelectedView
        {
            get => _selectedView;
            set
            {
                // each time it switches to another view that needs
                // up-to-date information, reload the data.
                _selectedView = value;
                OnPropertyChanged();
                if (value == "Client")
                    CrudVM.loadResources();
                else if (value == "Home")
                    HomeVM.loadResources();
                ChangeView();
            }
        }

        /* ══════════════════
         * Change the view
         * ══════════════════ */
        private void ChangeView()
        {
            switch (SelectedView)
            {
                case "Home": CurrentView = _homeView; break;
                case "Client": CurrentView = _crudView; break;
                case "Setting": CurrentView = _settingView; break;
                case "Form": CurrentView = _formView; break;
                case "Graphic": CurrentView = _graphicView; break;
            }
        }

        /* ══════════════════
         * Reload Orders
         * ══════════════════ */
        public void ReloadOrders()
        {
            Orders.Clear();
            var data = _clientService.LoadClientsData();
            foreach (var o in data.clients_orders)
            {
                Orders.Add(new Order
                {
                    IdOrd = (int)o.idOrd,
                    IdPro = (int)o.idPro,
                    IdCli = (int)o.idCli,
                    Units = (int)o.units,
                    UnitPrice = (double)o.unitPrice,
                    Price = (double)o.price,
                    Status = (string)o.status,
                    Date = DateTime.ParseExact((string)o.date, "dd/MM/yyyy",
                           System.Globalization.CultureInfo.InvariantCulture)
                });
            }
        }

        /* ════════════════════════════════════════════════════════════════════
         * Constructor: initializes services, loads data and sets up commands
         * ════════════════════════════════════════════════════════════════════ */
        public MainViewModel()
        {
            _clientService = new ClientService();

            // Inicialitzate the objects
            Clients = new ObservableCollection<Client>();
            Orders = new ObservableCollection<Order>();

            var data = _clientService.LoadClientsData();

            // Populate the shared Clients collection
            foreach (var c in data.clients)
            {
                Clients.Add(new Client
                {
                    IdCli = (int)c.IdCli,
                    DNI = (string)c.DNI,
                    Name = (string)c.Name,
                    Lastname = (string?)c.Lastname,
                    Mail = (string?)c.Mail,
                    Phone = (string?)c.Phone,
                    CreatedAt = c.date != null ? (DateTime)c.date : DateTime.Now
                });
            }

            // Populate the shared Orders collection
            foreach (var o in data.clients_orders)
            {
                Orders.Add(new Order
                {
                    IdOrd = (int)o.idOrd,
                    IdPro = (int)o.idPro,
                    IdCli = (int)o.idCli,
                    Units = (int)o.units,
                    UnitPrice = (double)o.unitPrice,
                    Price = (double)o.price,
                    Status = (string)o.status,
                    Date = DateTime.ParseExact((string)o.date, "dd/MM/yyyy",
                           System.Globalization.CultureInfo.InvariantCulture)
                });
            }

            // Instantiate child ViewModels, passing 'this' so they can
            // access shared collections and navigate between views.

            CrudVM = new CRUDViewModel(this);
            HomeVM = new HomeViewModel(this);
            SettingVM = new SettingViewModel(this);
            FormVM = new FormViewModel(this);
            GraphicVM = new GraphicViewModel(this);

            // Create each View once and reuse the same instance on every navigation
            // to avoid re-rendering costs and preserve UI state.

            _homeView = new HomeView { DataContext = HomeVM };
            _crudView = new CRUDView { DataContext = CrudVM };
            _settingView = new SettingView { DataContext = SettingVM };
            _formView = new Form { DataContext = FormVM };
            _graphicView = new GraphicView { DataContext = GraphicVM };

            // Navigate to the Home view as the default starting page.
            SelectedView = "Home";
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