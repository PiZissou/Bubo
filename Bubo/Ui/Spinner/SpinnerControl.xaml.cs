using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bubo
{
    /// <summary>
    /// Interaction logic for SpinnerControl.xaml
    /// Ui Control 
    /// - manipulate double entry value
    /// - value can be set by text entry or by buttons increment / decrement
    /// </summary>
    /// 
    public partial class SpinnerControl : UserControl, INotifyPropertyChanged
    {
        private bool _isDragging;
        private Point _mousePos;

        public event PropertyChangedEventHandler PropertyChanged;

        public Double SpinnerValue
        {
            get
            {
                return Math.Round((Double)GetValue(SpinnerValueProperty),3);
            }
            set
            {
                double oldValue = (Double)GetValue(SpinnerValueProperty);
                value = Math.Round(value, 3);
                SetValue(SpinnerValueProperty, value);
                if (ValueChanged != null)
                {
                    ValueChanged(this, new ValueChangedArg(oldValue, value));
                }
                NotifyPropertyChanged(nameof(SpinnerValue));
            }            
        }
        public static readonly DependencyProperty SpinnerValueProperty = DependencyProperty.Register("SpinnerValue", typeof(double), typeof(SpinnerControl), new PropertyMetadata((double)100));

        public Double Scale
        {
            get
            {
                return (Double)GetValue(ScaleProperty);
            }
            set
            {
                SetValue(ScaleProperty, value);
                NotifyPropertyChanged(nameof(Scale));
            }
        }
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register("Scale", typeof(double), typeof(SpinnerControl), new PropertyMetadata((double)1));

        public Double Minimum
        {
            get
            {
                return (Double)GetValue(MinimumProperty);
            }
            set
            {
                SetValue(MinimumProperty, value);
                NotifyPropertyChanged(nameof(Minimum));
            }
        }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(SpinnerControl), new PropertyMetadata((double) 0));

        public Double Maximum
        {
            get
            {
                return (Double)GetValue(MaximumProperty);
            }
            set
            {
                SetValue(MaximumProperty, value);
                NotifyPropertyChanged(nameof(Maximum));
            }
        }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(SpinnerControl), new PropertyMetadata((double) 100 ));

        public Double DefaultValue
        {
            get
            {
                return (Double)GetValue(DefaultValueProperty);
            }
            set
            {
                
                SetValue(DefaultValueProperty,value);
                NotifyPropertyChanged(nameof(DefaultValue));
            }
        }
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(double), typeof(SpinnerControl), new PropertyMetadata((double)0));

        public bool IsAnimated
        {
            get
            {
                return (bool)GetValue(IsAnimatedProperty);
            }
            set
            {

                SetValue(IsAnimatedProperty, value);
                NotifyPropertyChanged(nameof(IsAnimated));
            }
        }
        public static readonly DependencyProperty IsAnimatedProperty = DependencyProperty.Register("IsAnimated", typeof(bool), typeof(SpinnerControl), new PropertyMetadata(false));

        public bool IsArrow
        {
            get
            {
                return (bool)GetValue(IsArrowProperty);
            }
            set
            {

                SetValue(IsArrowProperty, value);
                NotifyPropertyChanged(nameof(IsArrow));
            }
        }
        public static readonly DependencyProperty IsArrowProperty = DependencyProperty.Register("IsArrow", typeof(bool), typeof(SpinnerControl), new PropertyMetadata(true));

        public bool IsBlendColor
        {
            get
            {
                return (bool)GetValue(IsBlendColorProterty);
            }
            set
            {

                SetValue(IsBlendColorProterty, value);
                NotifyPropertyChanged(nameof(IsBlendColor));
            }
        }
        public static readonly DependencyProperty IsBlendColorProterty = DependencyProperty.Register("IsBlendColor", typeof(bool), typeof(SpinnerControl), new PropertyMetadata(false));

        public SolidColorBrush BlendColor
        {
            get
            {
                return (SolidColorBrush)GetValue(BlendColorProterty);
            }
            set
            {

                SetValue(BlendColorProterty, value);
                NotifyPropertyChanged(nameof(BlendColor));
            }
        }
        public static readonly DependencyProperty BlendColorProterty = DependencyProperty.Register("BlendColor", typeof(SolidColorBrush), typeof(SpinnerControl), new PropertyMetadata( Brushes.MediumTurquoise));//LightGreen

        public SolidColorBrush BlendColorScript
        {
            get
            {
                return (SolidColorBrush)GetValue(BlendColorScriptProterty);
            }
            set
            {

                SetValue(BlendColorScriptProterty, value);
                NotifyPropertyChanged(nameof(BlendColorScript));
            }
        }
        public static readonly DependencyProperty BlendColorScriptProterty = DependencyProperty.Register("BlendColorScript", typeof(SolidColorBrush), typeof(SpinnerControl), new PropertyMetadata(Brushes.Tomato));//Tomato  OrangeRed
        public int FormatValue
        {
            get
            {
                return (int)GetValue(FormatValueProperty);
            }
            set
            {

                SetValue(FormatValueProperty, value);
                NotifyPropertyChanged(nameof(FormatValue));
            }
        }
        public static readonly DependencyProperty FormatValueProperty = DependencyProperty.Register("FormatValue", typeof(int), typeof(SpinnerControl), new PropertyMetadata(2));

        public delegate void ValueChangedEventHandler(object sender, ValueChangedArg e);
        public event ValueChangedEventHandler ValueChanged;

        public SpinnerControl()
        {
            try
            {
                InitializeComponent();
                
            } catch(Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(),ex);
            }
        }

        public void NotifyPropertyChanged(String propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private void OnPreviewMouseDownPlus(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                double newValue = SpinnerValue + Scale;
                if (newValue <= Maximum)
                {
                    SpinnerValue = newValue;
                }
                else if (SpinnerValue != Maximum)
                {
                    SpinnerValue = Maximum;
                }
                InitDragging(e);
                Cursor = Cursors.SizeNS;
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                SpinnerValue = DefaultValue;
            }
        }

        private void InitDragging(MouseButtonEventArgs e)
        {
            _mousePos = e.GetPosition(mgrid);
            _isDragging = true;
            mgrid.CaptureMouse();
            Cursor = Cursors.SizeNS;
        }

        private void StopDragging()
        {
            _isDragging = false;
            mgrid.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
        }

        private void OnPreviewMouseDownMinus(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                double newValue = SpinnerValue - Scale;
                if (newValue >= Minimum)
                {
                    SpinnerValue = newValue;
                }
                else if (SpinnerValue != Minimum)
                {
                    SpinnerValue = Minimum;
                }
                InitDragging(e);
                Cursor = Cursors.SizeNS;
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                SpinnerValue = DefaultValue;
            }
        }

        private void OnMouseDownGrid(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _mousePos = e.GetPosition((sender as UIElement));
                InitDragging(e);
            }

            else if (e.ChangedButton == MouseButton.Right)
            {
                SpinnerValue = DefaultValue;
            }
            e.Handled = true;
        }

        private void OnPreviewMouseUpGrid(object sender, MouseButtonEventArgs e)
        {
            StopDragging();                
        }

        private void OnPreviewMouseMoveGrid(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point newMousePos = e.GetPosition(sender as UIElement);
                double delta = _mousePos.Y - newMousePos.Y;
                delta *= Scale;
                _mousePos = newMousePos;
                double newValue = SpinnerValue + delta;
                    
                if ((newValue >= Minimum || newValue < Minimum && Minimum==-9999) && (newValue <= Maximum || newValue > Maximum && Maximum == 9999))
                {
                    SpinnerValue = newValue;
                }
                else if (newValue > Maximum && SpinnerValue != Maximum)
                {
                    SpinnerValue = Maximum;
                }
                else if (newValue < Minimum && SpinnerValue != Minimum)
                {
                    SpinnerValue = Minimum;
                }
            }
            e.Handled = true;
        }

        private void OnDoubleClickValue(object sender, MouseButtonEventArgs e)
        {
            if ( sender is TextBox tb)
            {
                ManagedServices.AppSDK.DisableAccelerators();
                tb.IsReadOnly = false;
                tb.SelectAll();
                tb.Focus();
                tb.Cursor = Cursors.IBeam;
            }
        }
        private void OnKeyUpValueBlock(object sender, KeyEventArgs e)
        {
            Tools.Format(MethodBase.GetCurrentMethod(),e.Key.ToString()) ;
            if (e.Key == Key.Enter && sender is TextBox tb)
            {
                Keyboard.ClearFocus();
            }
        }
        private void OnKeyboardLostFocusValueBlock(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.Cursor = Cursors.Arrow;
                tb.SelectionLength = 0;
                tb.IsReadOnly = true;

                if (float.TryParse(tb.Text, out float newValue))
                {
                    if ((newValue >= Minimum || newValue < Minimum && Minimum == -9999) && (newValue <= Maximum || newValue > Maximum && Maximum == 9999))
                    {
                        SpinnerValue = newValue;
                    }
                    else if (newValue > Maximum && SpinnerValue != Maximum)
                    {
                        SpinnerValue = Maximum;
                    }
                    else if (newValue < Minimum && SpinnerValue != Minimum)
                    {
                        SpinnerValue = Minimum;
                    }
                }
            }
            ManagedServices.AppSDK.EnableAccelerators();
        }

    }
}
