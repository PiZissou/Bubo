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
            try
            {
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                    return new SkinMod(m, node);
                }
                return null;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        public bool SaveSkin(int modifierHandle, int nodeHandle, string filename)
        {
            try 
            {
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                   return SkinMod.SaveSkin(m, node, filename);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public int[] GetSkinBones(int modifierHandle)
        {
            try
            {
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m )
                {
                     return MaxSDK.ToHandleArray( SkinMod.GetBones(m));
                }
                return new int[0];
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new int[0];
            }
        }
        public bool MixSkin(int modifierHandle, int nodeHandle, string[] filenames, float[] mix)
        {
            try
            { 
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                    return SkinMod.MixSkin(m, node, filenames, mix);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool LoadSkin(int modifierHandle, int nodeHandle, string fileName)
        {
            try
            { 
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                    return SkinMod.MixSkin(m, node, new string[] { fileName }, new float[] { 1 });
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }

        public bool LoadSkin(int modifierHandle, int nodeHandle, string fileName, bool onlySelected)
        {
            try
            {
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                    return SkinMod.MixSkin(m, node, new string[] { fileName }, new float[] { 1 }, onlySelected);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }


        public bool LoadSkin(int modifierHandle, int nodeHandle, string fileName,  int[] verticesToSkin)
        {
            try
            {
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                    return SkinMod.MixSkin(m, node, new string[] { fileName }, new float[] { 1 }, false, verticesToSkin);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }

        void SetSkinWeight(IModifier m, IINode node, float val, bool addValue)
        {
            try
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
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetSkinWeight(float val, bool addValue)
        {
            try
            {
                if (MaxSDK.GetCurrentModifier() is IModifier m && MaxSDK.IsSkin(m) && MaxSDK.GetMaxSelection(0) is IINode node)
                {
                    SetSkinWeight(m, node, val, addValue);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SetSkinWeight(float val)
        {
            try
            {
                SetSkinWeight(val, false);
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
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
            try
            {
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.IsSkin(m) && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                    SetSkinWeight(m, node, val, addValue);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void HoldBoneToggle()
        {
            try 
            { 
                if (Main.Skin.CurrentSkin is SkinMod mod)
                {
                    mod.HoldToggle();
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SkinGrow()
        {
            try 
            {
                if (Main.Skin.CurrentSkin is SkinMod mod)
                {
                    mod.Grow();
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SkinShrink()
        {
            try
            { 
                if (Main.Skin.CurrentSkin is SkinMod mod)
                {
                    mod.Shrink();
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SkinLoop()
        {
            try
            { 
                if (Main.Skin.CurrentSkin is SkinMod mod)
                {
                    mod.Loop();
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SkinRing()
        {
            try
            { 
                if (Main.Skin.CurrentSkin is SkinMod mod)
                {
                    mod.Ring();
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void SkinNextBone()
        {
            try
            { 
                Main.Skin.SelectNextBone();
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }

    }
}
