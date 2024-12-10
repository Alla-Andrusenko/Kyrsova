using ParkingInterface.Data;
using ParkingInterface.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ParkingInterface.ViewModels
{
    public class EntranceRecordsViewModel : ViewModelBase
    {
        public ObservableCollection<EntranceRecord> EntranceRecords { get; set; }
        public ObservableCollection<Car> Cars { get; set; } // Список автомобілів для ComboBox

        private EntranceRecord newRecord;
        public EntranceRecord NewRecord
        {
            get { return newRecord; }
            set { newRecord = value; OnPropertyChanged(); }
        }

        private EntranceRecord selectedRecord;
        public EntranceRecord SelectedRecord
        {
            get { return selectedRecord; }
            set
            {
                selectedRecord = value;
                OnPropertyChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public EntranceRecordsViewModel()
        {
            LoadData();
            NewRecord = new EntranceRecord
            {
                EntryTime = DateTime.Now
            };

            AddCommand = new RelayCommand(AddRecord);
            DeleteCommand = new RelayCommand(DeleteRecord, CanDeleteRecord);
        }

        private void LoadData()
        {
            EntranceRecords = new ObservableCollection<EntranceRecord>(DataAccess.GetEntranceRecords());
            Cars = new ObservableCollection<Car>(DataAccess.GetCars());
            OnPropertyChanged(nameof(EntranceRecords));
            OnPropertyChanged(nameof(Cars));
        }

        private void AddRecord(object obj)
        {
            if (!IsValid(NewRecord))
                return;

            DataAccess.InsertEntranceRecord(NewRecord);
            LoadData();
            NewRecord = new EntranceRecord
            {
                EntryTime = DateTime.Now
            };
        }

        private void DeleteRecord(object obj)
        {
            if (MessageBox.Show("Ви впевнені, що хочете видалити цей запис?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                DataAccess.DeleteEntranceRecord(SelectedRecord.RecordId);
                LoadData();
                SelectedRecord = null;
            }
        }

        private bool CanDeleteRecord(object obj)
        {
            return SelectedRecord != null;
        }

        private bool IsValid(EntranceRecord record)
        {
            if (record.CarId == 0)
            {
                MessageBox.Show("Виберіть автомобіль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (record.EntryTime == DateTime.MinValue)
            {
                MessageBox.Show("Вкажіть час в'їзду.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }
    }
}