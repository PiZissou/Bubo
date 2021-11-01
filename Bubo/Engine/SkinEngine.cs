using Bubo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using System.Reflection;
using System.IO;
using System.Xml.Linq;

namespace Bubo
{
    public class SkinEngine : BuboEngine
    {
        List<SkinVtx> _vertSel = new List<SkinVtx>();
        #region override properties

        public override IClass_ID ClassID { get; } = MaxSDK.SKIN_CLASSID;
        public override TreeItem TreeRoot { get; } = new TreeItem("RootSkin", true);
        public override string KeyConfigRoot { get; } = "Skin";
        public override string KeyTreeViewCM { get; } = "TreeViewSkinCM";
        public override IBuboMod CurrentMod
        {
            get
            {
                return base.CurrentMod;
            }
            set
            {
                base.CurrentMod = value;
                NotifyPropertyChanged(nameof(CurrentSkin));
            }
        }
        #endregion
        #region override methods

        public override IBuboMod CreateValidMod(IModifier m, IINode node)
        {
            base.CreateValidMod(m, node);
            if (MaxSDK.IsClassOf(m,ClassID))
            {
                SkinMod mod = new SkinMod(m, node);
                mod.SaveProperties();
                return mod;
            }
            return null;
        }

        public override InModNotification InMod(IModifier m, IINode node)
        {
            try
            {
                InModNotification notify;
                if (base.InMod(m,node) == InModNotification.InMod)
                {
                    if (CurrentSkin.Selected != _maxItemSelected)
                    {
                        _maxItemSelected = CurrentSkin.Selected;
                        
                        SelectItem(CurrentSkin.Selected, false );

                        notify = InModNotification.MaxItemSel;
                    }
                    else 
                    {
                        CurrentSkin.RedrawUI(RedrawUIOption.Full);
                        notify = InModNotification.InModRedraw;
                    }
                }
                else
                {
                    notify = InModNotification.NotMod;
                }
                Tools.Format(MethodBase.GetCurrentMethod(), notify.ToString());
                return notify;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return InModNotification.ExceptionInMod;
            }
        }
        #endregion
        public override XElement ResetConfig(XElement rootDoc)
        {
            XElement configRoot = base.ResetConfig(rootDoc);

            if (configRoot != null)
            {
                XElement excludeEnd = new XElement("ExcludeEnd");
                GrowConfig(excludeEnd, "Pattern", new string[] { "_Skin_RIG", "_SKIN", "_RIG" });
                configRoot.Add(excludeEnd);
                return configRoot;
            }
            return null;
        }
        #region local properties

        private float _absoluteWeight = 1.0f;
        public float AbsoluteWeight
        {
            get
            {
                return _absoluteWeight;
            }
            set
            {
                _absoluteWeight = value;
                NotifyPropertyChanged(nameof(AbsoluteWeight));
            }
        }

        private float _absoluteDQWeight = .8f;
        public float AbsoluteDQWeight
        {
            get
            {
                return _absoluteDQWeight;
            }
            set
            {
                _absoluteDQWeight = value;
                NotifyPropertyChanged(nameof(AbsoluteDQWeight));
            }
        }

        private float _transWeightBones = 50;
        public float TransWeightBones
        {
            get
            {
                return _transWeightBones;
            }
            set
            {
                _transWeightBones = value;
                NotifyPropertyChanged(nameof(TransWeightBones));
            }
        }

        float _currentWeight = 0.01f;
        public float CurrentWeight
        {
            get
            {
                return _currentWeight;
            }
            set
            {
                _currentWeight = value;
                NotifyPropertyChanged(nameof(CurrentWeight));
            }
        }
        
        float _scaleWeight = 0.95f;
        public float ScaleWeight
        {
            get
            {
                return _scaleWeight;
            }
            set
            {
                _scaleWeight = value;
                NotifyPropertyChanged(nameof(ScaleWeight));
            }
        }
        float _dQWeight = 0.8f;
        public float DQWeight
        {
            get
            {
                return _dQWeight;
            }
            set
            {
                _dQWeight = value;
                NotifyPropertyChanged(nameof(DQWeight));
            }
        }
        float _zeroWeight = 0.001f;
        public float ZeroWeight
        {
            get
            {
                return _zeroWeight;
            }
            set
            {
                _zeroWeight = value;
                NotifyPropertyChanged(nameof(ZeroWeight));
            }
        }

        float _mixSkinValue = 100;
        public float MixSkinValue
        {
            get
            {
                return _mixSkinValue;
            }
            set
            {
                _mixSkinValue = value;
                NotifyPropertyChanged(nameof(MixSkinValue));
            }
        }
        bool _unHoldOnly = false;
        public bool UnHoldOnly
        {
            get
            {
                return _unHoldOnly;
            }
            set
            {
                _unHoldOnly = value;
                _isConfigMode = !value;
                LoadCurrentMod();
                NotifyPropertyChanged(nameof(UnHoldOnly));
            }
        }

        SkinMod _currentSkin;
        public SkinMod CurrentSkin
        {
            get
            {
                if (Main.Instance.IsDisposed)
                {
                    IINode node = MaxSDK.GetMaxSelection(0);
                    IModifier m = MaxSDK.GetCurrentModifier();
                    if (_currentSkin != null && MaxSDK.IsEquals(m, _currentSkin.Modifier) && MaxSDK.IsEquals(node, _currentSkin.Node))
                    { 
                    }
                    else
                    {
                        _currentSkin = CreateValidMod(m, node) as SkinMod;
                    }
                }
                else
                {
                    if (CurrentMod != null)
                    {
                        _currentSkin = CurrentMod as SkinMod;
                    }
                    else
                    {
                        _currentSkin = null;
                    }
                }
                return _currentSkin;
            }
        }

