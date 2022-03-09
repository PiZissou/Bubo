using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;


namespace Bubo
{

    /// <summary>
    ///  Interaction logic for UICustomization.xaml
    /// </summary>
    public partial class UICustomization
    {
        private void OnMouseDownExpanderHeader(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            if (Tools.VisualUpwardSearch<Expander>((sender as DependencyObject)) is Expander exp)
            {
                exp.IsExpanded = !exp.IsExpanded;
            }

        }

        protected void OnPreviewMouseDownExpander(object sender, MouseButtonEventArgs e)
        {

            if (!(e.Source is ComboBox))
            {
                Keyboard.ClearFocus();
                (sender as UIElement).Focus();
            }
        }

        private void OnPreviewMouseDownTabHeader(object sender, MouseButtonEventArgs e)
        {
            if (Tools.VisualUpwardSearch<TabItem>((sender as DependencyObject)) is TabItem tab && Tools.VisualUpwardSearch<Expander>((sender as DependencyObject)) is Expander exp)
            {
                if (tab.IsSelected)
                {
                    exp.IsExpanded = !exp.IsExpanded;
                }
                else
                {
                    exp.IsExpanded = true;
                }
            }
        }

        private void OnMouseDownListBox(object sender, MouseButtonEventArgs e)
        {

            if (sender is ListBox lb && !(e.Source is ListBoxItem))
            {
                lb.UnselectAll();
            }
        }

        private void NumberTextBoxOnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
            
        }
    }
}
