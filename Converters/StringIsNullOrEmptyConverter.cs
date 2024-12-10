/*using System;
using System.Globalization;
using System.Windows.Data;

namespace ParkingInterface.Converters
{
    public class StringIsNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = false;
            if (parameter != null && bool.TryParse(parameter.ToString(), out bool parsedInvert))
            {
                invert = parsedInvert;
            }

            bool result = string.IsNullOrEmpty(value as string);
            return invert ? !result : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
*/