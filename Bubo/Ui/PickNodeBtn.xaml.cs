using Autodesk.Max;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Bubo
{
    public partial class PickNodeBtn : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string NodeName
        {
            get
            {
                return (string)GetValue(NodeNameProperty);
            }
            set
            {
                SetValue(NodeNameProperty, value);
                NotifyPropertyChanged(nameof(NodeName));
            }
        }
        public static readonly DependencyProperty NodeNameProperty = DependencyProperty.Register("NodeName", typeof(string), typeof(PickNodeBtn), new PropertyMetadata((string)"Pick Node"));

        bool _isPickingNode;
        public bool IsPickingNode
        {
            get
            {
                return _isPickingNode;
            }
            set
            {
                _isPickingNode = value;
                NotifyPropertyChanged(nameof(IsPickingNode));
            }
        }

        public event PickNodeEventHandler OnNodePicked;

        public PickNodeBtn()
        {
            try
            {
                DataContext = this;
                InitializeComponent();
            } catch(Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void OnClickBtn(object sender, RoutedEventArgs e)
        {
            try
            {
                if ( !IsPickingNode )
                {
                    IsPickingNode = true;
                    if (MaxSDK.PickNode() is IINode n)
                    {
                        OnNodePicked?.Invoke(this, new PickNodeArgs(n));
                    }
                    IsPickingNode = false;
                }
            } catch(Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public void NotifyPropertyChanged(string propName)
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

    public class PickNodeArgs : EventArgs
    {
        public IINode Node { get; private set; }

        public PickNodeArgs(IINode n)
        {
            Node = n;
        }
    }

    public delegate void PickNodeEventHandler(object sender, PickNodeArgs e);
}
