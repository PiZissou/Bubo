///==========================================
/// Title: Bubo - Setup Manager
/// Author: Pierre Lasbgnes
/// Date:  2012 - 2020
///==========================================
///
using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Bubo
{

    /// <summary>
    /// store all engines to be called in 3dsmax
    /// </summary>
    public class Main : INotifyPropertyChanged
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
                Tools.Print("NotifyPropertyChangedException : " + ex.Message, DebugLevel.EXCEPTION);
            }
        }
        int _selectedTab ;
        public int SelectedTab
        {
            get
            {
                return _selectedTab;
            }
            set
            {
                if (_selectedTab == 2 && value != _selectedTab)
                {
                    Projection.DisplayVertexColor(false);
                }
                _selectedTab = value;
                SyncEngineFromUI(_selectedTab, MaxSDK.GetCurrentModifier() ,MaxSDK.GetMaxSelection(0));
                NotifyPropertyChanged(nameof(SelectedTab));       
            }
        }
        public PolySym PolySym { get; } = new PolySym();
        public ProjectDataEngine Projection { get; } = new ProjectDataEngine();

        public bool IsDisposed = true;
        public bool PreviewTabFirst { get; set; }

        static BuboEngine[] _tabEngines = new BuboEngine[] {};

        public static BuboEngine _currentEngine;
        public static BuboEngine CurrentEngine
        {
            get
            {
                return _currentEngine;
            }
        }
        static Main _instance;
        public static Main Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Main();
                    _tabEngines = new BuboEngine[] { Skin, Morph };
                    _currentEngine = Skin;
                }
                return _instance;
            }
        }
        static SkinEngine _skin;
        public static SkinEngine Skin
        {
            get
            {
                if (_skin == null)
                {
                    _skin = new SkinEngine();
                }
                return _skin;
            }
        }
        static MorphEngine _morph;
        public static MorphEngine Morph
        {
            get
            {
                if (_morph == null)
                {
                    _morph = new MorphEngine();
                }
                return _morph;
            }
        }

        public void SyncEngineFromUI(int index , IModifier m, IINode node)
        {
            _selectedTab = index;
            LinkMax.UnregisterWhenNodeTransform();
            if (index < 2 && PreviewTabFirst)
            {
                if (index == 0)
                {
                    _currentEngine = Skin;
                }
                else
                {
                    _currentEngine = Morph;
                    LinkMax.RegisterWhenNodeTransform(node);
                }

                if (_currentEngine is BuboEngine && node != null)
                {
                    if (UISettings.Instance.SmartMode && _currentEngine.CurrentMod != null && MaxSDK.IsEquals(_currentEngine.CurrentMod.Node, node))
                    {
                        LinkMax.StartMute();
                        MaxSDK.SetCurrentObject(_currentEngine.CurrentMod.Modifier);
                    }
                    else
                    {
                        if (_currentEngine.AutoLoadMod(node) is IBuboMod mod && UISettings.Instance.SmartMode)
                        {
                            LinkMax.StartMute();
                            MaxSDK.SetCurrentObject(mod.Modifier);
                        }
                    }
                }
            }
            PreviewTabFirst = false;
        }
        public void SyncEngineFromMax(IModifier m, IINode node)
        {
            if (UISettings.Instance.SmartMode && node != null && m != null)
            {
                LinkMax.UnregisterWhenNodeTransform();
                if ( MaxSDK.IsSkin(m) )
                {
                    _selectedTab = 0;
                    _currentEngine = Skin;
                }
                else if (MaxSDK.IsMorpher(m))    
                {
                    LinkMax.RegisterWhenNodeTransform( node );
                    _selectedTab = 1;
                    _currentEngine = Morph;
                }
                NotifyPropertyChanged(nameof(SelectedTab));
            }
            if (_currentEngine is BuboEngine )
            {
                ModPanelNotification result = _currentEngine.NotifyModInUI( m , node );
                Tools.Format(MethodBase.GetCurrentMethod(), "Result : " + result);
            }
        }
        public void SyncSelectedNode( IINode node)
        {
            Tools.Format(MethodBase.GetCurrentMethod(), "SEL_NODE");
            if ( node != null)
            {
                if (_currentEngine is BuboEngine engine)
                {
                    engine.AutoLoadMod(node); 
                }
            }
        }
        public void SyncModPanel(IINode node)
        {
            if (UISettings.Instance.SmartMode && _currentEngine is BuboEngine engine && node != null)
            {
                if (engine.CurrentMod != null && MaxSDK.IsEquals(engine.CurrentMod.Node, node))
                {
                    MaxSDK.SetCurrentObject(engine.CurrentMod.Modifier);
                }
                else
                {
                    if (engine.AutoLoadMod(node) is IBuboMod mod)
                    {
                        MaxSDK.SetCurrentObject(mod.Modifier);
                    }
                }
            }
        }
        public void Dispose()
        {
            if (!IsDisposed)
            {
                LinkMax.UnregisterCallbacks();
            }
            IsDisposed = true;
            MaxSDK.SetViewportDisplay(ViewportDisplayColor.Material);
        }
        public void UnDispose()
        {
            if (IsDisposed)
            {
                LinkMax.RegisterCallbacks();

            }
            if (SelectedTab == 1)
            {
                LinkMax.RegisterWhenNodeTransform(MaxSDK.GetMaxSelection(0));
            }
            IsDisposed = false;
        }
        public void LoadUI()
        {
            UISettings.Instance.LoadSettings();
            if (!File.Exists(INI.ConfigFile))
            {
                ResetConfig();
            }
            foreach (BuboEngine engine in _tabEngines)
            {
                engine.InitUI();
            }
        }
        public void ResetConfig()
        {
            XDocument doc = new XDocument();
            XElement rootDoc = new XElement("Root");
            doc.Add(rootDoc);
            foreach (BuboEngine engine in _tabEngines)
            {
                engine.ResetConfig(rootDoc);
            }
            doc.Save(INI.ConfigFile);
        }
        public void Refresh()
        {
            if (_tabEngines != null)
            {
                foreach (BuboEngine engine in _tabEngines.Where(x => x != null))
                {
                    engine.DisposeConfig();
                    engine.InitUI();
                }
            }
        }
        public void DisposeScene()
        {
            foreach (BuboEngine engine in _tabEngines)
            {
                engine.DisposeMod();
                engine.InitUI();
            }

            PolySym.Dispose();
            Projection.Dispose();
        }
        public void DetectSymmetry(IINode node)
        {
            PolySym.DetectMirror(node);
            MaxSDK.ToMaxScript(node, "n");
            MaxSDK.ToMaxScript(PolySym.NotSymmetrical, "sel", false);
            MaxSDK.ExecuteMxs("polyop.setVertSelection n sel");
        }
    }
}
