using System;
using System.Windows.Data;

namespace HudOffsetFixer.Converters
{
    public class HexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ExplConvert(value, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ExplConvert(value, targetType);
        }

        private object ExplConvert(object value, Type targetType)
        {
            if (value is long lVal)
            {
                return lVal.ToString("X");

                //long output = 0;
                //try
                //{
                //    output = System.Convert.ToInt64(value.ToString(), 16);
                //}
                //catch
                //{
                //    // MessageBox.Show("Can't convert " + value + " to long: " + ex.Message);
                //}
                //return output;
            }

            if (value is string str)
            {
                return str;
            }
            else
            {
                long longVal = System.Convert.ToInt64(value);
                return longVal.ToString("X");
            }
        }
    }
}
