using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bubo
{
    /// <summary>
    /// Interaction logic for GetNameUI.xaml
    /// custom dialog UI used to edit name
    /// </summary>
    public partial class GetTextUI : Window, INotifyPropertyChanged
    {
        public String NewName { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _title = "Get Name";
        public String MyTitle
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                NotifyPropertyChanged(nameof(MyTitle));
            }
        }

        private string _msg = "New name";
        public String Msg
        {
            get
            {
                return _msg;
            }
            set
            {
                _msg = value;
                NotifyPropertyChanged(nameof(Msg));
            }
        }

        private GetNameOptions _textOption = GetNameOptions.All;

        public static GetTextUI _defaultDialog;

        public static string OpenGetTextDialog(Window owner,Point pos, double width = 150, string title = null ,string msg = null, string defaultTxt = "", GetNameOptions opt = GetNameOptions.All  )
        {
            _defaultDialog = new GetTextUI(owner, pos, title, msg, defaultTxt, opt );
            _defaultDialog.WindowStartupLocation = WindowStartupLocation.Manual;
            _defaultDialog.Left = pos.X;
            _defaultDialog.Top = pos.Y;
            _defaultDialog.Width = width;
                
            if (_defaultDialog.ShowDialog() == true)
            {
                    
                return _defaultDialog.NewName;
            }
            return null;
        }
        public GetTextUI(Window owner, Point pos, string title = null, string msg = null, string defaultTxt = "", GetNameOptions opt = GetNameOptions.All )
        {
            DataContext = this;

            InitializeComponent();
            System.Windows.Interop.WindowInteropHelper windowHandle =
                new System.Windows.Interop.WindowInteropHelper(this)
                {
                    Owner = ManagedServices.AppSDK.GetMaxHWND()
                };
            ManagedServices.AppSDK.ConfigureWindowForMax(this);

            if (owner is Window win && win.IsVisible)
            {
                Owner = win;
            }

            if (title != null)
            {
                MyTitle = title;
            }
            if (msg != null)
            {
                Msg = msg;
            }
            if (defaultTxt != null)
            {
                TextTB.Text = defaultTxt;
                TextTB.CaretIndex = defaultTxt.Length;
                TextTB.SelectAll();
            }
            _textOption = opt;
            TextTB.Focus();
        }
        private void OnClickOK(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            NewName = TextTB.Text;
            Close();
        }
        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            OnClickOK(null, null);
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
        private void OnPreviewTextInputTextBox(object sender, TextCompositionEventArgs e)
        {
            if (_textOption == GetNameOptions.Numbers)
            {
                Regex regex = new Regex(@"[0-9]|.");
                e.Handled = !regex.IsMatch(e.Text);
            }                
        }
    }
}
