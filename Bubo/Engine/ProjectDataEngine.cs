using Autodesk.Max;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bubo
{
    public class ProjectDataEngine : INotifyPropertyChanged
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

        bool _isDetected;
        public bool IsDetected
        {
            get
            {
                return _isDetected;
            }
            set
            {
                _isDetected = value;
                NotifyPropertyChanged(nameof(IsDetected));
            }
        }

        bool _isSkin = true;
        public bool IsSkin
        {
            get
            {
                return _isSkin;
            }
            set
            {
                _isSkin = value;
                NotifyPropertyChanged(nameof(IsSkin));
            }
        }

        bool _isMorph = true;
        public bool IsMorph
        {
            get
            {
                return _isMorph;
            }
            set
            {
                _isMorph = value;
                NotifyPropertyChanged(nameof(IsMorph));
            }
        }

        bool _isMapChannel = true;
        public bool IsMapChannel
        {
            get
            {
                return _isMapChannel;
            }
            set
            {
                _isMapChannel = value;
                NotifyPropertyChanged(nameof(IsMapChannel));
            }
        }

        bool _isDualQuat = true;
        public bool IsDualQuat
        {
            get
            {
                return _isDualQuat;
            }
            set
            {
                _isDualQuat = value;
                NotifyPropertyChanged(nameof(IsDualQuat));
            }
        }

        bool _isScript = true;
        public bool IsScript
        {
            get
            {
                return _isScript;
            }
            set
            {
                _isScript = value;
                NotifyPropertyChanged(nameof(IsScript));
            }
        }

        bool _isUnusedTargets = true;
        public bool IsUnusedTargets
        {
            get
            {
                return _isUnusedTargets;
            }
            set
            {
                _isUnusedTargets = value;
                NotifyPropertyChanged(nameof(IsUnusedTargets));
            }
        }

        bool _isUnusedBones = true;
        public bool IsUnusedBones
        {
            get
            {
                return _isUnusedBones;
            }
            set
            {
                _isUnusedBones = value;
                NotifyPropertyChanged(nameof(IsUnusedBones));
            }
        }

        IINode _refNode;
        public IINode RefNode
        {
            get
            {
                return _refNode;
            }
            set
            {
                _refNode = value;
                NotifyPropertyChanged(nameof(RefNode));
            }
        }
        public ObservableCollection<MapItem> MapChannels { get; } = new ObservableCollection<MapItem>();
        public List<MapItem> SelectedMapChannels 
        {
            get
            {
                return MapChannels.Where(x=>x.IsItemSelected).ToList();
            }
        }
        public bool LoadMapChannels(IINode node)
        {
            try
            {
                RefNode = node;
                IsDetected = true;
                MapChannels.Clear();

                foreach (int i in MaxSDK.GetMapChannels(RefNode))
                {
                    MapChannels.Add(new MapItem(i.ToString(), RefNode, i, true));
                }
                if (MapChannels.Count > 0 && MapChannels[0].Channel == 0)
                {
                    MapChannels[0].Name = "vertex Color";
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                IsDetected = false;
                return false;
            }
        }
        public void DoProcess()
        {
            LinkMax.SuspendCallbacks();

            List<IINode> nodes = MaxSDK.GetMaxSelection();

            if (_refNode == null || nodes.Count == 0)
                return;

            nodes.Remove(_refNode);
            bool needMapIndices = (_isSkin && _isDualQuat && MaxSDK.GetModifier(_refNode, MaxSDK.SKIN_CLASSID) is IModifier  || _isMapChannel); 

            foreach (IINode node in nodes)
            {
                List<int> indicies = new List<int>();
                if (needMapIndices)
                    indicies = MaxSDK.MapVertIndices(_refNode, node, true);
                
                if (_isMorph)
                    DoProcessMorph(_refNode, node, IsUnusedTargets, IsScript, true);

                if (_isSkin)
                    DoProcessSkin(_refNode, node, IsUnusedBones, IsDualQuat, indicies);

                if (_isMapChannel)
                    DoProcessMapChannels(_refNode, node, SelectedMapChannels.Select(x => x.Channel).ToArray(), indicies);
                
            }
            MaxSDK.SetMaxSelection(nodes);
            LinkMax.RestoreCallbacks();
        }
        static public void DoProcessMorph(IINode refNode, IINode nToSetup, bool isUnusedTargets, bool isScript, bool createSkwIfNotExists, string[] channelNames = null)
        {
            try
            {
                List<IModifier> morphs = MaxSDK.GetModifiers(refNode).Where(x => MaxSDK.IsMorpher(x)).Select(x => x).ToList();
                morphs.Reverse();

                foreach (IModifier refM in morphs)
                {
                    if (MorphMod.WrapByProjectEngine(refM, refNode, nToSetup, createSkwIfNotExists, channelNames) is IModifier mph)
                    {
                        MorphMod mod = new MorphMod(mph, nToSetup);
                        if (isUnusedTargets)
                        {
                            mod.RemoveUnusedChannels(0.001f);
                        }
                        if (!isScript) 
                        {
                            mod.SetDefaultController();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        static public void DoProcessSkin(IINode refNode, IINode nToSetup, bool isUnusedBones, bool isDualQuat, List<int> mappedIndices)
        {
            try
            {
                List<IModifier> skins = MaxSDK.GetModifiers(refNode).Where(x => MaxSDK.IsSkin(x)).Select(x=>x).ToList();
                skins.Reverse();

                foreach (IModifier refM in skins)
                {
                    if (SkinMod.WrapToSkin(refM, refNode, nToSetup) is IModifier sk)
                    {
                        SkinMod mod = new SkinMod(sk, nToSetup);
                        if (isUnusedBones)
                        {
                            mod.RemoveUnusedBones();
                        }
                        if (isDualQuat)
                        {
                            SkinMod.ProjectDualQuat(refM, mod.Modifier, mappedIndices);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        static public void DoProcessMapChannels(IINode refNode, IINode nToSetup, int[] mapChannels , List<int> mappedIndices)
        {
            try
            {
                if (mapChannels.Count() > 0)
                {
                    MaxSDK.ToMaxScript(mapChannels, "mapChannels", false);
                    MaxSDK.ToMaxScript(mappedIndices, "mappedIndices", true);
                    MaxSDK.ToMaxScript(refNode, "refNodeProj");
                    MaxSDK.ToMaxScript(nToSetup, "targetNode");
                    MaxSDK.ExecuteMxs("ProjectionJob.ProjectMapChannels  refNodeProj targetNode mapChannels mappedIndices false");
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void DisplayVertexColor( bool onOff )
        {
            if (RefNode != null)
            {
                MaxSDK.ToMaxScript(RefNode, "n");
                MaxSDK.ExecuteMxs(string.Format("ProjectionJob.DisplayVertexColor n {0}", onOff));
                MaxSDK.RedrawViews(RefNode);
            }
        }
        public void Dispose()
        {
            RefNode = null;
            IsDetected = false;
            MapChannels.Clear();
        }
    }
}
