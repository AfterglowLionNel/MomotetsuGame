using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MomotetsuGame.Converters
{
    /// <summary>
    /// Booleanを反転してVisibilityに変換するコンバーター
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// サイコロの値を●表示に変換するコンバーター
    /// </summary>
    public class DiceValueToDotsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int diceValue)
            {
                return diceValue switch
                {
                    1 => "⚀",
                    2 => "⚁",
                    3 => "⚂",
                    4 => "⚃",
                    5 => "⚄",
                    6 => "⚅",
                    _ => "?"
                };
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}