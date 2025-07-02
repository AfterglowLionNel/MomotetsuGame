using System;
using System.Globalization;
using System.Windows.Data;

namespace MomotetsuGame.Converters
{
    /// <summary>
    /// Enum値をbool値に変換するコンバーター（ラジオボタン用）
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return parameter;
            }
            return Binding.DoNothing;
        }
    }
}