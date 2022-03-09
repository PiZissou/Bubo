using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bubo
{
    /// <summary>
    /// store skin data for single vertex
    /// - list of bones
    /// - list of weights
    /// - DualQuat value
    /// implement operator+ 
    /// implement operator* 
    /// </summary>
    public class SkinVtx
    {
        public int Vtx { get; }
        public List<string> BonesN { get; } = new List<string>();
        public List<IINode> Bones { get; } = new List<IINode>();
        public List<float> Weights { get; } = new List<float>();
        public List<WeightItem> WeightsData { get; } = new List<WeightItem>();
        public List<WeightItem> DualQData { get; } = new List<WeightItem>();
        public float DualQ { get; set; }
        public IISkinContextData SkContext { get; }
        public IISkin Skin { get; }

        public SkinVtx(int id, List<string> boneNames, List<float> weights, float dualQ )
        {
            Vtx = id;
            BonesN = boneNames;
            Weights = weights;
            DualQ = dualQ;
        }
        public SkinVtx(int id, IISkinContextData skContext, IISkin skin)
        {
            Vtx = id;
            SkContext = skContext;
            Skin = skin;
            GetData();
        }
        public SkinVtxWeights GetWeights()
        {
            return new SkinVtxWeights(Vtx, MaxSDK.ToTabFloat(Weights.ToArray()), MaxSDK.ToTabNode(GetBoneNodes()));
        }
        public void GetData()
        {
            if (SkContext is IISkinContextData && Skin is IISkin)
            {
                Bones.Clear();
                BonesN.Clear();
                Weights.Clear();
                WeightsData.Clear();
                DualQ = SkContext.GetDQBlendWeight(Vtx);

                for (int k = 0; k < SkContext.GetNumAssignedBones(Vtx); k++)
                {
                    WeightItem wItem = new WeightItem
                    {
                        Vtx = Vtx,
                        BoneWeightId = k,
                        BoneId = SkContext.GetAssignedBone(Vtx, k)
                    };
                    wItem.Bone = Skin.GetBone(wItem.BoneId);
                    wItem.Weight = SkContext.GetBoneWeight(Vtx, k);
                    wItem.DualQ = DualQ;
                    if (wItem.Bone != null)
                    {
                        Bones.Add(wItem.Bone);
                        BonesN.Add(wItem.Bone.Name);
                    }
                    Weights.Add(SkContext.GetBoneWeight(Vtx, k));
                    WeightsData.Add(wItem);
                }
            }
        }
        public void SetWeightsHold( IINode bone , float val , bool addWeight)
        {
            int boneId = BonesN.IndexOf(bone.Name);
            if (boneId == -1)
            {
                if ( val <= 0 )
                {
                    return;
                }
                else
                {
                    boneId = BonesN.Count;
                    BonesN.Add( bone.Name );
                    Weights.Add(0.0f);
                }
            }

            float holded = 0.0f;
            float unHolded = 0.0f;
            List<int> freeIndices = new List<int>();

            for (int i = 0; i < Bones.Count; i++)
            {
                if (SkinMod.IsHoldBone(Bones[i]))
                {
                    holded += Weights[i];
                }
                else if (i != boneId)
                {
                    freeIndices.Add(i);
                    unHolded += Weights[i];
                }
            }
               
            if (freeIndices.Count == 0)
                return;

            float free = 1.0f - holded;
            float boneWeight = (addWeight) ? Weights[boneId] + val : val;

            boneWeight = (boneWeight > free) ? free : boneWeight;
            boneWeight = (boneWeight < 0) ? 0.0f : boneWeight;

            float added = boneWeight - Weights[boneId];
            Weights[boneId] = boneWeight;

            float dispatch = added / freeIndices.Count;
            float clamp = holded + boneWeight;
                
            foreach ( int i in freeIndices )
            {
                if (clamp < 1)
                {
                    float currWeight = Weights[i] - dispatch;
                    currWeight = (currWeight < 0) ? 0.0f : currWeight;
                    float backup = clamp;
                    clamp += currWeight;
                    if (clamp > 1)
                    {
                        Weights[i] = 1 - backup;
                        clamp = 1;
                    }
                    else
                    {
                        Weights[i] = currWeight;
                    }
                }
                else
                {
                    Weights[i] = 0.0f;
                }
            }
        }
        public static SkinVtx operator +(SkinVtx a, SkinVtx b)
        {
            SkinVtx r = new SkinVtx(a.Vtx, a.BonesN, a.Weights, a.DualQ);

            for (int i = 0; i < b.Weights.Count; i++)
            {
                string bName = b.BonesN[i];
                float bWeight = b.Weights[i];

                int rId = r.BonesN.IndexOf(bName);
                if (rId >= 0)
                {
                    r.Weights[rId] += bWeight;
                }
                else
                {
                    r.BonesN.Add(bName);
                    r.Weights.Add(bWeight);
                }
            }
            r.DualQ += b.DualQ;
            return r;
        }
        public static SkinVtx operator *(SkinVtx a, float val)
        {
            SkinVtx r = new SkinVtx(a.Vtx, a.BonesN, a.Weights, a.DualQ);
            val = (val < 0) ? 0 : val;
            val = (val > 1) ? 1 : val;
            for (int j = 0; j < r.Weights.Count; j++)
            {
                r.Weights[j] *= val;
            }
            r.DualQ *= val;
            return r;
        }
        public List<int> GetIndices(List<string> boneList)
        {
            List<int> indices = new List<int>();
            List<int> badIndices = new List<int>();

            for (int i = 0; i < indices.Count(); i++)
            {
                indices.Add(boneList.IndexOf(BonesN[i]));
                if (indices[i] == -1)
                {
                    badIndices.Add(i);
                }
            }
            if (badIndices.Count == 0)
            {
                return indices;
            }
            else if (badIndices.Count == indices.Count)
            {
                return null;
            }
            else
            {
                int v = indices.Find(x => x != -1);
                foreach (int index in badIndices)
                {
                    indices[index] = v;
                }
                return indices;
            }
        }
        public List<IINode> GetBoneNodes()
        {
            return MaxSDK.GetINodeListbyName(BonesN);
        }
        public override string ToString()
        {
            string s = string.Format("[Vtx {0}]\n", Vtx);
            float total = 0.0f;
            for (int i = 0; i<BonesN.Count; i++)
            {
                total += Weights[i];
                s += string.Format("\tBone:{0} ,Weight:{1}", BonesN[i] , Weights[i]);
            }
            s += string.Format("\tDualQuat {0}\n", DualQ);
            return string.Format ( "{0} / Total: {1}", s , total.ToString("0.0000"));
        }
    }
}
