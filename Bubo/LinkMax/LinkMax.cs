using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;

namespace Bubo
{
    /// <summary>
    /// link 3dsmax events with WPF UI
    /// </summary>
    public class LinkMax
    {
        static public IGlobal Global = GlobalInterface.Instance;
        static public IInterval Interval = Global.Interval.Create();
#if MAX_2022
        static public IInterface14 Interface = Global.COREInterface14;
#else
        static public IInterface19 Interface = Global.COREInterface19;
#endif

        static GlobalDelegates.Delegate5 _scene;
        static GlobalDelegates.Delegate5 _selChanged;
        static GlobalDelegates.Delegate5 _modChanged;
        static GlobalDelegates.Delegate5 _modDeleted;
        static GlobalDelegates.Delegate5 _undo;
        public static TimeChangedEvent _timeChanged;
#if MAX_2020
        static GlobalDelegates.Delegate18 _mxsTest;
#elif MAX_2021

        static GlobalDelegates.Delegate20 _mxsTest;
#endif
        public static System.Timers.Timer _timer;
        public static bool Mute { get; set; }
        static bool _suspendCalls;
        public static string NetCuiFile;
        public static string LocCuiFile;

        static unsafe LinkMax()
        {
            _undo = new GlobalDelegates.Delegate5(OnUndo);
            _selChanged = new GlobalDelegates.Delegate5(OnSelectionChanged);
            _modChanged = new GlobalDelegates.Delegate5(OnModPanelChanged);
            _modDeleted = new GlobalDelegates.Delegate5(OnModDeleted);
            _scene = new GlobalDelegates.Delegate5(OnSceneChanged);
            _timer = new System.Timers.Timer();
        }

