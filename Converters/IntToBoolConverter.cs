/*using System;
using System.Globalization;
using System.Windows.Data;

namespace ParkingInterface.Converters
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = false;
            if (parameter != null && bool.TryParse(parameter.ToString(), out bool parsedInvert))
            {
                invert = parsedInvert;
            }

            if (value is int intValue)
            {
                bool result = intValue == 0;
                return invert ? !result : result;
            }
            return false; //або інше значення за замовчуванням, якщо value не int
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}*/