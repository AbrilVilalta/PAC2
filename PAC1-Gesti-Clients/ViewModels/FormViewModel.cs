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

            // Name validation
            if (Name == string.Empty)
            {
                CustomDialog.Show("The name cannot be empty.", "Validation Error");
                isValid = false;
            } else if (Name.Length <= 1)
            {
                CustomDialog.Show("The Name is too short.", "Validation Error");
                isValid = false;
            }

            // Name validation
            if (!Regex.IsMatch(Lastname, @"^[a-zA-ZÀ-ÿ\s]*$"))
            {
                CustomDialog.Show("The last name cannot contain special characters.", "Validation Error");
                isValid = false;
            }

            // Validate the DNI
            if (!string.IsNullOrEmpty(DNI))
            {
                try
                {
                    if (!int.TryParse(DNI.Substring(0, 8), out int numDNI))
                    {
                        CustomDialog.Show("Invalid DNI. The first 8 characters must be digits.", "Validation Error");
                        isValid = false;
                    }
                    else
                    {
                        List<string> t_LletraDni = new List<string>
                        {
                            "T","R","W","A","G","M","Y","F","P","D","X","B","N","J","Z","S","Q","V","H","L","C","K","E"
                        };
                        string lletraDni = DNI[8].ToString().ToUpper();
                        int calcul = numDNI % 23;

                        if (lletraDni != t_LletraDni[calcul])
                        {
                            CustomDialog.Show(
                                $"Invalid DNI. With the number {DNI.Substring(0, 8)} the correct letter is '{t_LletraDni[calcul]}', not '{lletraDni}'.",
                                "Validation Error");
                            isValid = false;
                        }
                    }
                }
                catch
                {
                    CustomDialog.Show("Invalid DNI format. It must be 8 digits followed by a letter.", "Validation Error");
                    isValid = false;
                }
            }
            else
            {
                CustomDialog.Show("The DNI cannot be empty.", "Validation Error");
                isValid = false;
            }

            // If the phone is not null, validate it
            string digitsOnly = Phone.Replace("-", "").Replace(" ", "");
            if (!string.IsNullOrEmpty(digitsOnly))
            {
                if (digitsOnly.Length < 9 || !int.TryParse(digitsOnly, out _))
                {
                    CustomDialog.Show("Invalid phone number. It must have at least 9 digits.", "Validation Error");
                    isValid = false;
                }
            }

            // If the mail is not null, validate it
            if (!string.IsNullOrWhiteSpace(Mail))
            {
                try
                {
                    var addr = new MailAddress(Mail);
                    if (addr.Address != Mail)
                    {
                        CustomDialog.Show("Invalid email address.", "Validation Error");
                        isValid = false;
                    }
                }
                catch
                {
                    CustomDialog.Show("Invalid email address.", "Validation Error");
                    isValid = false;
                }
            }
            else
            {
                CustomDialog.Show("The email cannot be empty.", "Validation Error");
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