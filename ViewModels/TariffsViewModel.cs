using ParkingInterface.Data;
using ParkingInterface.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ParkingInterface.ViewModels
{
    public class TariffsViewModel : ViewModelBase
    {
        public ObservableCollection<Tariff> Tariffs { get; set; }

        private Tariff newTariff;
        public Tariff NewTariff
        {
            get { return newTariff; }
            set { newTariff = value; OnPropertyChanged(); }
        }

        private Tariff selectedTariff;
        public Tariff SelectedTariff
        {
            get { return selectedTariff; }
            set
            {
                selectedTariff = value;
                OnPropertyChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public TariffsViewModel()
        {
            Tariffs = new ObservableCollection<Tariff>(DataAccess.GetTariffs());
            NewTariff = new Tariff();

            AddCommand = new RelayCommand(AddTariff);
            DeleteCommand = new RelayCommand(DeleteTariff, CanDeleteTariff);
        }

        private void AddTariff(object obj)
        {
            if (!IsValid(NewTariff))
                return;

            DataAccess.InsertTariff(NewTariff);
            Tariffs.Add(NewTariff);
            NewTariff = new Tariff();
        }

        private void DeleteTariff(object obj)
        {
            if (MessageBox.Show("Ви впевнені, що хочете видалити цей тариф?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DataAccess.DeleteTariff(SelectedTariff.TariffId);
                Tariffs.Remove(SelectedTariff);
                SelectedTariff = null;
            }
        }

        private bool CanDeleteTariff(object obj)
        {
            return SelectedTariff != null;
        }

        private bool IsValid(Tariff tariff)
        {
            if (string.IsNullOrWhiteSpace(tariff.Name))
            {
                MessageBox.Show("Введіть назву тарифу.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (tariff.Price <= 0)
            {
                MessageBox.Show("Ціна має бути більше нуля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (tariff.Duration == TimeSpan.Zero)
            {
                MessageBox.Show("Вкажіть тривалість тарифу.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}