using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubo
{
    public class SkinVtxWeights
    {
        public int Vtx { get; }
        public ITab<float> Weights { get; }
        public ITab<IINode> Bones { get; }

        public SkinVtxWeights ( int vtx , ITab<float> weights, ITab<IINode> bones)
        {
            Vtx = vtx;
            Weights = weights;
            Bones = bones;
        }
    }
}
