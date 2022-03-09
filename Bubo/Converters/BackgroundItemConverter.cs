using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Bubo
{
    /// <summary>
    /// object type converted to soliColorBrush
    /// </summary>
    public class BackgroundItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LayerItem layerItem)
            {
                if  (layerItem.IsItemSelected )
                {
                    return SystemColors.HighlightBrushKey;
                }
                else
                {
                    return BuboUI.Instance.FindResource("layerItemBrush");
                }
                    
            }
            if (value is MaxItem maxItem )
            {
                if (maxItem.IsItemSelected)
                {
                    return SystemColors.HighlightBrushKey;
                }
                else if (maxItem.IsPair)
                {
                    return BuboUI.Instance.FindResource("layerItemBrush");
                }
                else
                {
                    return BuboUI.Instance.FindResource("MaxItemPairBrush");
                }
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Tools.Format(MethodBase.GetCurrentMethod(), "");
            throw new NotImplementedException();
        }
    }
}
