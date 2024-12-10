using ParkingInterface.Data;
using ParkingInterface.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ParkingInterface.ViewModels
{
    public class ContractsViewModel : ViewModelBase
    {
        public ObservableCollection<Contract> Contracts { get; set; }
        public ObservableCollection<Client> Clients { get; set; } // Список клієнтів для ComboBox

        private Contract newContract;
        public Contract NewContract
        {
            get { return newContract; }
            set { newContract = value; OnPropertyChanged(); }
        }

        private Contract selectedContract;
        public Contract SelectedContract
        {
            get { return selectedContract; }
            set
            {
                selectedContract = value;
                OnPropertyChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public ContractsViewModel()
        {
            LoadData();

            NewContract = new Contract
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            AddCommand = new RelayCommand(AddContract);
            DeleteCommand = new RelayCommand(DeleteContract, CanDeleteContract);
        }

        private void LoadData()
        {
            Contracts = new ObservableCollection<Contract>(DataAccess.GetContracts());
            Clients = new ObservableCollection<Client>(DataAccess.GetClients());
            OnPropertyChanged(nameof(Contracts));
            OnPropertyChanged(nameof(Clients));
        }

        private void AddContract(object obj)
        {
            if (!IsValid(NewContract))
                return;

            DataAccess.InsertContract(NewContract);
            LoadData(); // Автооновлення списку контрактів
            NewContract = new Contract
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };
        }

        private bool IsValid(Contract contract)
        {
            if (contract.ClientId == 0)
            {
                MessageBox.Show("Виберіть клієнта.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (contract.StartDate >= contract.EndDate)
            {
                MessageBox.Show("Дата початку має бути раніше за дату завершення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (contract.Amount <= 0)
            {
                MessageBox.Show("Сума має бути більше нуля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void DeleteContract(object obj)
        {
            if (MessageBox.Show("Ви впевнені, що хочете видалити цей договір та всі пов'язані з ним дані?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DataAccess.DeleteContract(SelectedContract.ContractId);
                LoadData(); // Автооновлення списку контрактів
                SelectedContract = null;
            }
        }

        private bool CanDeleteContract(object obj)
        {
            return SelectedContract != null;
        }
    }
}