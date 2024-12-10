using ParkingInterface.Data;
using ParkingInterface.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ParkingInterface.ViewModels
{
    public class ClientsViewModel : ViewModelBase
    {
        public ObservableCollection<Client> Clients { get; set; }

        private Client newClient;
        public Client NewClient
        {
            get { return newClient; }
            set { newClient = value; OnPropertyChanged(); }
        }

        private Client selectedClient;
        public Client SelectedClient
        {
            get { return selectedClient; }
            set { selectedClient = value; OnPropertyChanged(); DeleteCommand.RaiseCanExecuteChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public ClientsViewModel()
        {
            LoadData();
            NewClient = new Client();

            AddCommand = new RelayCommand(AddClient);
            DeleteCommand = new RelayCommand(DeleteClient, CanDeleteClient);
        }

        private void LoadData()
        {
            Clients = new ObservableCollection<Client>(DataAccess.GetClients());
            OnPropertyChanged(nameof(Clients));
        }

        private void AddClient(object obj)
        {
            if (!IsValid(NewClient))
                return;

            DataAccess.InsertClient(NewClient);
            LoadData();
            NewClient = new Client();
        }

        private void DeleteClient(object obj)
        {
            if (MessageBox.Show("Ви впевнені, що хочете видалити цього клієнта та всі пов'язані з ним дані?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DataAccess.DeleteClient(SelectedClient.ClientId);
                LoadData();
                SelectedClient = null;

            }
        }

        private bool CanDeleteClient(object obj)
        {
            return SelectedClient != null;
        }

        private bool IsValid(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.LastName))
            {
                MessageBox.Show("Введіть прізвище.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(client.FirstName))
            {
                MessageBox.Show("Введіть ім'я.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(client.PhoneNumber))
            {
                MessageBox.Show("Введіть номер телефону.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}