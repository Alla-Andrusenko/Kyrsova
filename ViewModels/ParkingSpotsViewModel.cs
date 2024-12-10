using ParkingInterface.Data;
using ParkingInterface.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ParkingInterface.ViewModels
{
    public class ParkingSpotsViewModel : ViewModelBase
    {
        public ObservableCollection<ParkingSpot> ParkingSpots { get; set; }
        public ObservableCollection<Contract> Contracts { get; set; } // Список контрактів для ComboBox

        private ParkingSpot newSpot;
        public ParkingSpot NewSpot
        {
            get { return newSpot; }
            set
            {
                if (newSpot != value)
                {
                    newSpot = value;
                    OnPropertyChanged();
                }
            }
        }

        private ParkingSpot selectedSpot;
        public ParkingSpot SelectedSpot
        {
            get { return selectedSpot; }
            set { selectedSpot = value; OnPropertyChanged(); DeleteCommand.RaiseCanExecuteChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public ParkingSpotsViewModel()
        {
            LoadData();
            NewSpot = new ParkingSpot();
        }

        private void LoadData()
        {
            ParkingSpots = new ObservableCollection<ParkingSpot>(DataAccess.GetParkingSpots());
            Contracts = new ObservableCollection<Contract>(DataAccess.GetContracts());
            OnPropertyChanged(nameof(ParkingSpots));
            OnPropertyChanged(nameof(Contracts));
        }

        private void AddSpot(object obj)
        {
            if (!IsValid(NewSpot))
                return;

            DataAccess.InsertParkingSpot(NewSpot);
            LoadData(); // Автооновлення списку паркувальних місць
            NewSpot = new ParkingSpot();
        }

        private bool IsValid(ParkingSpot spot)
        {
            if (spot.IsOccupied && spot.ContractId == null)
            {
                MessageBox.Show("Якщо місце зайняте, необхідно вибрати договір.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (spot.IsOccupied && spot.ContractId != null)
            {
                var contractExists = DataAccess.GetContracts().Exists(c => c.ContractId == spot.ContractId);
                if (!contractExists)
                {
                    MessageBox.Show("Вибраний договір не існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            return true;
        }

        private void DeleteSpot(object obj)
        {
            if (MessageBox.Show("Ви впевнені, що хочете видалити це паркувальне місце?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DataAccess.DeleteParkingSpot(SelectedSpot.SpotId);
                LoadData(); // Автооновлення списку паркувальних місць
                SelectedSpot = null;
            }
        }

        private bool CanDeleteSpot(object obj)
        {
            return SelectedSpot != null;
        }
    }
}