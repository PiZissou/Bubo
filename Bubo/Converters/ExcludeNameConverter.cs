using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Bubo
{
    public class ExcludeNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (values.Count() == 7 && values[0] is MaxItem maxItem)
                {
                    string outName = maxItem.Name;
                    if ( values[1] is bool onOffBaseName)
                    {
                        outName = ExcludeBaseName(outName, onOffBaseName);
                    }
                    if ( values[2] is bool onOffPattern && values[3] is IEnumerable<string> excludePatterns)
                    {
                        outName = ExcludeFromCollection(outName, excludePatterns, onOffPattern);
                    }
                    if ( values[4] is bool onOffEndName && values[5] is IEnumerable<string> excludeEnds)
                    {
                        outName = ExcludeFromCollection(outName, excludeEnds, onOffEndName);
                    }
                    return outName;
                }
                else if (values.Count() > 0 && values[0] is TreeItem layer)
                {
                    return layer.Name;
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
            return "Bubo!";
        }
        public string ExcludePattern(string name, string pattern, bool onOff)
        {
            if (onOff && pattern != null)
            {
                return Regex.Replace( name ,pattern , "" , RegexOptions.IgnoreCase);
            }
            return name;
        }
        public string ExcludeFromCollection(string name, IEnumerable<string> excludes , bool onOff)
        {
            if (onOff)
            {
                foreach ( string pattern in excludes)
                {
                    if(Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase))
                    {
                        return Regex.Replace(name , pattern, "", RegexOptions.IgnoreCase);
                    } 
                }
                return name;
            }
            return name;
        }
        public string ExcludeBaseName(string name, bool onOff)
        {
            if (onOff && Main.CurrentEngine != null  && Main.CurrentEngine.BaseName !="")
            {
                string pattern = Main.CurrentEngine.BaseName;
                if (Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase))
                {
                    return Regex.Replace(name, pattern + "_" , "", RegexOptions.IgnoreCase) ;
                }
            }
            return name;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
