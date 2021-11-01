using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace Bubo
{
    public partial class SkinMod
    {
        public static bool SaveSkin(IModifier m, IINode skinNode, string filename)
        {
            try
            {
                if (m != null && skinNode != null)
                {
                    SkinMod skData = new SkinMod(m, skinNode);
                    return skData.Save(filename);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public static bool MixSkin(IModifier m, IINode skinNode, string[] filenames, float[] blends, bool onlySelected = false, int[] verticesToMix = null)
        {
            try
            {
                if (m != null && skinNode != null && MaxSDK.GetSkin(m) is IISkin sk)
                {
                    Tools.Format(MethodBase.GetCurrentMethod(), "Begin!", DebugLevel.VERBOSE);
                    List<SkinData> skDatas = new List<SkinData>();
                    List<float> validBlends = new List<float>();

                    for (int i = 0; i < filenames.Count(); i++)
                    {
                        if (blends[i] > 0)
                        {
                            skDatas.Add(new SkinData(filenames[i], skinNode.Name));
                            validBlends.Add(blends[i]);
                        }
                    }
                    return CoreMixSkin(m, skinNode, skDatas.ToArray(), validBlends.ToArray() , onlySelected, verticesToMix);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex, "params( IModifier m, IINode skinNode, string[] filenames, float[] blends )");
                return false;
            }
        }
        public static bool CoreMixSkin(IModifier m, IINode skinNode, SkinData[] skDatas, float[] blends, bool onlySelected, int[] verticesToMix = null)
        {
            try
            {
                if (skDatas.Count() > 0)
                {
                    SkinMod skinM = new SkinMod(m, skinNode);
                    SkinData mixData = null;
                    for (int i = 0; i < skDatas.Count(); i++)
                    {
                        if (blends[i] > 0)
                        {
                            skDatas[i] *= blends[i];
                            if (mixData == null)
                            {
                                mixData = new SkinData(skDatas[i]);
                            }
                            else
                            {
                                mixData += skDatas[i];
                            }
                        }
                    }
                    return skinM.Load(mixData, onlySelected, verticesToMix);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex, "params( IModifier m, IINode skinNode, SkinData[] skDatas, float[] blends )");
                return false;
            }
        }
        public static IModifier WrapToSkin(IModifier sourceSk, IINode sourceNode, IINode wrappedNode)
        {
            try
            {
                MaxSDK.ToMaxScript(sourceSk, "sourceSk");
                MaxSDK.ToMaxScript(sourceNode, "sourceNode");
                MaxSDK.ToMaxScript(wrappedNode, "destNode");
                return MaxSDK.ExecuteMxs(string.Format("SkinJob.WrappedSkin  sourceSk sourceNode destNode")).R as IModifier;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public static IModifier AddSkin(List<IINode> nodes)
        {
            try
            {
                IModifier sk = null;
                foreach (IINode n in nodes)
                {
                    MaxSDK.ToMaxScript(n, "n");
                    sk = MaxSDK.ExecuteMxs(string.Format("SkinJob.AddNewSkin n ")).R as IModifier;
                }
                MaxSDK.SetCurrentObject(sk);
                return sk;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public static void HoldBone(IINode bone, bool onOff)
        {
            try
            {
                MaxSDK.ToMaxScript(bone, "n");
                MaxSDK.ExecuteMxs(string.Format("SkinJob.HoldBone n {0}", onOff));
                MaxSDK.RedrawViews(bone);
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public static bool IsHoldBone(IINode bone)
        {
            try
            {
                MaxSDK.ToMaxScript(bone, "n");
                IFPValue fpv = MaxSDK.ExecuteMxs(string.Format("SkinJob.IsHold n"));
                if (fpv.B is bool onOff)
                {
                    return onOff;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public static void DisplayHoldBones(IModifier m, bool onOff)
        {
            try
            {
                foreach (IINode n in GetBones(m))
                {
                    MaxSDK.ToMaxScript(n, "n");
                    MaxSDK.ExecuteMxs(string.Format("SkinJob.DisplayHoldBone n {0}", onOff));
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public static void RedrawViews( IModifier m )
        {
            MaxSDK.ToMaxScript(m, "sk");
            MaxSDK.ExecuteMxs("skinOps.Invalidate  sk  1");
        }
        public static bool SetVertexWeight(IINode skinNode, IModifier m, int vtx , ITab<IINode> bones, ITab<float> weights)
        {
            try
            {
                IISkinImportData skImport = MaxSDK.GetSkinImportData(m);
                bool success = skImport.AddWeights(skinNode, vtx, bones, weights);
                RedrawViews(m);
                return success;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public static float GetVertexDQWeight(IModifier m, int id)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "sk");
                return MaxSDK.ExecuteMxs(string.Format("skinJob.getVertexDQWeight sk {0}" , id + 1)).F;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return 0.0f;
            }
        }
        public static void SetVertexDQWeight(IModifier m, int id , float val)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "sk");
                MaxSDK.ExecuteMxs(string.Format("skinJob.setVertexDQWeight sk {0} {1}", id + 1 , val.ToString("0.000")));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public static void ProjectDualQuat(IModifier sourceMod, IModifier destMod, List<int> sourceIDs)
        {
            try
            {
                float[] values = new float[sourceIDs.Count];
                Tools.Format(MethodBase.GetCurrentMethod(), string.Format("sourceIDs.count = {0}, values.count() = {1}", sourceIDs.Count, values.Count()));

                MaxSDK.SetCurrentObject(sourceMod);
                for (int i = 0; i < sourceIDs.Count; i++)
                {
                    values[i] =  GetVertexDQWeight(sourceMod, sourceIDs[i]);
                }
                MaxSDK.SetCurrentObject(destMod);
                for (int i = 0; i < sourceIDs.Count; i++)
                {
                    SetVertexDQWeight(destMod, i, values[i]);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public static string  GetPropertyChanged (IModifier m )
        {
            try
            {
                MaxSDK.ToMaxScript(m, "sk");
                return MaxSDK.ExecuteMxs(string.Format("skinJob.GetPropertyChanged sk ")).S; 
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return "";
            }
        }
        public static void SaveProperties(IModifier m)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "sk");
                MaxSDK.ExecuteMxs(string.Format("skinJob.SaveProperties sk "));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public static bool IsMutedPropetyChanged(IModifier m)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "sk");
                return MaxSDK.ExecuteMxs(string.Format("skinJob.IsMutedPropetyChanged sk ")).B;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        private static bool AddToUndoRedo(IINode skinNode, IModifier m, SkinVtxWeights[] oldSet, SkinVtxWeights[] newSet)
        {
            try
            {
                bool res = false;
                MaxSDK.TheHold.Begin();

                if (MaxSDK.TheHold.Holding)
                {
                    MaxSDK.TheHold.Put(new RestoreSkinWeights(skinNode, m, oldSet, newSet));
                    res = true;
                }
                MaxSDK.TheHold.Accept("BuboSetWeights");
                return res;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        static public bool GetEditEnvelopes(IModifier m)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "sk");
                return MaxSDK.ExecuteMxs(string.Format("SkinJob.GetEditEnvelopes sk")).B;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        static public SkinVtx[] GetVertSel(IModifier m , IINode skinNode )
        {
            try
            {
                List<SkinVtx> sel = new List<SkinVtx>();
                
                if (!GetEditEnvelopes(m))
                {
                    return new SkinVtx[0];
                }

                if ( MaxSDK.GetSkin2(m) is IISkin2 skin2 && MaxSDK.GetSkin(m) is IISkin skin)
                {
                    IBitArray bitSel = MaxSDK.Global.BitArray.Create();
                    skin2.GetVertexSelection(skinNode, bitSel);

                    if (bitSel.AnyBitSet)
                    {
                        for (int i = 0; i < bitSel.Size; i++)
                        {
                            if (bitSel[i] == 1)
                            {
                                sel.Add(new SkinVtx(i, skin.GetContextInterface(skinNode), skin));
                            }
                        }
                    }
                }
                return sel.ToArray();
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new SkinVtx[0]; 
            }
        }
        static public bool GetEditingDQ(IModifier m)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "sk");
                return MaxSDK.ExecuteMxs(string.Format("SkinJob.GetEditingDQ sk")).B;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        static public void SetWeights( IModifier m , IINode skNode, IINode bone, SkinVtx[] vertices, float val, bool addWeight )
        {
            try
            {
                if ( bone != null && !IsHoldBone(bone))
                {
                    int numVtx = vertices.Count();
                    SkinVtxWeights[] oldSet = new SkinVtxWeights[numVtx];
                    SkinVtxWeights[] newSet = new SkinVtxWeights[numVtx];

                    for (int i = 0; i < numVtx; i++)
                    {
                        oldSet[i] = vertices[i].GetWeights();
                        vertices[i].SetWeightsHold(bone, val, addWeight);
                        newSet[i] = vertices[i].GetWeights();
                        SetVertexWeight(skNode, m, newSet[i].Vtx, newSet[i].Bones, newSet[i].Weights);
                    }
                    AddToUndoRedo(skNode, m, oldSet, newSet);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        static public void SetDQWeightsSelected( IModifier m, float v, bool addWeight)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "sk");
                if (addWeight)
                    MaxSDK.ExecuteMxs(string.Format("skinJob.AddVertexDQWeightSelected sk {0}", v.ToString("0.000")));
                else
                    MaxSDK.ExecuteMxs(string.Format("skinJob.SetVertexDQWeightSelected sk {0}", v.ToString("0.000")));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public static int GetNumBones(IModifier m)
        {
            try
            {
                if (m != null && MaxSDK.GetSkin(m) is IISkin sk)
                {
                    return sk.NumBones;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return 0;
            }
        }
        public static int GetSelectedId(IModifier m)
        {
            try
            {
                if (m != null && MaxSDK.GetSkin(m) is IISkin sk)
                {
                    int selectId = GetBones(m).FindIndex(x => x.Name == sk.GetBoneName(sk.SelectedBone));
                    return selectId;
                }
                return -1;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return -1;
            }
        }
        public static IINode GetSelectedBone(IModifier m)
        {
            try
            {
                if (m != null && MaxSDK.GetSkin(m) is IISkin sk)
                {
                    return GetBones(m).Find(x => x.Name == sk.GetBoneName(sk.SelectedBone));
                }
                return null;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public static void AddBones(IModifier m, List<IINode> bones)
        {
            try
            {
                if (MaxSDK.GetSkin(m) is IISkin sk && MaxSDK.GetSkinImportData(m) is IISkinImportData skImpData)
                {
                    AddBones(sk, skImpData, bones);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public static void AddBones(IISkin sk, IISkinImportData skImpData, List<IINode> bones)
        {
            Tools.Format(MethodBase.GetCurrentMethod(), "Begin");
            foreach (IINode n in bones.Except(GetBones(sk)))
            {
                Tools.Format(MethodBase.GetCurrentMethod(), string.Format("AddBoneEx {0}", n));
                skImpData.AddBoneEx(n, true);
            }
        }
        public static List<IINode> GetBones(IModifier m)
        {
            try
            {
                if (m != null && MaxSDK.GetSkin(m) is IISkin sk)
                {
                    return GetBones(sk);
                }
                return new List<IINode>();
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new List<IINode>();
            }
        }
        public static List<IINode> GetBones(IISkin sk)
        {
            List<IINode> skinBones = new List<IINode>();

            for (int i = 0; i < sk.NumBones; i++)
            {
                IINode node = sk.GetBone(i);
                skinBones.Add(node);
            }
            return skinBones;
        }
    }
}
