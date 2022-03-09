using Autodesk.Max;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Bubo
{
    /// <summary>
    /// Interaction logic for PickNodeBtn.xaml
    /// Creat Pick button control ( like in 3dsmax pickbutton)
    /// - on click =  pick mode enabled to select node in 3dsmax viewport.
    /// - if object in clicked,  node is saved and pick mode is desabled
    /// - if cancel or pick is empty, pick mode is desabled
    /// </summary>
    /// 
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
            DataContext = this;
            InitializeComponent();
        }

        private void OnClickBtn(object sender, RoutedEventArgs e)
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
        }

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
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
