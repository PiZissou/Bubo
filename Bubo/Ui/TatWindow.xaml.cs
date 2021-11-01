using Bubo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

// Based on https://www.source-weave.com/blog/custom-wpf-window
namespace TatWindow
{
    public partial class TatWindow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        bool _isMouseButtonDown;
        bool _isManualDrag;
        Point _mouseDownPos;
        Point _positionBeforeDrag;
        double _heightBeforeMaximize;
        double _widthBeforeMaximize;
        Point _previousScreenBounds;
        WindowState _previousState;
        private void OnClickMaximize(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Tools.VisualUpwardSearch<Window>((sender as DependencyObject)) is Window w)
                {
                    ToggleWindowState(w);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnClickRestore(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Tools.VisualUpwardSearch<Window>((sender as DependencyObject)) is Window w)
                {
                    ToggleWindowState(w);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnClickMinimize(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Tools.VisualUpwardSearch<Window>((sender as DependencyObject)) is Window w)
                {
                    w.WindowState = WindowState.Minimized;
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnClickClose(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Tools.VisualUpwardSearch<Window>((sender as DependencyObject)) is Window w)
                {
                    w.Close();
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void ToggleWindowState(Window w)
        {
            try
            {
                if (w.WindowState != WindowState.Maximized)
                {
                    w.WindowState = WindowState.Maximized;
                }
                else
                {
                    w.WindowState = WindowState.Normal;
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void RefreshWindowState(Window w)
        {
            try
            {
                if (w.WindowState == WindowState.Maximized)
                {
                    ToggleWindowState(w);
                    ToggleWindowState(w);
                }
            } catch(Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private Thickness GetDefaultMarginForDpi()
        {
            try
            {
                int currentDPI = SystemHelper.GetCurrentDPI();
                Thickness thickness = new Thickness(8, 8, 8, 8);
                if (currentDPI == 120)
                {
                    thickness = new Thickness(7, 7, 4, 5);
                }
                else if (currentDPI == 144)
                {
                    thickness = new Thickness(7, 7, 3, 1);
                }
                else if (currentDPI == 168)
                {
                    thickness = new Thickness(6, 6, 2, 0);
                }
                else if (currentDPI == 192)
                {
                    thickness = new Thickness(6, 6, 0, 0);
                }
                else if (currentDPI == 240)
                {
                    thickness = new Thickness(6, 6, 0, 0);
                }
                return thickness;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new Thickness(8, 8, 8, 8);
            }
        }
        private Thickness GetFromMinimizedMarginForDpi()
        {
            try
            {
                int currentDPI = SystemHelper.GetCurrentDPI();
                Thickness thickness = new Thickness(7, 7, 5, 7);
                if (currentDPI == 120)
                {
                    thickness = new Thickness(6, 6, 4, 6);
                }
                else if (currentDPI == 144)
                {
                    thickness = new Thickness(7, 7, 4, 4);
                }
                else if (currentDPI == 168)
                {
                    thickness = new Thickness(6, 6, 2, 2);
                }
                else if (currentDPI == 192)
                {
                    thickness = new Thickness(6, 6, 2, 2);
                }
                else if (currentDPI == 240)
                {
                    thickness = new Thickness(6, 6, 0, 0);
                }
                return thickness;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new Thickness(7, 7, 5, 7);
            }
        }
        private void OnMouseLeftButtonDownWindowHeaderBar(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!_isManualDrag && Tools.VisualUpwardSearch<Window>((sender as DependencyObject)) is Window w)
                {
                    Point position = e.GetPosition(w);

                    if (e.ClickCount == 2 && w.ResizeMode == ResizeMode.CanResize)
                    {
                        ToggleWindowState(w);
                        return;
                    }

                    if (w.WindowState == WindowState.Maximized)
                    {
                        _isMouseButtonDown = true;
                        _mouseDownPos = position;
                    }
                    else
                    {
                        _positionBeforeDrag = new Point(w.Left, w.Top);
                        w.DragMove();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnMouseMoveWindowHeaderBar(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (_isMouseButtonDown && Tools.VisualUpwardSearch<Window>((sender as DependencyObject)) is Window w)
                {
                    double currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
                    Point position = e.GetPosition(w);
                    Point screen = w.PointToScreen(position);
                    double x = _mouseDownPos.X - position.X;
                    double y = _mouseDownPos.Y - position.Y;
                    if (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) > 1)
                    {
                        double actualWidth = _mouseDownPos.X;

                        if (_mouseDownPos.X <= 0)
                        {
                            actualWidth = 0;
                        }
                        else if (_mouseDownPos.X >= w.ActualWidth)
                        {
                            actualWidth = _widthBeforeMaximize;
                        }

                        if (w.WindowState == WindowState.Maximized)
                        {
                            ToggleWindowState(w);
                            w.Top = (screen.Y - position.Y) / currentDPIScaleFactor;
                            w.Left = (screen.X - actualWidth) / currentDPIScaleFactor;
                            w.CaptureMouse();
                        }

                        _isManualDrag = true;

                        w.Top = (screen.Y - _mouseDownPos.Y) / currentDPIScaleFactor;
                        w.Left = (screen.X - actualWidth) / currentDPIScaleFactor;
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnMouseUpWindow(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Tools.VisualUpwardSearch<Window>((sender as DependencyObject)) is Window w)
                {
                    _isMouseButtonDown = false;
                    _isManualDrag = false;
                    w.ReleaseMouseCapture();
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Window w)
                {
                    w.StateChanged += new EventHandler(OnWindowStateChanged);

                    double currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
                    Screen screen = Screen.FromHandle((new WindowInteropHelper(w)).Handle);
                    System.Drawing.Rectangle workingArea = screen.WorkingArea;
                    w.MaxHeight = (double)(workingArea.Height + 16) / currentDPIScaleFactor;

                    double width = (double)screen.WorkingArea.Width;
                    _previousScreenBounds = new Point(width, (double)workingArea.Height);

                    if (GetLayoutRoot(w) is Grid g)
                    {
                        g.Margin = GetDefaultMarginForDpi();
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (sender is Window w)
                {
                    if (w.WindowState == WindowState.Normal)
                    {
                        _heightBeforeMaximize = w.ActualHeight;
                        _widthBeforeMaximize = w.ActualWidth;
                        return;
                    }

                    if (w.WindowState == WindowState.Maximized)
                    {
                        Screen screen = Screen.FromHandle((new WindowInteropHelper(w)).Handle);
                        if (_previousScreenBounds.X != screen.WorkingArea.Width || _previousScreenBounds.Y != screen.WorkingArea.Height)
                        {
                            double width = (double)screen.WorkingArea.Width;
                            System.Drawing.Rectangle workingArea = screen.WorkingArea;
                            _previousScreenBounds = new Point(width, (double)workingArea.Height);
                            RefreshWindowState(w);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        private Grid GetLayoutRoot(Window w)
        {
            try
            {
                if (w != null && VisualTreeHelper.GetChildrenCount(w) > 0) 
                {
                   DependencyObject firstChild = VisualTreeHelper.GetChild(w, 0);
                    if (VisualTreeHelper.GetChildrenCount(firstChild) > 0 && VisualTreeHelper.GetChild(firstChild, 0) is Grid g  && g.Name == "LayoutRoot")
                    {
                        return g;
                    }
                }
                return null;
            } catch(Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            } 
        }
        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender is Window w)
                {
                    Screen screen = Screen.FromHandle((new WindowInteropHelper(w)).Handle);
                    Thickness thickness = new Thickness(0);
                    if (w.WindowState != WindowState.Maximized)
                    {
                        double currentDPIScaleFactor = SystemHelper.GetCurrentDPIScaleFactor();
                        System.Drawing.Rectangle workingArea = screen.WorkingArea;
                        w.MaxHeight = (workingArea.Height + 16) / currentDPIScaleFactor;
                        w.MaxWidth = double.PositiveInfinity;
                    }
                    else
                    {

                        thickness = this.GetDefaultMarginForDpi();
                        if (_previousState == WindowState.Minimized || w.Left == _positionBeforeDrag.X && w.Top == _positionBeforeDrag.Y)
                        {
                            thickness = this.GetFromMinimizedMarginForDpi();
                        }
                    }

                    if (GetLayoutRoot(w) is Grid g)
                    {
                        g.Margin = thickness;
                    }
                    _previousState = w.WindowState;
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void NotifyPropertyChanged(String propName)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }

    }
}