        public static void StartUpdate(double interval = 5000)
        {
            _timer.Elapsed += new ElapsedEventHandler(OnUpdateTimedEvent);
            _timer.Interval = interval;
            _timer.Start();
        }
        public static void OnUpdateTimedEvent(object source, ElapsedEventArgs e)
        {
            Tools.Format(MethodBase.GetCurrentMethod(), "Try Delete");
            _timer.Stop();
            if (File.Exists(LocCuiFile))
            {
                File.Delete(LocCuiFile);
            }
            File.Copy(NetCuiFile, LocCuiFile);
            _timer.Elapsed -= new ElapsedEventHandler(OnUpdateTimedEvent);
        }
        public static void SuspendCallbacks()
        {
            _suspendCalls = Mute;
            Mute = true;
        }
        public static void RestoreCallbacks()
        {
            Mute = _suspendCalls; 
        }
        public static void StartMute(double interval = 250)
        {
            if (!Mute)
            {
                _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                _timer.Interval = interval;
                _timer.Start();
                Mute = true;
            }
        }
        static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Mute = false;
            _timer.Stop();
            _timer.Elapsed -= new ElapsedEventHandler(OnTimedEvent);
        }
        static void OnSceneChanged (IntPtr obj, INotifyInfo info)
        {
            Main.Instance.DisposeScene();
        }
        static void OnSelectionChanged(IntPtr obj, INotifyInfo info)
        {
            if (!Mute)
            {
                IINode node = MaxSDK.GetMaxSelection(0);
                Main.Instance.SyncSelectedNode(node);

                RegisterWhenNodeTransform(node);
            }
        }
        public static void OnNodeRenameChanged()
        {
            if (!Mute)
            {
                Main.CurrentEngine.OnNodeRenameChanged();
            }
        }
        public static void OnNodeTransformChanged()
        {
            if (!Mute)
            {
                Main.Morph.CurrentMod.RedrawUI(RedrawUIOption.RefreshMorphValues);
            }
        }
        static void OnModPanelChanged(IntPtr obj, INotifyInfo info)
        {
            if (!Mute)
            {
                Main.Instance.SyncEngineFromMax(MaxSDK.GetCurrentModifier(), MaxSDK.GetMaxSelection(0));
            }
        }
        public static void OnModDeleted(IntPtr obj, INotifyInfo info)
        {
            if (!Mute)
            {
                Main.CurrentEngine.OnModDeleted(MaxSDK.GetCurrentModifier(), MaxSDK.GetMaxSelection(0));
            }
        }
        public static void OnUndo(IntPtr obj, INotifyInfo info)
        {
            OnUndo();
        }
        public static void OnUndo()
        {
            if (!Mute)
            {
                Main.CurrentEngine.OnNodeTabChanged();
            }
        }
        public static void OnNodeTabChanged()
        {
            if (!Mute)
            {
                Main.CurrentEngine.OnNodeTabChanged();
            }
        }
        public static void OnInMod()
        {
            if (!Mute)
            {
                StartMute();
                Tools.Format(MethodBase.GetCurrentMethod(), "Begin");
                InModNotification notification = Main.CurrentEngine.InMod(MaxSDK.GetCurrentModifier(), MaxSDK.GetMaxSelection(0));
                if (Main.Skin.CurrentMod is SkinMod mod)
                {
                    bool editEnv = mod.GetEditEnvelopes();
                    mod.SetEditEnvelopesDisplay(editEnv, editEnv);
                    mod.DisplayHoldBones(editEnv);
                }
            }
        }
        static void OnMaxTimeChanged(TimeChangeEventArgs e)
        {
            if (UISettings.Instance.RefreshOnTime && Main.Instance.SelectedTab == 1)
            {
                Main.Morph.OnTimeChanged();
            }
        }
        static void RegisterTimeChanged()
        {
            _timeChanged += OnMaxTimeChanged;
            Interface.RegisterTimeChangeCallback(TimeChangedCallback.Instance);
        }
        static void UnregisterTimeChanged()
        {
            _timeChanged -= OnMaxTimeChanged;
            Interface.UnRegisterTimeChangeCallback(TimeChangedCallback.Instance);
        }
        static void RegisterNodeEvents()
        {
            MaxSDK.ExecuteMxs("BuboNodeDeleted = NodeEventCallback mouseUp:true delay:1000  deleted:Bubo.NodeDeleted");
            MaxSDK.ExecuteMxs("BuboNodeRenamed = NodeEventCallback mouseUp:true delay:1000  nameChanged:Bubo.NodeRenamed");
            MaxSDK.ExecuteMxs("BuboModifierChanged = NodeEventCallback mouseUp:true delay:1000   geometryChanged:Bubo.InMod");
           
            MaxSDK.ExecuteMxs("RegisterTimeCallback BuboLinkMax.OnTimeChanged");
        }
        static void UnregisterNodeEvents()
        {
            MaxSDK.ExecuteMxs("BuboNodeDeleted = undefined");
            MaxSDK.ExecuteMxs("BuboNodeRenamed = undefined");
            MaxSDK.ExecuteMxs("BuboModifierChanged = undefined");

            MaxSDK.ExecuteMxs("UnRegisterTimeCallback BuboLinkMax.OnTimeChanged");
            MaxSDK.ExecuteMxs("gc light:true");
        }
        public static void RegisterWhenNodeTransform( IINode node)
        {
            UnregisterWhenNodeTransform();
            Tools.Format(MethodBase.GetCurrentMethod(), "");
            if (node is IINode &&  Main.Instance.SelectedTab == 1 && Main.Morph.CurrentMod is MorphMod mod && !MaxSDK.IsEquals(node, mod.Node))
            {
                MaxSDK.ToMaxScript(node, "buboTransformedNode");
                MaxSDK.ExecuteMxs("Bubo.WhenTransformChanged  buboTransformedNode");
            }
        }
        public static void UnregisterWhenNodeTransform()
        {
            Tools.Format(MethodBase.GetCurrentMethod(), "");
            MaxSDK.ExecuteMxs("deleteAllChangeHandlers id:#BuboNodeTransformChanged");
        }
        public static void RegisterCallbacks()
        {
            Global.RegisterNotification(_undo, null, SystemNotificationCode.SceneUndo);
            Global.RegisterNotification(_modDeleted, null, SystemNotificationCode.PostModifierDeleted);
            Global.RegisterNotification(_modChanged, null, SystemNotificationCode.ModpanelSelChanged);
            Global.RegisterNotification(_selChanged, null, SystemNotificationCode.SelectionsetChanged);
            Global.RegisterNotification(_scene, null, SystemNotificationCode.FilePostOpen);
            Global.RegisterNotification(_scene, null, SystemNotificationCode.SystemPostNew);
            Global.RegisterNotification(_scene, null, SystemNotificationCode.SystemPostReset);

            RegisterNodeEvents();
            RegisterTimeChanged();
        }
        public static void UnregisterCallbacks()
        {
            Global.UnRegisterNotification(_scene, null);
            Global.UnRegisterNotification(_modDeleted, null);
            Global.UnRegisterNotification(_modChanged, null);
            Global.UnRegisterNotification(_selChanged, null);
            Global.UnRegisterNotification(_undo, null);

            UnregisterNodeEvents();
            UnregisterTimeChanged();
            UnregisterWhenNodeTransform();
        }
    }
}
