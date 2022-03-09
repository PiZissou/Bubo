using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace Bubo
{
    public interface IBuboMod
    {
        /// <summary>
        /// interface for bubo modifier
        /// used in morphEngine and SkinEngin
        /// </summary>
        IModifier Modifier { get; }
        IINode Node { get; }
        string MxsModifier { get; }
        string MxsNode { get; }
        int Count { get; }
        int Selected { get; }
        ListTree<MaxItem> MaxItems { get; }
        IEnumerable<MaxItem> MaxItemSel { get; }
        void SelectMaxItem(MaxItem maxItem);
        void RedrawViews();
        void RedrawUI(RedrawUIOption redraw);
    }
}
