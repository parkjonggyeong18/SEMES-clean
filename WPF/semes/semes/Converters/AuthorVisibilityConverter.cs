using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace semes.Converters
{
    public class AuthorVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string author && values[1] is string currentUser)
            {
                return author == currentUser ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
