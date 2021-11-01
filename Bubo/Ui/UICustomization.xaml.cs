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

// https://www.source-weave.com/blog/custom-wpf-window

namespace Bubo
{
    public partial class UICustomization
    {
        private void OnMouseDownExpanderHeader(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (Tools.VisualUpwardSearch<Expander>((sender as DependencyObject)) is Expander exp)
                {
                    exp.IsExpanded = !exp.IsExpanded;
                }
            } catch(Exception ex)
            {
                Tools.Print("UICustomizationOnMouseDownExpanderHeaderException : " + ex.Message, DebugLevel.EXCEPTION);
            }
        }

        protected void OnPreviewMouseDownExpander(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!(e.Source is ComboBox))
                {
                    Keyboard.ClearFocus();
                    (sender as UIElement).Focus();
                }
            }
            catch (Exception ex)
            {
                Tools.Print("UICustomizationOnPreviewMouseDownExpanderException : " + ex.Message, DebugLevel.EXCEPTION);
            }
        }

        private void OnPreviewMouseDownTabHeader(object sender, MouseButtonEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Tools.Print("UICustomizationOnPreviewMouseDownTabHeaderException : " + ex.Message, DebugLevel.EXCEPTION);
            }
        }

        private void OnMouseDownListBox(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is ListBox lb && !(e.Source is ListBoxItem))
                {
                    lb.UnselectAll();
                }
            } catch(Exception ex)
            {
                Tools.Print("UICustomizationOnMouseDownListBoxException : " + ex.Message, DebugLevel.EXCEPTION);
            }
        }

        private void NumberTextBoxOnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
            catch (Exception ex)
            {
                Tools.Print("UICustomizationOnMouseDownListBoxException : " + ex.Message, DebugLevel.EXCEPTION);
            }
        }
    }
}
