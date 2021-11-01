using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Bubo
{
    public class CustToggleButton : ToggleButton
    {
        public Brush ToggledColor
        {
            get
            {
                return (Brush)GetValue(ToggledColorProperty);
            }
            set
            {
                SetValue(ToggledColorProperty, value);
            }
        }
        public static readonly DependencyProperty ToggledColorProperty = DependencyProperty.Register("ToggledColor", typeof(Brush), typeof(CustToggleButton), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public Brush ToggledTextColor
        {
            get
            {
                return (Brush)GetValue(ToggledTextColorProperty);
            }
            set
            {
                SetValue(ToggledTextColorProperty, value);
            }
        }
        public static readonly DependencyProperty ToggledTextColorProperty = DependencyProperty.Register("ToggledTextColor", typeof(Brush), typeof(CustToggleButton), new PropertyMetadata(new SolidColorBrush(Colors.LightGray)));

    }
}
