using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UiViewModels.Actions;
using System.Windows.Controls;
using System.Timers;
using System.IO;

namespace Bubo
{
    /// <summary>
    /// used to dock buboUI userControl in 3dsmax as floating window, or docking left, docking right. 
    /// </summary>
    public class CUI
    {
        public static ActionDock Dockable { get; set; }
    }

    public class ActionDock : CuiDockableContentAdapter
    {
        public override string ActionText { get { return "BuboCUI"; } }
        public override string Category { get { return "tat_Cui"; } }
        public override string ObjectName { get { return "ActionDock"; } }
        public override string WindowTitle
        {
            get
            {
                return "Bubo";
            }
        }
        public object Content { get; set; }
        public string ExecuteOnOpen { get; set; } = "BuboAPI.UI.OnOpen()";

        public ActionDock()
            : base()
        {
            CUI.Dockable = this;
        }
        public override Type ContentType
        {
            get
            {
                if (Content != null)
                {
                    return Content.GetType();
                }
                return typeof(UserControl);
            }
        }
        public override object CreateDockableContent()
        {
            if (Content != null)
            {
                Tools.ExecuteMxs(ExecuteOnOpen);
                return Content;
            }
            return null;
        }
        public override DockStates.Dock DockingModes
        {
            get
            {
                return DockStates.Dock.Left | DockStates.Dock.Right | DockStates.Dock.Floating;
            }
        }
    }
}
