using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using UiViewModels.Actions;
using MaxCustomControls;

namespace Bubo
{
    public partial class API
    {
        public PolySym Sym
        {
            get
            {
                return Main.Instance.PolySym;
            }
        }
        public API()
        {

        }
        public BuboUI UI
        {
            get
            {
                return BuboUI.Instance;
            }
        }
        public MaxForm GetControl()
        {
            /*             public ElementHost CtrlHost { get; } = new ElementHost();
                    public MainUI()
                    {
                        Activate();
                        CtrlHost.Dock = DockStyle.Fill;
                        Controls.Add(CtrlHost);
                        ShowInTaskbar = false;*/
            MaxForm form = new MaxForm();
            ElementHost ctrlHost = new ElementHost();
            //ctrlHost.Child = UI;
            ctrlHost.Dock = DockStyle.Fill;
            form.Controls.Add(ctrlHost);
            form.ShowInFrame();
            // form.Text = "MAXFORM Bubo";
            return form;
        }

        public void Dispose()
        {
            Main.Instance.Dispose();
        }
        public void UnDispose()
        {
            Main.Instance.UnDispose();
        }
        public void ResetConfig()
        {
            Main.Instance.ResetConfig();
        }
        public void Refresh()
        {
            Main.Instance.Refresh();
        }
        public bool ProjectMorph(int sHandle, int dHandle, string[] channelNames, bool isScript, bool isUnusedTargets, bool createSkwIfNotExists)
        {
            if (MaxSDK.GetAnimByHandle(sHandle) is IINode sourceNode && MaxSDK.GetAnimByHandle(dHandle) is IINode destNode)
            {
                ProjectDataEngine.DoProcessMorph(sourceNode, destNode, isUnusedTargets, isScript, createSkwIfNotExists, channelNames);
                return true;
            }
            return false;
        }

        public bool ProjectSkin(int sHandle, int dHandle, bool isDualQuat, bool isUnusedBones )
        {
            if (MaxSDK.GetAnimByHandle(sHandle) is IINode sourceNode && MaxSDK.GetAnimByHandle(dHandle) is IINode destNode)
            {
                LinkMax.SuspendCallbacks();

                List<int> indicies = new List<int>();
                if (isDualQuat && MaxSDK.GetModifier(sourceNode, MaxSDK.SKIN_CLASSID) is IModifier )
                    indicies = MaxSDK.MapVertIndices(sourceNode, destNode, true);

                ProjectDataEngine.DoProcessSkin(sourceNode, destNode, isDualQuat, isUnusedBones, indicies);

                LinkMax.RestoreCallbacks();
                return true;
            }
            return false;
        }
        public int[] MapIndices(int sHandle, int dHandle)
        {
            
            IINode sNode = MaxSDK.GetAnimByHandle(sHandle)as IINode;
            IINode dNode = MaxSDK.GetAnimByHandle(dHandle) as IINode;
            return MaxSDK.ToMaxIndexTab(MaxSDK.MapVertIndices(sNode, dNode));
        }
        public void LogException( string declaringType , string method , string msg)
        {
            Tools.Format( declaringType , method , msg , DebugLevel.EXCEPTION);
        }
        public void LogException( string msg)
        {
            Tools.Format(MethodBase.GetCurrentMethod(), msg, DebugLevel.EXCEPTION);
        }
        public string GetBasename(string s)
        {
            return Tools.GetBasename(s);
        }
        public string ReplaceBasename(string s, string replaceS)
        { 
            return Tools.ReplaceBasename(s, replaceS);
        }
    }
}
