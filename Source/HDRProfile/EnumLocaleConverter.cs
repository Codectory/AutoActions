using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HDRProfile
{
    [ValueConversion(typeof(Enum), typeof(string))]

    public class EnumLocaleConverter : IValueConverter
    {
        private ResourceManager ResourceManager => Locale_Enums.ResourceManager;

         
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ResourceManager.GetString($"{value.GetType().Name}.{value}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
