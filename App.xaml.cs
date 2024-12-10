using ParkingInterface.Data;
using System;
using System.Windows;

namespace ParkingInterface
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                DataAccess.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при ініціалізації бази даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}