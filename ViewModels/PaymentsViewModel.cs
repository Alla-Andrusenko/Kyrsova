using ParkingInterface.Data;
using ParkingInterface.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ParkingInterface.ViewModels
{
    public class PaymentsViewModel : ViewModelBase
    {
        public ObservableCollection<Payment> Payments { get; set; }
        public ObservableCollection<Contract> Contracts { get; set; } // Список контрактів для ComboBox

        private Payment newPayment;
        public Payment NewPayment
        {
            get { return newPayment; }
            set { newPayment = value; OnPropertyChanged(); }
        }

        private Payment selectedPayment;
        public Payment SelectedPayment
        {
            get { return selectedPayment; }
            set
            {
                selectedPayment = value;
                OnPropertyChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public PaymentsViewModel()
        {
            LoadData();
            NewPayment = new Payment
            {
                PaymentDate = DateTime.Now
            };

            AddCommand = new RelayCommand(AddPayment);
            DeleteCommand = new RelayCommand(DeletePayment, CanDeletePayment);
        }

        private void LoadData()
        {
            Payments = new ObservableCollection<Payment>(DataAccess.GetPayments());
            Contracts = new ObservableCollection<Contract>(DataAccess.GetContracts());
            OnPropertyChanged(nameof(Payments));
            OnPropertyChanged(nameof(Contracts));
        }

        private void AddPayment(object obj)
        {
            if (!IsValid(NewPayment))
                return;

            DataAccess.InsertPayment(NewPayment);
            LoadData(); // Автооновлення списку платежів
            NewPayment = new Payment
            {
                PaymentDate = DateTime.Now
            };
        }

        private bool IsValid(Payment payment)
        {
            if (payment.ContractId == 0)
            {
                MessageBox.Show("Виберіть договір.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (payment.Amount <= 0)
            {
                MessageBox.Show("Сума має бути більше нуля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void DeletePayment(object obj)
        {
            if (MessageBox.Show("Ви впевнені, що хочете видалити цей платіж?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DataAccess.DeletePayment(SelectedPayment.PaymentId);
                LoadData(); // Автооновлення списку платежів
                SelectedPayment = null;
            }
        }

        private bool CanDeletePayment(object obj)
        {
            return SelectedPayment != null;
        }
    }
}