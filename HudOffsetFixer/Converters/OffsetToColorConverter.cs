using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HudOffsetFixer.Core;

namespace HudOffsetFixer.Converters
{
    public class OffsetToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var searchStatus = (OffsetSearchStatus) value;

            switch (searchStatus)
            {
                case OffsetSearchStatus.Unknown:
                    return new SolidColorBrush(Colors.White);
                case OffsetSearchStatus.NotFound:
                    return new SolidColorBrush(Colors.PaleVioletRed);
                case OffsetSearchStatus.FoundSingle:
                    return new SolidColorBrush(Colors.LightGreen);
                case OffsetSearchStatus.FoundMultiple:
                    return new SolidColorBrush(Colors.Yellow);
                default:
                    throw new ArgumentOutOfRangeException();
            }      
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
