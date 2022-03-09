using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bubo
{
    /// <summary>
    /// used in DataProjection list items
    /// </summary>
    public class MapItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
            catch (Exception ex)
            {
                Tools.Print("TreeViewItemNotifyPropertyChangedException : " + ex.Message, DebugLevel.EXCEPTION);
            }
        }
        IINode Node { get; }
        int _channel;
        public int Channel
        {
            get
            {
                return _channel;
            }
        }
        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        bool _isItemSelected;
        public bool IsItemSelected
        {
            get
            {
                return _isItemSelected;
            }
            set
            {
                _isItemSelected = value;
                ShowVertexColor(value);
                NotifyPropertyChanged(nameof(IsItemSelected));
            }
        }
        public MapItem(string name , IINode node)
        {
            Name = name;
            Node = node;
        }
        public MapItem(string name, IINode node, int channel)
            : this(name, node)
        {

            _channel = channel;
        }
        public MapItem(string name, IINode node, int channel, bool isSelected)
            :this(name, node ,channel)
        {
            IsItemSelected = isSelected;
        }
        public void ShowVertexColor( bool onOff )
        {
            if(Node != null)
            {
                Node.VertexColorMapChannel = Channel;
                Node.VertexColorType = 5;
                MaxSDK.ToMaxScript(Node, "n");
                MaxSDK.ExecuteMxs(string.Format("ProjectionJob.DisplayVertexColor n {0}", onOff));
                MaxSDK.RedrawViews(Node);
            }
        }
    }
}
