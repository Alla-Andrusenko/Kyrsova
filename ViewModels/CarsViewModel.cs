using ParkingInterface.Data;
using ParkingInterface.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ParkingInterface.ViewModels
{
    public class CarsViewModel : ViewModelBase
    {
        public ObservableCollection<Car> Cars { get; set; }
        public ObservableCollection<Client> Clients { get; set; } // Список клієнтів для ComboBox

        private Car newCar;
        public Car NewCar
        {
            get { return newCar; }
            set { newCar = value; OnPropertyChanged(); }
        }

        private Car selectedCar;
        public Car SelectedCar
        {
            get { return selectedCar; }
            set { selectedCar = value; OnPropertyChanged(); DeleteCommand.RaiseCanExecuteChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public CarsViewModel()
        {
            LoadData();

            NewCar = new Car();

            AddCommand = new RelayCommand(AddCar);
            DeleteCommand = new RelayCommand(DeleteCar, CanDeleteCar);
        }

        private void LoadData()
        {
            Cars = new ObservableCollection<Car>(DataAccess.GetCars());
            Clients = new ObservableCollection<Client>(DataAccess.GetClients());
            OnPropertyChanged(nameof(Cars));
            OnPropertyChanged(nameof(Clients));
        }

        private void AddCar(object obj)
        {
            if (!IsValid(NewCar))
                return;

            DataAccess.InsertCar(NewCar);
            LoadData(); // Автооновлення списку автомобілів
            NewCar = new Car();
        }

        private bool IsValid(Car car)
        {
            if (string.IsNullOrWhiteSpace(car.LicensePlate))
            {
                MessageBox.Show("Введіть державний номер.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(car.Brand))
            {
                MessageBox.Show("Введіть марку автомобіля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(car.Model))
            {
                MessageBox.Show("Введіть модель автомобіля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (car.ClientId == 0)
            {
                MessageBox.Show("Виберіть клієнта-власника.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var clientExists = DataAccess.GetClients().Exists(c => c.ClientId == car.ClientId);
            if (!clientExists)
            {
                MessageBox.Show("Вибраний клієнт не існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void DeleteCar(object obj)
        {
            if (MessageBox.Show("Ви впевнені, що хочете видалити цей автомобіль?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DataAccess.DeleteCar(SelectedCar.CarId);
                LoadData(); // Автооновлення списку автомобілів
                SelectedCar = null;
            }
        }

        private bool CanDeleteCar(object obj)
        {
            return SelectedCar != null;
        }
    }
}