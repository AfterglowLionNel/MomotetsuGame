using System;
using System.Globalization;
using System.Windows.Data;
using MomotetsuGame.Core.Enums;

namespace MomotetsuGame.Converters
{
    /// <summary>
    /// PropertyCategoryをアイコン文字に変換するコンバーター
    /// </summary>
    public class CategoryToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string categoryString && Enum.TryParse<PropertyCategory>(categoryString, out var category))
            {
                return category switch
                {
                    PropertyCategory.Agriculture => "🌾",
                    PropertyCategory.Fishery => "🐟",
                    PropertyCategory.Commerce => "🏪",
                    PropertyCategory.Industry => "🏭",
                    PropertyCategory.Tourism => "🏖️",
                    _ => "🏢"
                };
            }

            return "🏢";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}