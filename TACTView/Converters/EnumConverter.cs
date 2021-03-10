using System;
using System.Globalization;
using System.Windows.Data;

namespace TACTView.Converters {
    public class EnumConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (targetType.IsEnum) return Enum.Format(targetType, value, "F");
            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Enum.Parse(targetType, value.ToString() ?? string.Empty, true);
        }
    }
}
