using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Bubo
{
    /// <summary>
    /// inherit from TreeItem
    /// base class for SkinItem, MorphItem
    /// </summary>
    public class MaxItem : TreeItem
    {
        public IBuboMod BuboMod { get; }
        public virtual bool IsRight
        {
            get
            {
                return Name.Contains("_D_");
            }
        }
        public virtual bool IsLeft
        {
            get
            {
                return Name.Contains("_G_");
            }
        }
        public string ParentPattern { get; set; }
        protected int _maxIndex;
        public int MaxIndex
        {
            get
            {
                return _maxIndex;
            }
        }
        public MaxItem(string name, IBuboMod mod)
            : base(name)
        {
            BuboMod = mod;
        }
        public virtual void Refresh()
        {
            
        }
    }
}
