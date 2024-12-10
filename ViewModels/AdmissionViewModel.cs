using ParkingInterface.Data;
using ParkingInterface.Utilities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ParkingInterface.ViewModels
{
    public class AdmissionsViewModel : ViewModelBase
    {
        public ObservableCollection<Admission> Admissions { get; set; }
        public ObservableCollection<Car> Cars { get; set; } // Список автомобілів для ComboBox

        private Admission newAdmission;
        public Admission NewAdmission
        {
            get { return newAdmission; }
            set { newAdmission = value; OnPropertyChanged(); }
        }

        private Admission selectedAdmission;
        public Admission SelectedAdmission
        {
            get { return selectedAdmission; }
            set
            {
                selectedAdmission = value;
                OnPropertyChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public AdmissionsViewModel()
        {
            LoadData();
            NewAdmission = new Admission();

            AddCommand = new RelayCommand(AddAdmission);
            DeleteCommand = new RelayCommand(DeleteAdmission, CanDeleteAdmission);
        }

        private void LoadData()
        {
            Admissions = new ObservableCollection<Admission>(DataAccess.GetAdmissions());
            Cars = new ObservableCollection<Car>(DataAccess.GetCars());
            OnPropertyChanged(nameof(Admissions));
            OnPropertyChanged(nameof(Cars));
        }

        private void AddAdmission(object obj)
        {
            if (!IsValid(NewAdmission))
                return;

            DataAccess.InsertAdmission(NewAdmission);
            LoadData();
            NewAdmission = new Admission();
        }

        private void DeleteAdmission(object obj)
        {
            if (MessageBox.Show("Ви впевнені, що хочете видалити цей допуск?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DataAccess.DeleteAdmission(SelectedAdmission.AdmissionId);
                LoadData();
                SelectedAdmission = null;
            }
        }

        private bool CanDeleteAdmission(object obj)
        {
            return SelectedAdmission != null;
        }

        private bool IsValid(Admission admission)
        {
            if (admission.CarId == 0)
            {
                MessageBox.Show("Виберіть автомобіль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
    }
}