using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Bubo
{
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        private IntPtr _hwnd;
        public WindowWrapper(IntPtr handle)
        {
            _hwnd = handle;
        }

        public WindowWrapper(Window window)
        {
            _hwnd = new WindowInteropHelper(window).Handle;
        }

        public IntPtr Handle
        {
            get { return _hwnd; }
        }
        
    }
}
