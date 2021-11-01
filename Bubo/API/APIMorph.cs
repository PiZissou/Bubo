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
    public partial class API
    {
        public bool SaveMorphChannels(int modifierHandle, int nodeHandle, string filename, string[] channelNames)
        {
            try 
            {
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                    return MorphMod.SaveChannels( m, node, filename, channelNames);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool LoadMorphChannels(int modifierHandle, int nodeHandle, string filename ,string[] channelNames, bool clearChannels, bool keepTargetNodes)
        {
            try
            {

                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                    return MorphMod.LoadChannels(m, node, new string[] { filename }, new float[] { 1.0f }, channelNames, clearChannels, keepTargetNodes);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool MixMorphChannels(int modifierHandle, int nodeHandle, string[] filenames, float[] mixValues, string[] channelNames, bool clearChannels, bool keepTargetNodes)
        {
            try 
            {
                if (MaxSDK.GetAnimByHandle(modifierHandle) is IModifier m && MaxSDK.GetAnimByHandle(nodeHandle) is IINode node)
                {
                    return MorphMod.LoadChannels(m, node, filenames, mixValues, channelNames, clearChannels, keepTargetNodes);
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
    }
}
