using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bubo
{
    /// <summary>
    /// implement IBuboMod
    /// used to store 3dsmax morpher modifier and perform specialized methods
    /// </summary>
    public partial class MorphMod : IBuboMod
    {
        public IModifier Modifier { get; }
        public IINode Node { get; }
        public string MxsModifier { get; } = "buboMorphMod";
        public string MxsNode { get; } = "buboMorphNode";
        public ListTree<MaxItem> MaxItems { get; } = new ListTree<MaxItem>();
        public IEnumerable<MaxItem> MaxItemSel
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
                try
                {
                    return GetValidChannels().Count();
                }
                catch (Exception ex)
                {
                    Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                    return 0;
                }
            }
        }
        public int Selected
        {
            get
            {
                /* Get Selected Index from selectedItem because we cannot get it form 3dsmax */
                if (SelectedItem is MorphItem item)
                {
                    return SelectedItem.MaxIndex;
                }
                return 0;
            }
        }
        MorphItem _selectedItem;
        public MorphItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }
        }
        public bool UseLimits
        {
            get
            {
                try
                {
                    return ExecuteJob("mph.Use_Limits == 1").B;
                }
                catch (Exception ex)
                {
                    Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                    return true;
                }
            }
            set
            {
                try
                {
                    ExecuteJob(string.Format("mph.Use_Limits = {0}", (value)?1:0));
                }
                catch (Exception ex)
                {
                    Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                }
            }
        }
        public float SpinMin
        {
            get
            {
                try
                {
                    return ExecuteJob("mph.Spinner_Minimum").F;
                }
                catch(Exception ex)
                {
                    Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                    return 0.0f;
                }
            }
            set
            {
                try
                {
                    ExecuteJob(string.Format("mph.Spinner_Minimum = {0}", value));
                }
                catch (Exception ex)
                {
                    Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                }
            }
        }
        public float SpinMax
        {
            get
            {
                try
                {
                    return ExecuteJob("mph.Spinner_Maximum").F;
                }
                catch (Exception ex)
                {
                    Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                    return 0.0f;
                }
            }
            set
            {
                try
                {
                    ExecuteJob(string.Format("mph.Spinner_Maximum = {0}", value));
                }
                catch (Exception ex)
                {
                    Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                }
            }
        }
        public MorphMod(IModifier m, IINode node)
        {
            Modifier = m;
            Node = node;
            MaxSDK.ToMaxScript(Modifier, MxsModifier);
            MaxSDK.ToMaxScript(Node, MxsNode);

            foreach ( int i in GetValidChannels())
            {
                MaxItems.Add(new MorphItem(GetName(i), this, i));
            }
        }
        public void SelectMaxItem(MaxItem maxItem)
        {
            if (maxItem is MorphItem morph)
            {
                /* save selectedItem from MorphMod bscause we cannot get it form 3dsmax */
                _selectedItem = morph;
                SetChannelSel(morph.MaxIndex);
                SetChannelPos(morph.MaxIndex);
            }
        }
        public void RenameMaxItem(MaxItem maxItem , string newName)
        {
            if (maxItem is MorphItem morph)
            {
                morph.Name = newName;
                SetName(morph.MaxIndex , newName);
            }
        }
        public void RedrawUI(RedrawUIOption redraw)
        {
            if (redraw == RedrawUIOption.RefreshMorphValues)
            {
                foreach (MorphItem it in MaxItems)
                {
                    it.RefreshValue();
                }
            }
            else if (redraw == RedrawUIOption.Full || redraw == RedrawUIOption.RefreshItems)
            {
                foreach (MorphItem it in MaxItems)
                {
                    it.Refresh();
                }
            }
        }
        public void RedrawViews()
        {
            
        }
        public bool DeleteChannel(IEnumerable<int> indices)
        {
            DeleteChannels(Modifier, indices);
            return true;
        }
        public int[] GetValidChannels()
        {
            return GetValidChannels(Modifier);
        }
        public void SetDefaultController()
        {
            MaxSDK.ToMaxScript( Modifier, "mph");
            MaxSDK.ToMaxScript(GetValidChannels(), "indices", true );
            MaxSDK.ExecuteMxs("MorphJob.SetController mph (bezier_float()) indices");
        }
        public void SetActiveChannels( bool onOff)
        {
            foreach (MaxItem item in MaxItemSel)
            {
                (item as MorphItem).IsActive = onOff;
            }
            RedrawUI(RedrawUIOption.Full);
            SelectMaxItem(SelectedItem);
        }
        public void SetValueChannels( float val)
        {
            foreach (MaxItem item in MaxItemSel)
            {
                (item as MorphItem).Value = val;
            }
        }
        public float GetLimit( Spinner spin , int index )
        {
            return ExecuteJob(string.Format("{0} mph {1}", (spin == Spinner.Min) ? "WM3_MC_GetLimitMIN" : "WM3_MC_GetLimitMAX", index + 1)).F;
        }
        public bool SetLimit(Spinner spin , int index , float val )
        {
            return ExecuteJob(string.Format("{0} mph {1}", (spin == Spinner.Min) ? "WM3_MC_GetLimitMIN" : "WM3_MC_GetLimitMAX", index + 1, val.ToString("0.0000"))).B;
        }
        public string GetName(int index)
        {
            return ExecuteJob(string.Format("WM3_MC_GetName mph {0}", index + 1)).S;
        }
        public bool SetName(int index, string name)
        {
            Tools.Format(MethodBase.GetCurrentMethod(), index.ToString());
            if (ExecuteJob(string.Format("WM3_MC_SetName {0}  {1} \"{2}\"", MxsModifier, index + 1, name)).B)
            {
                SelectMaxItem(SelectedItem);
                return true;
            }
            return false;
        }
        public bool SetChannelSel(int index)
        {
            return ExecuteJob(string.Format("MorphJob.SetChannelSel mph {0}", index + 1)).B;
        }
        public bool SetChannelPos(int index)
        {
            return ExecuteJob(string.Format("MorphJob.SetChannelpos mph {0}", index + 1)).B;
        }
        public int GetFreeChannel()
        {
            return ExecuteJob("MorphJob.GetFreeChannel mph").I - 1;
        }
        public bool AddChannel(IINode target)
        {
            return AddChannels(new List<IINode>() { target });
        }
        public bool AddChannels(List<IINode> targets)
        {
            MaxSDK.ToMaxScript(targets, "targets");
            return ExecuteJob(string.Format("MorphJob.AddChannels mph targets")).B;
        }
        public bool ReplaceFromSel()
        {
            ExecuteJob(string.Format("MorphJob.ReplaceFromNodes mph (selection as array)"));
            return true;
        }
        public IFPValue ExecuteJob(string mxs)
        {
            MaxSDK.ToMaxScript(Modifier, "mph");
            MaxSDK.ToMaxScript(Node, "mphNode");
            return MaxSDK.ExecuteMxs(mxs);
        }
        public bool Compact()
        {
            ExecuteJob("MorphJob.Compact  mph");
            return true;
        }
        public bool RemoveUnusedChannels(float val)
        {
            ExecuteJob(string.Format("MorphJob.RemoveUnusedChannels  mph  mphNode {0}", val.ToString("0.0000")));
            return true;
        }
        public void ShowChannelController(List<int> indices)
        {
            MaxSDK.ToMaxScript(indices, "indices", true);
            MaxSDK.ExecuteMxs(string.Format("MorphJob.ShowController {0} {1}", MxsModifier, "indices"));
        }
        public void SelectTargets(List<int> indices)
        {
            MaxSDK.ToMaxScript(indices, "indices", true);
            MaxSDK.ExecuteMxs(string.Format("MorphJob.SelectTargets {0} {1}", MxsModifier, "indices"));
        }
        public void HideTargets(List<int> indices, bool isHidden)
        {
            MaxSDK.ToMaxScript(indices, "indices", true);
            MaxSDK.ExecuteMxs(string.Format("MorphJob.HideTargets {0} {1} {2}", MxsModifier, "indices", isHidden));
        }
        public bool Extract(List<int> indices)
        {
            MaxSDK.ToMaxScript(indices, "indices", true);
            ExecuteJob("MorphJob.ExtractMorph  mph  mphNode indices \"ExtractMorph\"");
            RedrawUI(RedrawUIOption.Full);
            return true;
        }
        public bool Offset(List<int> indices , IINode destNode )
        {
            if (indices.Count > 0 && destNode != null && !MaxSDK.IsEquals(destNode, Node))
            {
                MaxSDK.ToMaxScript(indices, "indices", true);
                MaxSDK.ToMaxScript(destNode, "destNode");
                ExecuteJob("MorphJob.OffsetMorph  mph  mphNode destNode indices \"OffsetMorph\"");
            }
            return true;
        }
        public IModifier WrapByMorphEngine (IEnumerable<int> indices, IINode destNode)
        {
            IModifier newMorph = Wrap(Modifier, Node, indices, destNode, false);
            return newMorph;
        }
        public static IModifier WrapByProjectEngine(IModifier sourceMph, IINode SourceNode, IINode destNode, bool createSkwIfNotExists, string[] channelNames)
        {
            int[] indices = GetValidChannels(sourceMph);
            if (channelNames != null && channelNames.Length > 0)
            {
                indices = indices.Where(x => channelNames.Any(y => MaxSDK.MatchPattern(GetName(sourceMph, x), y))).ToArray();
            }
            return Wrap(sourceMph, SourceNode, indices, destNode, createSkwIfNotExists);
        }
        public static IModifier Wrap( IModifier sourceMph, IINode SourceNode, IEnumerable<int> indices, IINode destNode, bool createSkwIfNotExists)
        {
            if (indices.Count() > 0 && destNode != null && !MaxSDK.IsEquals(destNode, SourceNode))
            {
                MaxSDK.ToMaxScript(indices, "indices", true);
                MaxSDK.ToMaxScript(sourceMph, "mph");
                MaxSDK.ToMaxScript(SourceNode, "mphNode");
                MaxSDK.ToMaxScript(destNode, "wrappedNode");
                return MaxSDK.ExecuteMxs(string.Format("MorphJob.WrappedMorph  mph  mphNode wrappedNode indices \"WrappedMorph\" createSkwIfNotExists:{0}", createSkwIfNotExists)).R as IModifier;
            }
            return null;
        }

        public bool Collapse(List<int> indices )
        {
            if (indices.Count > 0)
            {
                LinkMax.StartMute(500);
                MaxSDK.ToMaxScript(indices, "indices", true);
                ExecuteJob("MorphJob.CollapseMorph  mph  mphNode indices");
            }
            return true;
        }
        public List<int> GetSideIndices( string side , List<int> indices )
        {
            return indices.Where(index => MaxItems.Find(item => item.MaxIndex == index) is MorphItem x && Regex.IsMatch(x.Name, string.Format("^{0}[0-9]?_", side))).Select(index => index).ToList();
        }
        public void MirrorChannels(string side, List<int> indices)
        {
            List<int> sideIndices = GetSideIndices(side, indices);
            if (indices.Count > 0)
            {
                MaxSDK.ToMaxScript(sideIndices, "sideIndices", true);
                MaxSDK.ExecuteMxs(string.Format("MorphJob.MirrorMorph {0} {1} {2} \"{3}\" \"{4}\" ", MxsModifier, MxsNode, "sideIndices", side, "MirrorMorph"));
            }
        }
        public void CurrentToMaxDelta(List<int> indices , float percent )
        {
            if (indices.Count > 0)
            {
                float delta = 100 / percent;
                MorphMod.CurrentToMaxDelta(Modifier, Node, indices.ToArray(), delta);
            }
        }

        public void ResetVertexSel()
        {
            TransfertVertexPos(Node, MaxSDK.GetMaxSelection(0));
        }

        public void SaveXml()
        {
            if (MaxSDK.GetSaveFileName(caption: "BuboMorphSave", types: "Xml files(*.xml)|*.xml", historyCategory: "BuboMorphDirectories") is string filePath)
            {
                MaxSDK.ToMaxScript(Modifier, "mph");
                MaxSDK.ToMaxScript(filePath, "xmlFile");
                MaxSDK.ExecuteMxs("Forge.SaveAnimatableReferences #(gethandlebyanim mph) xmlFile true true");
            }
        }

        public void LoadXml(string replaceName, bool withScript)
        {
            if (MaxSDK.GetOpenFileName(caption: "BuboMorphSave", types: "Xml files(*.xml)|*.xml", historyCategory: "BuboMorphDirectories") is string filePath)
            {
                MaxSDK.ToMaxScript(Modifier, "mph");
                MaxSDK.ToMaxScript(filePath, "xmlFile");
                MaxSDK.ToMaxScript(replaceName, "replaceName");
                MaxSDK.ExecuteMxs(string.Format("Forge.LoadAnimatableReferences xmlFile #(gethandlebyanim mph) replaceName {0}", withScript));
            }
        }
    }
}
