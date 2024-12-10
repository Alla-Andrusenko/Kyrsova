using ParkingInterface.Utilities;
using System.Windows.Input;

namespace ParkingInterface.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get { return currentViewModel; }
            set { currentViewModel = value; OnPropertyChanged(); }
        }

        // Команди для навігації
        public ICommand ShowClientsCommand { get; }
        public ICommand ShowCarsCommand { get; }
        public ICommand ShowContractsCommand { get; }
        public ICommand ShowParkingSpotsCommand { get; }
        public ICommand ShowPaymentsCommand { get; }
        public ICommand ShowTariffsCommand { get; }
        public ICommand ShowEntranceRecordsCommand { get; }
        public ICommand ShowAdmissionsCommand { get; }

        public MainWindowViewModel()
        {
            // Встановлюємо початковий ViewModel
            CurrentViewModel = new ClientsViewModel();

            // Ініціалізуємо команди
            ShowClientsCommand = new RelayCommand(ShowClients);
            ShowCarsCommand = new RelayCommand(ShowCars);
            ShowContractsCommand = new RelayCommand(ShowContracts);
            ShowParkingSpotsCommand = new RelayCommand(ShowParkingSpots);
            ShowPaymentsCommand = new RelayCommand(ShowPayments);
            ShowTariffsCommand = new RelayCommand(ShowTariffs);
            ShowEntranceRecordsCommand = new RelayCommand(ShowEntranceRecords);
            ShowAdmissionsCommand = new RelayCommand(ShowAdmissions);
        }

        private void ShowClients(object obj)
        {
            CurrentViewModel = new ClientsViewModel();
        }

        private void ShowCars(object obj)
        {
            CurrentViewModel = new CarsViewModel();
        }

        private void ShowContracts(object obj)
        {
            CurrentViewModel = new ContractsViewModel();
        }

        private void ShowParkingSpots(object obj)
        {
            CurrentViewModel = new ParkingSpotsViewModel();
        }

        private void ShowPayments(object obj)
        {
            CurrentViewModel = new PaymentsViewModel();
        }

        private void ShowTariffs(object obj)
        {
            CurrentViewModel = new TariffsViewModel();
        }

        private void ShowEntranceRecords(object obj)
        {
            CurrentViewModel = new EntranceRecordsViewModel();
        }

        private void ShowAdmissions(object obj)
        {
            CurrentViewModel = new AdmissionsViewModel();
        }
    }
}