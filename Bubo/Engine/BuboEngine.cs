using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
using System.Reflection;
using Microsoft.SqlServer.Server;
using System.Timers;

namespace Bubo
{
    /// <summary>
    /// Base class to manage custom treeview 
    ///  - layer nodes can be automaticaly created from external xml file. 
    ///   it allow to group MaxItems by categories
    ///  - CRUD methods for nodes
    /// </summary>
    public class BuboEngine : INotifyPropertyChanged
    {
        #region virtual properties
        public virtual IClass_ID ClassID { get; }
        public virtual TreeItem TreeRoot { get; } = new TreeItem("Root", true);
        public virtual string KeyConfigRoot { get; } = "";
        public virtual string KeyTreeViewCM { get; } = "";

        #endregion
        #region common properties

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        XElement _config;

        public ObservableCollection<string> ExcluEnds { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ExcluPatterns { get; } = new ObservableCollection<string>();

        string _excludeEndKey = "ExcludeEnd";
        string _excludePatternKey = "ExcludePattern";
        LayerItem _defaultLayer;
        public LayerItem DefaultLayer
        {
            get
            {
                return _defaultLayer;
            }
        }
        public List<TreeItem> SelectedItems { get; } = new List<TreeItem>();
        public TreeItem SelectedItem
        {
            get
            {
                if (SelectedItems.Count > 0)
                {
                    return SelectedItems[SelectedItems.Count - 1];
                }
                else
                {
                    return null;
                }
            }
        }
        public virtual MaxItem SelectedMaxItem
        {
            get
            {
                List<MaxItem> sel = SelectedItems.Where(x => x is MaxItem).Cast<MaxItem>().ToList();
                if (sel.Count > 0)
                {
                    return sel[sel.Count - 1];
                }
                else
                {
                    return null;
                }
            }
        }
        
        string _nodeName ="";
        public string NodeName
        {
            get
            {
                return _nodeName;
            }
            set
            {
                _nodeName = value;
                NotifyPropertyChanged(nameof(NodeName));
            }
        }
        string _baseName;
        public string BaseName
        {
            get
            {
                return _baseName;
            }
        }
      
        protected int _maxItemCount;
        protected int _maxItemSelected;
        protected bool _isConfigMode = true;

        IBuboMod _currentMod;
        public virtual IBuboMod CurrentMod 
        {
            get
            {
                return _currentMod;
            }
            set
            {
                _currentMod = value;
                NotifyPropertyChanged(nameof(CurrentMod));
                if (CurrentMod != null)
                {
                    SetBaseName(CurrentMod.Node);
                }
            }
        }

        bool _showItemR = true;
        public bool ShowItemR
        {
            get
            {
                return _showItemR;
            }
            set
            {
                _showItemR = value;
                NotifyPropertyChanged(nameof(ShowItemR));
            }
        }

        bool _showItemM = true;
        public bool ShowItemM
        {
            get
            {
                return _showItemM;
            }
            set
            {
                _showItemM = value;
                NotifyPropertyChanged(nameof(ShowItemM));
            }
        }

        bool _showItemL = false;
        public bool ShowItemL
        {
            get
            {
                return _showItemL;
            }
            set
            {
                _showItemL = value;
                NotifyPropertyChanged(nameof(ShowItemL));
            }
        }

        bool _excluBaseName = true;
        public bool ExcluBaseName
        {
            get
            {
                return _excluBaseName;
            }
            set
            {
                _excluBaseName = value;
                NotifyPropertyChanged(nameof(ExcluBaseName));
            }
        }
        bool _excluEndName = true;
        public bool ExcluEndName
        {
            get
            {
                return _excluEndName;
            }
            set
            {
                _excluEndName = value;
                NotifyPropertyChanged(nameof(ExcluEndName));
            }
        }
        bool _excluPattern = true;
        public bool ExcluPattern
        {
            get
            {
                return _excluPattern;
            }
            set
            {
                _excluPattern = value;
                NotifyPropertyChanged(nameof(ExcluPattern));
            }
        }



        #endregion

        #region virtual methods

        public virtual IBuboMod AutoLoadMod(IINode node)
        {
            IModifier m = MaxSDK.GetModifier(node, ClassID);
            IBuboMod mod = LoadModInUI(m, node);

            if (mod != null)
            {
                Tools.Format(MethodBase.GetCurrentMethod(), "Result : " + ModPanelNotification.AutoLoadTrue);
            }
            else
            {
                Tools.Format(MethodBase.GetCurrentMethod(), "Result : " + ModPanelNotification.AutoLoadFalse);
            }
            return mod;
        }
        public virtual IBuboMod LoadModInUI(IModifier m, IINode node)
        {
            IBuboMod validMod = CreateValidMod(m, node);
            if (validMod != null)
            {

                DisposeMod();
                CurrentMod = validMod;
                CurrentMod.RedrawViews();
                if (_isConfigMode)
                {
                    LoadConfig();
                    ConfigLayers(CurrentMod.MaxItems, TreeRoot);
                    ConfigLayerDefault(CurrentMod.MaxItems, DefaultLayer);
                }
                else
                {
                    ConfigLayerDefault(CurrentMod.MaxItems, TreeRoot);
                }
                SelectItem(CurrentMod.Selected, false);
                _maxItemCount = CurrentMod.Count;
                _maxItemSelected = CurrentMod.Selected;
            }
            else
            {
                Tools.Format(MethodBase.GetCurrentMethod(), "Result : " + LoadModNotification.NULL_Input);
            }
            return CurrentMod;
        }
        public virtual IBuboMod  LoadCurrentMod()
        {
            return LoadModInUI(CurrentMod.Modifier,CurrentMod.Node);
        }
        public virtual IBuboMod CreateValidMod(IModifier m, IINode node)
        {
            if (node != null)
            {
                NodeName = node.Name;
            }
            return null;
        }
        public virtual void OnModDeleted(IModifier m, IINode node)
        {
            if (CurrentMod != null && !MaxSDK.ModifierExists(CurrentMod.Modifier, node))
            {
                string msg = "Modifier DELETED";
                Tools.Format(MethodBase.GetCurrentMethod(), msg);
                DisposeMod();
                LoadModInUI(m, node);

            }
        }
        public void DeleteMod()
        {
            if (CurrentMod!=null)
            {
                MaxSDK.DeleteModifier(CurrentMod.Node,CurrentMod.Modifier);
            }
        }
        public virtual void OnNodeTabChanged()
        {
            if (CurrentMod != null)
            {
                if (CurrentMod.Count != CurrentMod.MaxItems.Count)
                {
                    LoadModInUI(CurrentMod.Modifier, CurrentMod.Node);
                }
                else
                {
                    CurrentMod.RedrawUI(RedrawUIOption.Full);
                }
            }
        }
        public virtual void OnTimeChanged()
        {

        }
        public virtual void OnNodeRenameChanged()
        {
            if (CurrentMod != null)
            {
                NodeName = CurrentMod.Node.Name;
                CurrentMod.RedrawUI(RedrawUIOption.RefreshItems);
                Tools.Format(MethodBase.GetCurrentMethod(), CurrentMod.Modifier.Name);
            }
        }
        public virtual InModNotification InMod(IModifier m, IINode node)
        {
            InModNotification notify;

            if (CurrentMod != null && MaxSDK.IsEquals(m, CurrentMod.Modifier))
            {
                if (CurrentMod.Count != _maxItemCount)
                {
                    LoadModInUI(CurrentMod.Modifier, CurrentMod.Node);
                    _maxItemCount = CurrentMod.Count;
                    notify = InModNotification.MaxItemCount;
                }
                else
                {
                    notify = InModNotification.InMod;
                }
            }
            else
            {
                notify = InModNotification.NotMod;
            }
            Tools.Format(MethodBase.GetCurrentMethod(), notify.ToString());
            return notify;
        }

        #endregion
        #region common methods

        public virtual XElement ResetConfig( XElement rootDoc )
        {
            XElement configRoot = new XElement(KeyConfigRoot);
            rootDoc.Add(configRoot);

            List<XElement> main = GrowConfig(configRoot, "Layer", new string[] { "Facial", "Body" });
            List<XElement> facial = GrowConfig(main[0], "Layer", new string[] { "Upper Face", "Lower Face" });
            List<XElement> UpperFace = GrowConfig(facial[0], "Layer", new string[] { "EyeBrow", "Eye" });
            GrowConfig(UpperFace[0], "Pattern", new string[] { "EyeBrow_" });
            GrowConfig(UpperFace[1], "Pattern", new string[] { "CreaseEye_", "Eye_", "MasterEye_" });
            List<XElement> LowerFace = GrowConfig(facial[1], "Layer", new string[] { "Mouth" });
            GrowConfig(LowerFace[0], "Pattern", new string[] { "UpperLips_", "LowerLips_", "MouthCorner_" });

            return configRoot;
        }

        public void SetBaseName (IINode node)
        {
            string outName = "";
            string[] buffer = node.Name.Split('_');
            if (buffer.Count() > 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    outName += buffer[i];
                    if (i == 0)
                    {
                        outName += "_";
                    }
                }
                _baseName = outName;
            }
            else
            {
                _baseName = "";
            }
        }
        public List<XElement> GrowConfig(XElement xl, string type, IEnumerable<string> children)
        {
            List<XElement> output = new List<XElement>();
            foreach (string ch in children)
            {
                XElement el = new XElement(type, new XAttribute("name", ch));
                output.Add(el);
                xl.Add(el);
            }
            return output;
        }
        public void DisposeConfig()
        {
            _config = null;
            ExcluEnds.Clear();
            ExcluPatterns.Clear();
        }
        public virtual void DisposeMod()
        {
            if (CurrentMod != null && CurrentMod.MaxItems != null)
            {
                CurrentMod.MaxItems.ClearTree();
            }
            ClearMaxItems(TreeRoot);
            CurrentMod = null;
        }
        public void LoadConfig()
        {
            if (File.Exists(INI.ConfigFile))
            {
                if (_config == null)
                {
                    ClearTree();
                    _config = XElement.Load(INI.ConfigFile);
                    XElement configRoot = _config.Element(KeyConfigRoot);
                    if (configRoot != null)
                    {
                        GetEndPatterns(configRoot, _excludeEndKey, ExcluEnds);
                        GetEndPatterns(configRoot, _excludePatternKey, ExcluPatterns);
                        GetLayerItems(configRoot, TreeRoot);
                    }
                    else
                    {
                        Tools.Format(MethodBase.GetCurrentMethod(), "ConfigRoot was not found!");
                    }
                }
            }
        }
        public void GetEndPatterns(XElement configRoot , string key , ICollection<string> patterns )
        {
            patterns.Clear();
            XElement excludeEnd = configRoot.Element(key);
            if (excludeEnd != null)
            {
                foreach (XElement xl in excludeEnd.Elements("Pattern"))
                {
                    if (xl.Attribute("name") is XAttribute att && att.Value is string name)
                    {
                        patterns.Add(name);
                    }
                }
            }
        }
        public List<LayerItem> GetLayerItems(XElement root, TreeItem parent)
        {
            List<LayerItem> layers = new List<LayerItem>();

            foreach (XElement xl in root.Elements("Pattern"))
            {
                if (parent is LayerItem layParent)
                {
                    if (xl.Attribute("name") is XAttribute att && att.Value is string name)
                    {
                        layParent.Patterns.Add(name);
                    }
                }
            }
            foreach (XElement xl in root.Elements("Layer"))
            {
                if (xl.Attribute("name") is XAttribute att && att.Value is string name)
                {
                    LayerItem lay = new LayerItem(name, parent);
                    layers.Add(lay);
                    layers.AddRange(GetLayerItems(xl, lay));
                }
            }
            return layers;
        }
        public void ConfigLayers(IEnumerable<TreeItem> objects, TreeItem parent)
        {
            foreach (TreeItem it in parent.Children)
            {
                if (it is LayerItem lay)
                {
                    foreach (string pat in lay.Patterns)
                    {
                        foreach (TreeItem o in objects.Where<TreeItem>(x => Regex.IsMatch(x.Name, pat, RegexOptions.IgnoreCase)))
                        {
                            if (o is MaxItem maxItem)
                            {
                                maxItem.Parent = lay;
                                maxItem.ParentPattern = pat;
                            }
                        }
                    }
                }
                ConfigLayers(objects, it);
            }
        }
        public void ConfigLayerDefault(IEnumerable<TreeItem> objects, TreeItem parent)
        {
            foreach (TreeItem it in objects.Where(x => x.Parent == null))
            {
                it.Parent = parent;
            }
        }
        public void InitUI()
        {
            if (_defaultLayer == null)
            {
                _defaultLayer = new LayerItem("Other");
                _defaultLayer.Parent = TreeRoot;
            }
            AutoLoadMod(MaxSDK.GetMaxSelection(0));
        }
        public ModPanelNotification NotifyModInUI(IModifier m ,IINode node)
        {
            if (!MaxSDK.IsClassOf(m, ClassID))
            {
                return ModPanelNotification.NotLoad;
            }
            else if (CurrentMod == null)
            {
                LoadModInUI(m, node);
                return ModPanelNotification.InitLoad;
            }
            else if (MaxSDK.IsEquals(CurrentMod.Node, node))
            {
                if (MaxSDK.IsEquals(CurrentMod.Modifier, m))
                {
                    CurrentMod.RedrawUI(RedrawUIOption.Full);
                    return ModPanelNotification.SelfLoad;
                }
                else
                {
                    LoadModInUI(m, node);
                    return ModPanelNotification.AddLoad;
                }
            }
            else
            {
                if (AutoLoadMod(node) is IBuboMod mod)
                {
                    return ModPanelNotification.AutoLoadTrue;
                }
                return ModPanelNotification.AutoLoadFalse;
            }
        }
        public void ClearTree()
        {
            ClearMaxItems(TreeRoot);
            List<TreeItem> items = new List<TreeItem>(TreeRoot.Children);
            foreach (TreeItem it in items)
            {
                if (it is LayerItem layer && it != _defaultLayer)
                {
                    TreeRoot.Children.Remove(it);
                }
            }
        }
        public void ClearMaxItems(TreeItem current)
        {
            List<TreeItem> items = new List<TreeItem>(current.Children);
            foreach (TreeItem it in items)
            {
                ClearMaxItems(it);
                if (it is MaxItem)
                {
                    current.Children.Remove(it);
                }
            }
        }
        public void ClearSelection()
        {
            foreach (TreeItem it in TreeRoot.GetDescendents<TreeItem>().Where(x => x.IsItemSelected))
            {
                it.IsItemSelected = false;
            }
            SelectedItems.Clear();
        }
        public void SetSelectedItems (TreeItem it , bool select )
        {
            it.IsItemSelected = select;
            if (it.IsItemSelected && !SelectedItems.Exists(x => x == it))
            {
                SelectedItems.Add(it);
            }
            else if (!it.IsItemSelected)
            {
                SelectedItems.Remove(it);
            }
        }
        public void SetSelectedItems(List<TreeItem> items)
        {
            ClearSelection();
            foreach (TreeItem it in items.Where(x => x.IsVisible))
            {
                Tools.Format(MethodBase.GetCurrentMethod(), it.Name);
                SetSelectedItems(it, true);
            }
        }
        public void SetSelectedItems(TreeItem startIt, TreeItem endItem)
        {
            List<TreeItem> items = TreeRoot.GetDescendents<TreeItem>();

            int start = items.IndexOf(startIt);
            int end = items.IndexOf(endItem);

            if (start > end)
            {
                int tmp = start;
                start = end;
                end = tmp;
            }
            for (int i = start; i <= end; i++)
            {
                if (items[i].IsVisible)
                {
                    SetSelectedItems(items[i], true);
                }
            }
        }
        public void SelectItem(TreeItem it, SelectionItem options , bool selectMaxNode)
        {
            if (options == SelectionItem.Select)
            {
                ClearSelection();
                SetSelectedItems(it, true);
            }
            else if (options == SelectionItem.SelectMore)
            {
                TreeItem startItem = SelectedItem;
                SetSelectedItems(startItem, it);
            }
            else if (options == SelectionItem.SelectToggle)
            {
                SetSelectedItems(it, !it.IsItemSelected);
            }
            if (selectMaxNode && CurrentMod != null && SelectedMaxItem != null)
            {

                if (Main.CurrentEngine.CurrentMod is SkinMod mod)
                {
                    LinkMax.StartMute();
                    MaxSDK.SetCurrentObject(mod.Modifier);
                    if (!mod.IsEditEnvelopes)
                    {
                        mod.IsEditEnvelopes = true;
                    }
                }
                LinkMax.StartMute();
                CurrentMod.SelectMaxItem(SelectedMaxItem);
            }
        }
        public  void SelectItem( int index, bool selectMaxNode)
        {
            if (index > -1 && index < CurrentMod.MaxItems.Count)
            {
                SelectItem(CurrentMod.MaxItems[index], SelectionItem.Select, selectMaxNode);
            }
        }

        #endregion
    }
}
