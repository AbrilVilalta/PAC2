/* ════════════════════════════════════════════
 * FORMVIEWMODEL
 * ════════════════════════════════════════════ */

using DLL_AbrilVilalta_MosiesCuartero;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using WPF_MVVM_SPA_Template.Models;
using WPF_MVVM_SPA_Template.Services;
using WPF_MVVM_SPA_Template.Views; // CustomDialog

namespace WPF_MVVM_SPA_Template.ViewModels
{
    class FormViewModel : INotifyPropertyChanged
    {

        /* ════════════════════════════════════════════════════
         * Reference to the main ViewModel and ClientService
         * ════════════════════════════════════════════════════ */

        private readonly MainViewModel _mainViewModel;
        private readonly ClientService _clientService;

        /* ════════════════════════════════════════════════════════════════════════════
         * Observable collection of clients, notifies the view when the list changes
         * ════════════════════════════════════════════════════════════════════════════ */
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();

        /* ════════════════════════════════════════
         * RelayCommands bound to the view buttons
         * ════════════════════════════════════════ */
        public RelayCommand SendCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        /* ════════════════════════════════════════════════
         * Stores the raw JSON data loaded from the file
         * ════════════════════════════════════════════════ */

        private dynamic? myArray;

        /* ══════════════════════════════════════════════════════
         * Indicates whether the DNI field has passed validation
         * ══════════════════════════════════════════════════════ */

        private bool _validDNI = false;
        public bool ValidDNI
        {
            get { return _validDNI; }
            set { _validDNI = value; OnPropertyChanged(); }
        }

        /* ════════════════════════════════════════════════════════════════════════
         * Form fields — each property notifies the view when its value changes
         * ════════════════════════════════════════════════════════════════════════ */

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private string _lastname = "";
        public string Lastname
        {
            get { return _lastname; }
            set { _lastname = value; OnPropertyChanged(); }
        }

        private string _dni = "";
        public string DNI
        {
            get { return _dni; }
            set { _dni = value; OnPropertyChanged(); }
        }

        private string _mail = "";
        public string Mail
        {
            get { return _mail; }
            set { _mail = value; OnPropertyChanged(); }
        }

        private string _phone = "";
        public string Phone
        {
            get { return _phone; }
            set { _phone = value; OnPropertyChanged(); }
        }

        public DateTime CreatedAtDisplay
        {
            get
            {
                if (SelectedClient != null)
                    return SelectedClient.CreatedAt;

                return DateTime.Now;
            }
        }

        private bool _isDNIValid = false;
        public bool IsDNIValid
        {
            get => _isDNIValid;
            set { _isDNIValid = value; OnPropertyChanged(); }
        }

        private bool _isEmailValid = false;
        public bool IsEmailValid
        {
            get => _isEmailValid;
            set { _isEmailValid = value; OnPropertyChanged(); }
        }

        private bool _isNameValid = false;
        public bool IsNameValid
        {
            get => _isNameValid;
            set { _isNameValid = value; OnPropertyChanged(); }
        }

        private bool _isLastNameValid = false;
        public bool IsLastNameValid
        {
            get => _isLastNameValid;
            set { _isLastNameValid = value; OnPropertyChanged(); }
        }

        private bool _isPhoneValid = false;
        public bool IsPhoneValid
        {
            get => _isPhoneValid;
            set { _isPhoneValid = value; OnPropertyChanged(); }
        }

        /* ════════════════════════════════════════════════════════════════════════
         * Currently selected client — also notifies CreatedAtDisplay when changed
         * ════════════════════════════════════════════════════════════════════════ */

