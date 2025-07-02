using System;
using System.Globalization;
using System.Windows.Data;

namespace MomotetsuGame.Converters
{
    /// <summary>
    /// int�l��bool�l�ɕϊ�����R���o�[�^�[�i���W�I�{�^���p�j
    /// </summary>
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter is string paramString && int.TryParse(paramString, out int paramValue))
            {
                return intValue == paramValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter is string paramString && int.TryParse(paramString, out int paramValue))
            {
                return paramValue;
            }
            return Binding.DoNothing;
        }
    }
}