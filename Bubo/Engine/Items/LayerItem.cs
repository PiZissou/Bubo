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
    ///  - all MaxItem thad match Patterns will be added as child
    /// </summary>
    public class LayerItem : TreeItem
    {
        public List<string> Patterns { get; } = new List<string>();
        public LayerItem(string name)
            : base(name)
        {

        }
        public LayerItem (string name , TreeItem parent)
            : base(name, parent) 
        {
            
        }
    }
}
