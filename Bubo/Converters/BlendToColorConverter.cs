using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media;

namespace Bubo
{
    /// <summary>
    /// multiDataBinding converted to soliColorBrush
    /// values = {"IsBlendColor",
    ///           "IsEnabled",
    ///           "BlendColor",
    ///           "BlendColorScript",
    ///           "SpinnerValue",
    ///           "Maximum",
    ///           }
    /// </summary>
    public class BlendToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (values[0] is bool isBlendColor && isBlendColor && values[1] is bool isEnabled )
            {
                if (values[2] is SolidColorBrush brush && brush.Color is Color blendColor && values[3] is SolidColorBrush brushScript && brushScript.Color is Color blendColorScript)
                {
                    if (values[4] is double spinnerValue && values[5] is double maximumValue)
                    {
                        spinnerValue = (spinnerValue < 0) ? 0 : (spinnerValue > maximumValue) ? maximumValue : spinnerValue;
                        Color col = (isEnabled) ? blendColor : blendColorScript;
                        byte alpha = System.Convert.ToByte(spinnerValue / maximumValue * 255);
                        return new SolidColorBrush(Color.FromArgb(alpha, col.R, col.G, col.B));
                    }
                }
            }
      
            return new SolidColorBrush();
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
