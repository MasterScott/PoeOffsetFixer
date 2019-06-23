using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HudOffsetFixer.Converters
{
    public class OffsetToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var offsets = (List<int>)value;

            if(offsets.Count == 0 )
                return  new SolidColorBrush(Colors.IndianRed);
            if(offsets.Count > 1)
                return new SolidColorBrush(Colors.Yellow);

            return new SolidColorBrush(Colors.LightGreen);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
