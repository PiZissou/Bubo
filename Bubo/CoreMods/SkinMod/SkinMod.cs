using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace Bubo
{
    public partial class SkinMod : SkinData , IBuboMod , INotifyPropertyChanged
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
        public override List<SkinVtx> Vertices
        {
            get
            {
                List < SkinVtx >  vertices = new List<SkinVtx>();
                for (int i = 0; i < SkContext.NumPoints; i++)
                {
                    vertices.Add(new SkinVtx(i, SkContext, Skin));
                }
                return vertices;
            }
        }
        public override List<string> BoneNList
        {
            get
            {
                List<string> boneNames = new List<string>();
                foreach (SkinItem skItem in MaxItems)
                {
                    boneNames.Add(skItem.Bone.Name);
                }
                return boneNames;
            }
        }
        public ObservableCollection<WeightItem> WeightItems { get; } = new ObservableCollection<WeightItem>();
        public IModifier Modifier { get; }
        public IINode Node { get; }
        public string MxsModifier { get; } = "buboSkinMod";
        public string MxsNode { get; } = "buboSkinNode";

        ListTree<MaxItem> _maxItems = new ListTree<MaxItem>();
        public ListTree<MaxItem> MaxItems 
        { 
            get
            {
                if (Main.Instance.IsDisposed)
                {
                    _maxItems = GetSkinItems();
                }
                return _maxItems;
            }
        }
        public IEnumerable<MaxItem>  MaxItemSel
        {
            get
            {
                return MaxItems.Where(x => x.IsItemSelected);
            }
        }
        public int Count 
        {
            get 
            {
                if (Modifier == null)
                {
                    return 0;
                }
                return GetNumBones(Modifier);
            }
        }
        public int Selected
        {
            get
            {
                if (Modifier == null)
                {
                    return -1;
                }
                return GetSelectedId(Modifier); 
            }
        }
        public IINode SelectedBone
        {
            get
            {
                if (Modifier == null)
                {
                    return null;
                }
                return GetBoneByID(Selected);
            }
        }
        public SkinItem SelectedItem
        {
            get
            {
                if ( MaxItemSel.Last() is SkinItem skItem)
                {
                    return skItem;
                }
                return null;
            }
        }
        int _nextBone;
        SkinVtx[] _vertSel = new SkinVtx[0];

        bool _isEditEnvelopes = false;
        public bool IsEditEnvelopes
        {
            get
            {
                return _isEditEnvelopes;
            }
            set
            {
                SetEditEnvelopes(value);
                SetEditEnvelopesDisplay(value, value);
                DisplayHoldBones(value);
                GetEditComponents();

                _isEditEnvelopes = value;
                NotifyPropertyChanged(nameof(IsEditEnvelopes));
            }
        }
        bool _isEditDQ = false;
        public bool IsEditDQ
        {
            get
            {
                return _isEditDQ;
            }
            set
            {
                _isEditDQ = value;
                SetEditingDQ(value);
                NotifyPropertyChanged(nameof(IsEditDQ));
            }
        }

        bool _isEnabledDQ = false;
        public bool IsEnabledDQ
        {
            get
            {
                return _isEnabledDQ;
            }
            set
            {
                _isEnabledDQ = value;
                SetEnabledDQ(value);
                NotifyPropertyChanged(nameof(IsEnabledDQ));
            }
        }
        float _paintBlendWeight = 1.0f;
        public float PaintBlendWeight
        {
            get
            {
                return _paintBlendWeight;
            }
            set
            {
                _paintBlendWeight = value;
                if ( IsPaintBlend )
                {
                    SetPaintMaxStr(value);
                }
                NotifyPropertyChanged(nameof(PaintBlendWeight));
            }
        }
        float _paintSetWeight = 0.01f;
        public float PaintSetWeight
        {
            get
            {
                return _paintSetWeight;
            }
            set
            {
                _paintSetWeight = value;
                if (IsPaintSet)
                {
                    SetPaintMaxStr(value);
                }
                NotifyPropertyChanged(nameof(PaintSetWeight));
            }
        }
        bool _isPaintBlend = false;
        public bool IsPaintBlend
        {
            get
            {
                return _isPaintBlend;
            }
            set
            {
                _isPaintBlend = value;
                if (value)
                {
                    IsPaintSet = false;
                }
                SetPaintWeight(PaintBlendWeight, IsPaintBlend, IsPaintBlend);
                NotifyPropertyChanged(nameof(IsPaintBlend));
            }
        }

        bool _isPaintSet = false;
        public bool IsPaintSet
        {
            get
            {
                return _isPaintSet;
            }
            set
            {
                _isPaintSet = value;
                if (value)
                {
                    IsPaintBlend = false;
                }
                SetPaintWeight(PaintSetWeight, IsPaintBlend, IsPaintSet);
                NotifyPropertyChanged(nameof(IsPaintSet));
            }
        }
        bool _isDisplayFaces = false;
        public bool IsDisplayFaces
        {
            get
            {
                return _isDisplayFaces;
            }
            set
            {
                _isDisplayFaces = value;
                if (value)
                {
                    IsDisplayVertices = false;
                }
                SetDisplayFaces(IsDisplayFaces);
                NotifyPropertyChanged(nameof(IsDisplayFaces));
            }
        }
        bool _isDisplayVertices = false;
        public bool IsDisplayVertices
        {
            get
            {
                return _isDisplayVertices;
            }
            set
            {
                _isDisplayVertices = value;
                if (value)
                {
                    IsDisplayFaces = false;
                }
                SetDisplayVertices(IsDisplayVertices);
                NotifyPropertyChanged(nameof(IsDisplayVertices));
            }
        }
        public void SelectMaxItem(MaxItem maxItem)
        {
            SelectBone( maxItem.MaxIndex + 1);
            GetWeightItemSelected();
        }
        public IISkin Skin { get; }
        public IISkin2 Skin2 { get; }
        public IISkinContextData SkContext { get; }
        public IISkinImportData SkImport { get; }
        public override string NodeName 
        {
            get
            {
                if ( Node != null )
                {
                    return Node.Name;
                }
                else
                {
                    return "";
                }
            }
        }
        public SkinMod(SkinData b)
            : base( b)
        {
            if ( b is SkinMod bMod )
            {
                Modifier = bMod.Modifier;
                Node = bMod.Node;
                Skin = bMod.Skin;
                Skin2 = bMod.Skin2;
                SkContext = bMod.SkContext;
                _maxItems = bMod.MaxItems;
            }
        }
        public SkinMod(IModifier m, IINode skinNode)
        {
            try
            {
                if (m != null && MaxSDK.GetSkin(m) is IISkin sk && MaxSDK.GetSkin2(m) is IISkin2 sk2)
                {
                    Modifier = m;
                    Node = skinNode;
                    MaxSDK.ToMaxScript(Modifier, MxsModifier);
                    MaxSDK.ToMaxScript(Node, MxsNode);
                    Skin = sk;
                    Skin2 = sk2;
                    SkContext = sk.GetContextInterface(skinNode);
                    SkImport = MaxSDK.GetSkinImportData(m);

                    for (int i = 0; i < sk.NumBones; i++)
                    {
                        BoneNList.Add(sk.GetBoneName(i));
                        IINode bone = sk.GetBone(i);
                        MaxItems.Add(new SkinItem(bone.Name, this, bone, i));
                    }
                    RedrawUI(RedrawUIOption.Full);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public ListTree<MaxItem> GetSkinItems()
        {
            try
            {
                ListTree<MaxItem> items = new ListTree<MaxItem>();
                for (int i = 0; i < Skin.NumBones; i++)
                {
                    IINode bone = Skin.GetBone(i);
                    items.Add(new SkinItem(bone.Name, this, bone, i));
                }
                return items;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new ListTree<MaxItem>();
            }

        }

        public void RedrawUI(RedrawUIOption redraw)
        {
            if (redraw == RedrawUIOption.Full || redraw == RedrawUIOption.RefreshItems)
            {
                foreach (MaxItem it in MaxItems)
                {
                    it.Refresh();
                }
            }
            if (redraw == RedrawUIOption.Full )
            {
                bool editEnv = GetEditEnvelopes();
                GetEditComponents();
                SetEditEnvelopesDisplay(editEnv, editEnv);
                DisplayHoldBones(editEnv);
            }
        }
        public void RedrawViews()
        {
            RedrawViews(Modifier);
        }
        public IINode GetBoneByID(int id )
        {
            try
            {
                return MaxSDK.ExecuteMxs(string.Format("skinOps.GetBoneNode {0} {1}", MxsModifier, id + 1)).N;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }

        }
        public void RemoveBoneSel()
        {
            foreach (SkinItem skItem in MaxItemSel )
            {

            }
        }
        public void SelectBoneNodes()
        {
            try
            {
                LinkMax.StartMute();
                IsEditEnvelopes = false;
                MaxSDK.Interface.ClearNodeSelection(true);
                MaxSDK.SetMaxSelection(MaxItemSel.Cast<SkinItem>().Select(x => x.Bone).ToList());
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void HideBoneNodes(IEnumerable<MaxItem> maxitems, bool val)
        {
            try
            {
                foreach (SkinItem skItem in maxitems)
                {
                    skItem.Bone.Hide(val);
                }
            }
            catch ( Exception ex )
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void HideBoneNodes( bool val)
        {
            try
            {
                HideBoneNodes(MaxItemSel, val);
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public bool Load( SkinData skData , bool onlySelected, int[] verticesToMix)
        {
            try
            {
                List<SkinVtx> vertices = new List<SkinVtx>();
                IBitArray bitSel = MaxSDK.Global.BitArray.Create();
                Skin2.GetVertexSelection(Node, bitSel);

                if (verticesToMix != null)
                {
                    bitSel.ClearAll();
                    foreach(int i in verticesToMix)
                    {
                        bitSel.Set(i);
                    }
                }
                else if (onlySelected && bitSel.AnyBitSet)
                {
                
                }
                else
                {
                    bitSel.SetAll();
                    ClearBones();
                }
                Tools.Format(MethodBase.GetCurrentMethod(), "Begin");

                AddBones(Skin, SkImport, skData.GetBoneNodeList());

                foreach (SkinVtx vtx in skData.Vertices.Where( x => bitSel[x.Vtx] == 1) )
                {
                    ITab<float> floatTab = MaxSDK.ToTabFloat(vtx.Weights.ToArray());
                    ITab<IINode> nodeTab = MaxSDK.ToTabNode(vtx.GetBoneNodes());

                    bool success = SkImport.AddWeights(Node, vtx.Vtx, nodeTab, floatTab);
                    SkContext.SetDQBlendWeight(vtx.Vtx, vtx.DualQ);
                    Tools.Format(MethodBase.GetCurrentMethod(), string.Format("success : {0} , {1}", success, vtx.ToString()), DebugLevel.ULTRAVERBOSE);
                }
                return true;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public void SetDQWeightsSelected(float v, bool addWeight)
        {
            try
            {
                if (IsEditDQ)
                {
                    if (addWeight)
                        MaxSDK.ExecuteMxs(string.Format("skinJob.AddVertexDQWeightSelected {0} {1}", MxsModifier, v.ToString("0.000")));
                    else
                        MaxSDK.ExecuteMxs(string.Format("skinJob.SetVertexDQWeightSelected {0} {1}", MxsModifier, v.ToString("0.000")));
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetWeights(IINode bone, SkinVtx[] vertices, float val, bool addWeight)
        {
            try
            {
                if (!IsEditDQ )
                {
                    SetWeights( Modifier , Node, bone, vertices, val, addWeight);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetPaintMaxStr(float val)
        {
            try
            {
                MaxSDK.ExecuteMxs(string.Format("thePainterInterface.maxstr = {0}", val));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetScaleWeights(float val)
        {
            try
            {
                Tools.Format(MethodBase.GetCurrentMethod(), MxsModifier);
                MaxSDK.ExecuteMxs(string.Format("SkinJob.SetScaleWeights {0} {1}", MxsModifier, val));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetEditEnvelopes(bool onOff)
        {
            try
            {
                LinkMax.StartMute();
                MaxSDK.ExecuteMxs(string.Format("SkinJob.SetEditEnvelopes {0} {1} {2}", MxsModifier, MxsNode, onOff));   
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetEditEnvelopesDisplay(bool shading , bool vertexColor)
        {
            try
            {
                LinkMax.StartMute();
                MaxSDK.ExecuteMxs(string.Format("SkinJob.EditEnvelopesDisplay {0} {1} {2} {3}", MxsModifier , MxsNode , shading, vertexColor));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void DisplayHoldBones(bool onOff)
        {
            try
            {
                DisplayHoldBones(Modifier, onOff);
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetPaintWeight( float val, bool blendMode , bool onOff)
        {
            try
            {
                LinkMax.StartMute();
                MaxSDK.ExecuteMxs(string.Format("SkinJob.SetPaintWeights {0} {1} {2} {3}", MxsModifier, val, blendMode, onOff));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetDisplayFaces(bool onOff)
        {
            try
            {
                LinkMax.StartMute();
                MaxSDK.ExecuteMxs(string.Format("SkinJob.SetDisplayFaces {0} {1}", MxsModifier, onOff));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetDisplayVertices(bool onOff)
        {
            try
            {
                LinkMax.StartMute();
                MaxSDK.ExecuteMxs(string.Format("SkinJob.SetDisplayVertices {0} {1}", MxsModifier, onOff));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetEditingDQ(bool onOff)
        {
            try
            {
                LinkMax.StartMute();
                MaxSDK.ExecuteMxs(string.Format("SkinJob.SetEditingDQ {0} {1}", MxsModifier, onOff));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetEnabledDQ(bool onOff)
        {
            try
            {
                LinkMax.StartMute();
                Skin.DQBlending = onOff;
                MaxSDK.ExecuteMxs(string.Format("SkinJob.SetEnabledDQ {0} {1}", MxsModifier, onOff));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public SkinVtx[] GetVertSel()
        {
            try
            {
                List<SkinVtx> sel = new List<SkinVtx>();
                   
                if (!GetEditEnvelopes(Modifier))
                {
                    return new SkinVtx[0];
                }
                IBitArray bitSel = MaxSDK.Global.BitArray.Create();
                Skin2.GetVertexSelection(Node, bitSel);

                if (bitSel.AnyBitSet)
                {
                    for (int i = 0; i < bitSel.Size; i++)
                    {
                        if (bitSel[i] == 1)
                        {
                            sel.Add(new SkinVtx(i, SkContext, Skin));
                        }
                    }
                }
                _vertSel = sel.ToArray();
                return _vertSel;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                _vertSel = new SkinVtx[0];
                return _vertSel;
            }
        }
        public void GetWeightItems(SkinVtx lastVert)
        {
            WeightItems.Clear();
            if (IsEditDQ)
            {
                if (lastVert.WeightsData.Count > 0)
                {
                    WeightItems.Add(lastVert.WeightsData[0]);
                    lastVert.WeightsData[0].LinkedItem = new MaxItem("DuatQuat", this);
                }
            }
            else
            {
                foreach (WeightItem wItem in lastVert.WeightsData)
                {
                    wItem.LinkedItem = MaxItems.Find(x => x.Name == wItem.Bone.Name) as SkinItem;
                    WeightItems.Add(wItem);
                }
            }

        }
        public void GetWeightItemSelected()
        {
            foreach (WeightItem wIt in WeightItems.Where(x => x.LinkedItem is SkinItem))
            {
                wIt.IsItemSelected = (wIt.BoneId == Selected) ? true : false;
            }
        }
        public bool GetEditEnvelopes()
        {
            try
            {
                _isEditEnvelopes = GetEditEnvelopes ( Modifier);
                NotifyPropertyChanged(nameof(IsEditEnvelopes));
                return _isEditEnvelopes;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public void GetEditComponents()
        {
            NotifyEditingDQ();
            NotifyEnabledDQ();
            NotifyPaintBlend();
            NotifyPaintSet();
            NotifyDisplayFaces();
            NotifyDisplayVertices();
            GetVertSel();
            if (_vertSel.Count() > 0)
            {
                GetWeightItems(_vertSel[_vertSel.Count() - 1]);
                GetWeightItemSelected();
            }
        }
        public bool NotifyEnabledDQ()
        {
            try
            {
                _isEnabledDQ = Skin.DQBlending;
                NotifyPropertyChanged(nameof(IsEnabledDQ));
                return _isEnabledDQ;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool NotifyPaintBlend()
        {
            try
            {
                _isPaintBlend = MaxSDK.ExecuteMxs(string.Format("SkinJob.GetPaintMode {0} {1}", MxsModifier, true)).B;
                NotifyPropertyChanged(nameof(IsPaintBlend));
                return _isPaintBlend;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool NotifyPaintSet()
        {
            try
            {
                _isPaintSet = MaxSDK.ExecuteMxs(string.Format("SkinJob.GetPaintMode {0} {1}", MxsModifier, false)).B;
                NotifyPropertyChanged(nameof(IsPaintSet));
                return _isPaintSet;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool NotifyEditingDQ()
        {
            try
            {
                _isEditDQ = GetEditingDQ( Modifier );
                NotifyPropertyChanged(nameof(IsEditDQ));
                return _isEditDQ;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool NotifyDisplayFaces()
        {
            try
            {
                _isDisplayFaces = MaxSDK.ExecuteMxs(string.Format("SkinJob.GetDisplayFaces {0}", MxsModifier)).B;
                NotifyPropertyChanged(nameof(IsDisplayFaces));
                return _isDisplayFaces;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool NotifyDisplayVertices()
        {
            try
            {
                _isDisplayVertices = MaxSDK.ExecuteMxs(string.Format("SkinJob.GetDisplayVertices {0}", MxsModifier)).B;
                NotifyPropertyChanged(nameof(IsDisplayVertices));
                return _isDisplayVertices;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public void Shrink()
        {
            try
            {
                MaxSDK.ExecuteMxs(string.Format("SkinJob.Shrink {0}", MxsModifier));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void Grow()
        {
            try
            {
                MaxSDK.ExecuteMxs(string.Format("SkinJob.Grow {0}", MxsModifier));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void Ring()
        {
            try
            {
                MaxSDK.ExecuteMxs(string.Format("SkinJob.Ring {0}", MxsModifier));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void Loop()
        {
            try
            {
                MaxSDK.ExecuteMxs(string.Format("SkinJob.Loop {0}", MxsModifier));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public List<IINode> GetUnusedBones()
        {
            try
            {
                List<IINode> unusedBones = new List<IINode>();
                List<string> influences = new List<string>();
                foreach (SkinVtx d in Vertices)
                {
                    influences.AddRange(d.BonesN.Except(influences));
                }

                for (int i = 0; i < Skin.NumBones; i++)
                {
                    IINode bone = Skin.GetBone(i);
                    if (!influences.Exists(x => x == bone.Name))
                    {
                        unusedBones.Add(bone);
                    }
                }
                return unusedBones;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new List<IINode>();
            }
        }
        public void RemoveZeroWeights(float val)
        {
            val = (val == 0.0f) ? 0.0001f : val;
            MaxSDK.ExecuteMxs(string.Format("SkinJob.RemoveZeroWeights {0} {1}", MxsModifier, val.ToString("0.0000")));
        }
        public void RemoveUnusedBones()
        {
            try
            {
                RemoveZeroWeights(0.001f);
                List<IINode> unusedBones = GetUnusedBones();
                foreach (IINode bone in unusedBones)
                {
                    RemoveBone(bone);
                    HoldBone(bone, false);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void HoldToggle()
        {
            try
            {
                Hold(MaxItemSel.Cast<SkinItem>(), !SelectedItem.IsHold);
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void HoldAll(bool onOff)
        {
            Hold(MaxItems.Cast<SkinItem>(), onOff);
        }
        public void Hold(IEnumerable<SkinItem> bones, bool onOff)
        {
            foreach (SkinItem bone in bones)
            {
                Hold(bone, onOff);
            }
        }
        public void Hold(SkinItem bone, bool onOff)
        {
            if (bone != null)
            {
                bone.IsHold = onOff;
                HoldBone(bone.Bone, onOff);
            }
        }
        public void SetWeights(float val)
        {
            try
            {
                if (!IsEditDQ && SelectedBone != null)
                {
                    SetWeights(SelectedBone, GetVertSel() ,val, false);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public int ClampNextBone ( int clamp)
        {
            _nextBone = (clamp > _nextBone) ? _nextBone : 0;
            return _nextBone;
        }
        public void SelectNextBone()
        {
            try
            {
                List<SkinItem> skinItems = MaxItems.Cast<SkinItem>().Where(x => !x.IsHold).Select(b => b).ToList();
                if (MaxSDK.IsEquals(skinItems[ClampNextBone(skinItems.Count)].Bone, SelectedBone))
                {
                    _nextBone += 1;
                }
                SelectMaxItem(skinItems[ClampNextBone(skinItems.Count)]);
                _nextBone += 1;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SelectBone(int index)
        {
            try
            {
                MaxSDK.ExecuteMxs(string.Format("SkinJob.SelectBone {0} {1}", MxsModifier , index));
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
                List<IINode> bones = GetBones(Modifier);
                MaxSDK.ToMaxScript(bones, "nodes");
                MaxSDK.ExecuteMxs(string.Format("SkinJob.ResetPos {0} nodes", MxsModifier));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void ReplaceBones( List<IINode> sourceBones, IINode destBone)
        {
            try
            {
                MaxSDK.ToMaxScript(sourceBones, "nodes");
                MaxSDK.ToMaxScript(destBone, "dest");
                MaxSDK.ExecuteMxs(string.Format("SkinJob.ReplaceBone {0} nodes dest" , MxsModifier));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void MirrorSkin( string side)
        {
            try
            {
                MaxSDK.ExecuteMxs(string.Format("SkinJob.MirrorWeights {0} {1} \"{2}\"", MxsModifier , MxsNode ,side));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void RemoveBone( IINode bone)
        {
            try
            {
                MaxSDK.ToMaxScript(bone, "skinBone");
                MaxSDK.ExecuteMxs(string.Format("SkinJob.RemoveBone {0} skinBone" , MxsModifier));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void ClearBones()
        {
            try
            {
                foreach (IINode bone in GetBones(Modifier))
                {
                    RemoveBone( bone);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void TransfertWeightBones( float val)
        {
            try
            {
                MaxSDK.ExecuteMxs(string.Format("SkinJob.TransfertWeightBones {0} {1}",MxsModifier, val));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public  string GetPropertyChanged()
        {
            return GetPropertyChanged(Modifier);
        }
        public  void SaveProperties()
        {
            SaveProperties(Modifier);
        }
        public  bool IsMutedPropetyChanged()
        {
            return IsMutedPropetyChanged(Modifier);
        }
    }
}
