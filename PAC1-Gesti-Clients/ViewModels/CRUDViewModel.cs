
/* ════════════════════════════════════════════
 * CRUDVIEWMODEL
 * ════════════════════════════════════════════ */

using DLL_AbrilVilalta_MosiesCuartero;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using WPF_MVVM_SPA_Template.Models;
using WPF_MVVM_SPA_Template.Services;
using WPF_MVVM_SPA_Template.Views;

namespace WPF_MVVM_SPA_Template.ViewModels
{
    /* ═════════════════════════════════════════════════════════════════════════════════
     * Inherits from INotifyPropertyChanged to enable property binding with the view.
     * ═════════════════════════════════════════════════════════════════════════════════ */
    class CRUDViewModel : INotifyPropertyChanged
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

        /* ═══════════════════════════════════════
        * Currently selected client in the view
        * ════════════════════════════════════════ */

        private Client? _selectedClient;
        public Client? SelectedClient
        {
            get { return _selectedClient; }
            set { _selectedClient = value; OnPropertyChanged(); } // When the var change,
                                                                  // call the OnPropertyChanged() function
        }

        /* ════════════════════════════════════════
         * RelayCommands bound to the view buttons
         * ════════════════════════════════════════ */

        public RelayCommand DelClientCommand { get; set; } 
        public RelayCommand FromAddCommand { get; set; }
        public RelayCommand FromEditCommand { get; set; }
        public RelayCommand GraphicCommand { get; set; }

        /* ═════════════════════════════════════════════════════════════════
         * Loads clients from the JSON file into the ObservableCollection
         * ═════════════════════════════════════════════════════════════════ */

        private dynamic? myArray;
        public void loadResources()
        {
            Clients.Clear();
            myArray = _clientService.LoadClientsData();

            // If the clients array is not null, map each item to a Client object
            if (myArray?["clients"] != null)
                foreach (var item in myArray.clients)
                {
                    Clients.Add(new Client
                    {
                        IdCli = (int)item["IdCli"],
                        Name = (string)item["Name"],
                        Lastname = (string)item["Lastname"],
                        DNI = (string)item["DNI"],
                        Mail = (string)item["Mail"],
                        Phone = (string)item["Phone"],
                        CreatedAt = (DateTime)item["date"]
                    });
                }
        }

        /* ═════════════════════════════════════════════════════════════════
         * Asks for confirmation and deletes the selected client
         * ═════════════════════════════════════════════════════════════════ */
        private void DelClient(Client? client)
        {
            if (client == null) return;

            MessageBoxResult result = DeleteDialog.Show(
                 "Are you sure you want to delete this client?",
                 "Confirm Delete"
             );

            if (result != MessageBoxResult.Yes) return;

            dynamic? itemToRemove = null;

            foreach (var item in myArray!["clients"])
            {
                if ((int)item.IdCli == client.IdCli)
                {
                    itemToRemove = item;
                }
            }

            if (itemToRemove != null)
            {
                myArray["clients"].Remove(itemToRemove);
                _clientService.SaveToFile(myArray);
            }

            Clients.Remove(client);
        }

        /* ════════════════════════════════════════════════════════════════════
         * Navigates to the form view, loading the selected client for editing
         * or clearing the form if no client is selected
         * ════════════════════════════════════════════════════════════════════ */

        private void OpenFormEdit(Client? client)
        {
            if (client != null)
                _mainViewModel.FormVM.LoadClientForEdit(client);
            
            _mainViewModel.SelectedView = "Form";
        }
        private void OpenFormAdd(Client? client)
        {
            _mainViewModel.FormVM.ClearForm();
            _mainViewModel.SelectedView = "Form";
        }

        /* ═══════════════════════════════════
         * Navigates to the graphic view
         * ═══════════════════════════════════ */

        private void OpenGraphic(Client? client)
        {
            _mainViewModel.SelectedView = "Graphic";
        }

        /* ════════════════════════════════════════════════════════════════════
         * Constructor: initializes services, loads data and sets up commands
         * ════════════════════════════════════════════════════════════════════ */

        public CRUDViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _clientService = new ClientService();
            loadResources();
            // 
            DelClientCommand = new RelayCommand(x => DelClient(SelectedClient));
            FromAddCommand = new RelayCommand(x => OpenFormAdd(SelectedClient));
            FromEditCommand = new RelayCommand(x => OpenFormEdit(SelectedClient));
            GraphicCommand = new RelayCommand(x => OpenGraphic(SelectedClient));
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
