using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bubo
{
    public class WeightItem : INotifyPropertyChanged
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
                NotifyPropertyChanged(nameof(IsItemSelected));
            }
        }
        public int Vtx { get; set; }
        public IINode Bone { get; set; }
        public int BoneId { get; set; }
        public int BoneWeightId { get; set; }
        public float Weight { get; set; }
        public float DualQ { get; set; }
        public MaxItem LinkedItem { get; set; }

    }
}
