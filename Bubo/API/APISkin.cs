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
    /// API used with maxscript in 3dsmax.
    /// API for skin methods
    /// </summary>
    public partial class  API
    {
        public float CurrentSkinWeight
        {
            get
            {
                return Main.Skin.CurrentWeight;
            }
        }

        public SkinMod GetSkinMod(int modifierHandle, int nodeHandle)
        {
            if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
            {
                return new SkinMod(m, node);
            }
            return null;
        }

        public bool SaveSkin(int modifierHandle, int nodeHandle, string filename)
        {
            if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
            {
                return SkinMod.SaveSkin(m, node, filename);
            }
            return false;
        }
        public int[] GetSkinBones(int modifierHandle)
        {

            if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m )
            {
                    return MaxSDK.ToHandleArray( SkinMod.GetBones(m));
            }
            return new int[0];
        }
        public bool MixSkin(int modifierHandle, int nodeHandle, string[] filenames, float[] mix)
        {

            if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
            {
                return SkinMod.MixSkin(m, node, filenames, mix);
            }
            return false;
        }
        public bool LoadSkin(int modifierHandle, int nodeHandle, string fileName)
        {
            if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
            {
                return SkinMod.MixSkin(m, node, new string[] { fileName }, new float[] { 1 });
            }
            return false;   
        }

        public bool LoadSkin(int modifierHandle, int nodeHandle, string fileName, bool onlySelected)
        {

            if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
            {
                return SkinMod.MixSkin(m, node, new string[] { fileName }, new float[] { 1 }, onlySelected);
            }
            return false;
        }


        public bool LoadSkin(int modifierHandle, int nodeHandle, string fileName,  int[] verticesToSkin)
        {

            if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
            {
                return SkinMod.MixSkin(m, node, new string[] { fileName }, new float[] { 1 }, false, verticesToSkin);
            }
            return false;
        }

        void SetSkinWeight(IModifier m, IINode node, float val, bool addValue)
        {

            if (MaxSDK.IsSkin(m))
            {
                if (SkinMod.GetEditingDQ(m))
                {
                    SkinMod.SetDQWeightsSelected(m, val, addValue);
                }
                else
                {
                    SkinMod.SetWeights(m, node, SkinMod.GetSelectedBone(m), SkinMod.GetVertSel(m, node), val, addValue);
                }
            }
        }
        public void SetSkinWeight(float val, bool addValue)
        {

            if (MaxSDK.GetCurrentModifier() is IModifier m && MaxSDK.IsSkin(m) && MaxSDK.GetMaxSelection(0) is IINode node)
            {
                SetSkinWeight(m, node, val, addValue);
            }
        }
        public void SetSkinWeight(float val)
        {
            SetSkinWeight(val, false);
        }
        public void SkinWeightPlus()
        {
            float val = CurrentSkinWeight;
            SetSkinWeight(val, true);
        }
        public void SkinWeightMinus()
        {
            float val = CurrentSkinWeight * -1;
            SetSkinWeight(val, true);
        }
        public void SetSkinWeight(int modifierHandle, int nodeHandle, float val, bool addValue )
        {
            if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.IsSkin(m) && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
            {
                SetSkinWeight(m, node, val, addValue);
            }
        }
        public void HoldBoneToggle()
        {
            if (Main.Skin.CurrentSkin is SkinMod mod)
            {
                mod.HoldToggle();
            }
        }
        public void SkinGrow()
        {
            if (Main.Skin.CurrentSkin is SkinMod mod)
            {
                mod.Grow();
            }
        }
        public void SkinShrink()
        {
            if (Main.Skin.CurrentSkin is SkinMod mod)
            {
                mod.Shrink();
            }
        }
        public void SkinLoop()
        {
            if (Main.Skin.CurrentSkin is SkinMod mod)
            {
                mod.Loop();
            }
        }
        public void SkinRing()
        {
            if (Main.Skin.CurrentSkin is SkinMod mod)
            {
                mod.Ring();
            }
        }
        public void SkinNextBone()
        {
            Main.Skin.SelectNextBone();
        }

    }
}
