using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bubo
{
    public class RestoreSkinWeights : Autodesk.Max.Plugins.RestoreObj
    {
        IINode SkinNode { get; }
        IModifier Modifier { get; }
        SkinVtxWeights[] OldSet { get; }
        SkinVtxWeights[] NewSet { get; }
        public RestoreSkinWeights( IINode skinNode, IModifier m , SkinVtxWeights[] oldSet, SkinVtxWeights[] newSet)
        {
            SkinNode = skinNode;
            Modifier = m;
            OldSet = oldSet;
            NewSet = newSet;
        }

        void SetSkinVtxWeights( in SkinVtxWeights[] set )
        {
            for (int i = 0; i < set.Count(); i++)
            {
                SkinMod.SetVertexWeight(SkinNode, Modifier, set[i].Vtx, set[i].Bones, set[i].Weights);
            }
        }
        public override void Redo()
        {
            try
            {
                SetSkinVtxWeights( NewSet);
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public override void Restore(bool isUndo)
        {
            try
            {
                SetSkinVtxWeights(OldSet);
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex );
            }
        }
    }
}