        private Client? _selectedClient;
        public Client? SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreatedAtDisplay));
            }
        }

        /* ═════════════════
         * Load resources
         * ═════════════════ */

        private void loadResources()
        {
            myArray = _clientService.LoadClientsData();
        }

        /* ═════════════════════════
         * Load a client to edit
         * ═════════════════════════ */
        public void LoadClientForEdit(Client client)
        {
            SelectedClient = client;
            Name = client.Name!;
            Lastname = client.Lastname!;
            DNI = client.DNI;
            Mail = client.Mail!;
            Phone = client.Phone!;
        }

        /* ═════════════════════════
         * Validate inserted data
         * ═════════════════════════ */
        public bool Validate()
        {
            bool isValid = true;

            if (!IsNameValid)
            {
                CustomDialog.Show("Invalid Name.", "Validation Error");
                isValid = false;
            }

            if (!IsLastNameValid)
            {
                CustomDialog.Show("Invalid LastName.", "Validation Error");
                isValid = false;
            }

            if (!IsDNIValid)
            {
                CustomDialog.Show("Invalid DNI.", "Validation Error");
                isValid = false;
            }

            if (!IsPhoneValid)
            {
                CustomDialog.Show("Invalid phone address.", "Validation Error");
                isValid = false;
            }

            if (!IsEmailValid)
            {
                CustomDialog.Show("Invalid email address.", "Validation Error");
                isValid = false;
            }

            return isValid;
        }

        /* ═════════════════════════
         * To clean the form
         * ═════════════════════════ */

        public void ClearForm()
        {
            SelectedClient = null;
            Name = "";
            Lastname = "";
            DNI = "";
            Mail = "";
            Phone = "";
        }

        /* ═════════════════════════
         * Generate random orders
         * ═════════════════════════ */

        private void GenerateRandomOrders(int clientId)
        {
            var random = new Random();
            myArray = _clientService.LoadClientsData();

            int lastOrdId = 0;
            if (myArray["clients_orders"] != null && myArray["clients_orders"].Count > 0)
                lastOrdId = (int)myArray["clients_orders"].Last["idOrd"];

            var products = myArray?["products"];
            if (products == null || products.Count == 0) return;

            int numOrders = random.Next(1, 5);
            var statuses = new[] { "Completed", "Pending", "Shipped" };

            for (int i = 0; i < numOrders; i++)
            {
                var product = products[random.Next(0, (int)products.Count)];
                int units = random.Next(1, 6);
                double unitPrice = (double)product["price"];
                double totalPrice = Math.Round(unitPrice * units, 2);
                int daysBack = random.Next(0, 180);
                string date = DateTime.Now.AddDays(-daysBack).ToString("dd/MM/yyyy");

                myArray!["clients_orders"].Add(Newtonsoft.Json.Linq.JObject.FromObject(new
                {
                    idOrd = lastOrdId + i + 1,
                    idPro = (int)product["idPro"],
                    idCli = clientId,
                    units = units,
                    unitPrice = unitPrice,
                    price = totalPrice,
                    status = statuses[random.Next(0, statuses.Length)],
                    date = date
                }));
            }

            _clientService.SaveToFile(myArray!);
        }

        /* ═════════════════════════
         * Send Form
         * ═════════════════════════ */

        private void SendForm()
        {
            int Id;
            DateTime CreatedAt;

            myArray = _clientService.LoadClientsData();

            if (SelectedClient != null)
            {
                Id = SelectedClient.IdCli;
                CreatedAt = SelectedClient.CreatedAt;

                dynamic? itemToRemove = null;
                foreach (var item in myArray!["clients"])
                {
                    if ((int)item.IdCli == SelectedClient.IdCli)
                    {
                        itemToRemove = item;
                        break;
                    }
                }
                if (itemToRemove != null)
                    myArray!["clients"].Remove(itemToRemove);
            }
            else
            {
                var clients = myArray?["clients"];
                Id = (clients != null && clients!.Count > 0)
                            ? ((int)clients!.Last["IdCli"]) + 1
                            : 1;
                CreatedAt = DateTime.Now;
            }

            var newClient = new Client
            {
                IdCli = Id,
                Name = Name,
                Lastname = Lastname,
                DNI = DNI,
                Mail = Mail,
                Phone = Phone,
                CreatedAt = CreatedAt
            };

            if (newClient != null)
            {
                myArray!["clients"].Add(Newtonsoft.Json.Linq.JObject.FromObject(new
                {
                    IdCli = newClient.IdCli,
                    newClient.Name,
                    newClient.Lastname,
                    newClient.DNI,
                    newClient.Mail,
                    newClient.Phone,
                    date = newClient.CreatedAt
                }));

                _clientService.SaveToFile(myArray!);

                if (SelectedClient == null)
                    GenerateRandomOrders(newClient.IdCli);

                _mainViewModel.ReloadOrders();
                ClearForm();
                _mainViewModel.SelectedView = "Client";
            }
        }

        /* ═════════════════════════
         * Cancel sending the form
         * ═════════════════════════ */

        public bool Cancel()
        {
            _mainViewModel.SelectedView = "Client";
            return false;
        }

        /* ════════════════════════════════════════════════════════════════════
         * Constructor: initializes services, loads data and sets up commands
         * ════════════════════════════════════════════════════════════════════ */

        public FormViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _clientService = new ClientService();
            loadResources();

            SendCommand = new RelayCommand(x =>
            {
                if (!Validate()) return;
                SendForm();
            });

            CancelCommand = new RelayCommand(x =>
            {
                if (!Cancel()) return;
            });
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