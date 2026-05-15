using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TechInventory.Helpers
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            if (status == "Работает")
                return new SolidColorBrush(Colors.Green);
            else if (status == "Сломан")
                return new SolidColorBrush(Colors.Red);
            else
                return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}