        public override void DisposeMod()
        {
            base.DisposeMod();
            NotifyPropertyChanged(nameof(CurrentSkin));
            Tools.Format(MethodBase.GetCurrentMethod(), (CurrentSkin == null).ToString());
        }
        public override MaxItem SelectedMaxItem
        {
            get
            {

                if (Main.Instance.IsDisposed )
                { 
                    if (CurrentSkin is SkinMod && CurrentSkin.MaxItems.Count > 0)
                        return CurrentSkin.SelectedItem;
                    else 
                    {
                        return null;
                    }
                }
                else
                {
                    return base.SelectedMaxItem;
                }
            }
        }
        #endregion

        #region local methods

        public void AddBones(List<IINode> nodes)
        {
            try
            {
                if (CurrentSkin != null)
                {
                    SkinMod.AddBones(CurrentSkin.Modifier, nodes);

                    MaxSDK.SetMaxSelection(new List<IINode> { CurrentSkin.Node });
                    MaxSDK.SetCurrentObject(CurrentSkin.Modifier);
                    List<TreeItem> sel = CurrentSkin.MaxItems.Where(x => x is SkinItem it && nodes.Exists(b => b.Name == it.Bone.Name)).Select(s => s).ToList<TreeItem>();
                    if (sel.Count > 0)
                    {
                        SetSelectedItems(sel);
                        CurrentSkin.SelectMaxItem(sel[sel.Count - 1] as MaxItem);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SelectNextBone()
        {
            try
            {
                if (CurrentSkin != null)
                {
                    if (!CurrentSkin.IsEditEnvelopes)
                    {
                        CurrentSkin.IsEditEnvelopes = true;
                    }
                    CurrentSkin.SelectNextBone();
                    _maxItemSelected = CurrentSkin.Selected;
                    SelectItem(CurrentSkin.Selected,false);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void ReplaceBones(List<IINode> nodes)
        {
            try
            {
                if (CurrentSkin != null)
                {
                    IINode dest = nodes[nodes.Count - 1];
                    nodes.RemoveAt(nodes.Count - 1);
                    CurrentSkin.ReplaceBones( nodes, dest);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void ResetPos()
        {
            try
            {
                if (CurrentSkin != null)
                {
                    CurrentSkin.ResetPos();
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public SkinItem GetSkinBone(IINode bone)
        {
            return CurrentSkin.MaxItems.Find(x => x is SkinItem it && MaxSDK.IsEquals(it.Bone, bone)) as SkinItem;
        }
        public void SaveSkin()
        {
            try
            {
                DirectoryInfo dir = Directory.CreateDirectory(INI.SkinDir);
                string filename = MaxSDK.GetSaveFileName(INI.SkinDir, "BuboSkinSave", "Xml files(*.xml)|*.xml", "BuboSkinDirectories");
                if (filename != null)
                {
                    SkinMod.SaveSkin(CurrentSkin.Modifier, CurrentSkin.Node, filename);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public bool LoadSkin(IModifier m, IINode node, string fileName)
        {
            try
            {
                return SkinMod.MixSkin(m, node, new string[] { fileName }, new float[] { 1 });
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }

        internal void ReplaceSkin()
        {
            ReplaceSkin((float)(MixSkinValue * 0.01) , true);
        }
        internal void ReplaceSkin(float blend , bool onlySelected)
        {
            try
            {
                DirectoryInfo dir = Directory.CreateDirectory(INI.SkinDir);
                string filename = MaxSDK.GetOpenFileName(INI.SkinDir, "BuboSkinLoad", "Xml files(*.xml)|*.xml", "BuboSkinDirectories");
                if (filename != null && File.Exists(filename) && CurrentSkin != null && CurrentSkin is SkinMod skinMod)
                {
                    Tools.Print(filename, DebugLevel.VERBOSE);
                    IModifier m = skinMod.Modifier;
                    IINode skinNode = skinMod.Node;
                    
                    if (1 - blend > 0)
                    {
                        SkinMod.CoreMixSkin(m, skinNode, new SkinData[] { skinMod, new SkinData(filename, skinNode.Name) }, new float[] { 1 - blend, blend }, onlySelected);
                    }
                    else
                    {
                        SkinMod.CoreMixSkin(m, skinNode, new SkinData[] { new SkinData(filename, skinNode.Name) }, new float[] { 1 }, onlySelected);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void Mirrorkin(string side)
        {
            try
            {
                if (CurrentSkin != null)
                {
                    CurrentSkin.MirrorSkin(side);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void WeightsPlus()
        {
            AddWeights(CurrentWeight);
        }
        public void WeightsMinus()
        {
            AddWeights(-CurrentWeight);
        }
        public void AddWeights(float val )
        {
            try
            {
                if ( CurrentSkin is SkinMod skinMod && SelectedItem is SkinItem skinIt)
                {
                    skinMod.SetWeights(skinIt.Bone, skinMod.GetVertSel(), val , true);
                    CurrentSkin.SetDQWeightsSelected(val, true);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetWeights()
        {
            if (CurrentSkin is SkinMod skinMod && SelectedItem is SkinItem skinIt)
            {
                skinMod.SetWeights(skinIt.Bone , skinMod.GetVertSel() , AbsoluteWeight, false);
            }
        }
        public void SetDQWeights()
        {
            if (CurrentSkin != null)
                CurrentSkin.SetDQWeightsSelected(AbsoluteDQWeight, false);
        }

        internal void TransfertWeightBones()
        {
            TransfertWeightBones(TransWeightBones);
        }
        public void TransfertWeightBones(float val)
        {
            try
            {
                if (CurrentSkin != null)
                {
                    CurrentSkin.TransfertWeightBones(val * 0.01f);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        #endregion
    }
}
