using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Bubo
{
    public class VisibleItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ( values[0] is MaxItem maxItem)
                {
                    string name = maxItem.Name;

                    if ( values[1] is bool only && only )
                    {
                        if (maxItem is MorphItem morphItem && !morphItem.IsActive)
                            return maxItem.IsVisible = false;
                        if (maxItem is SkinItem skinItem && skinItem.IsHold)
                            return maxItem.IsVisible = false;
                    }
                    if (values[2] is bool showR && maxItem.IsRight)
                    {
                        return maxItem.IsVisible = showR;
                    }
                    else if(values[3] is bool showL && maxItem.IsLeft)
                    {
                        return maxItem.IsVisible = showL;
                    }
                    else if (values[4] is bool showM )
                    {
                        return maxItem.IsVisible = showM;
                    }
                    else
                    {
                        return maxItem.IsVisible = true;
                    }
                }
                else if (values[0] is LayerItem layer )
                {
                    return layer.IsVisible = layer.HasChildrenVisible;
                }
                else if (values[0] is TreeItem item )
                {
                    return item.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
            return true;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
