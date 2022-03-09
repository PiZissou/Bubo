using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bubo
{
    /// <summary>
    /// used to store and perform operations of targetMorphs vertex positions   
    /// </summary>
    public class ChannelData
    {
        public string Name { get; }
        public int Id { get; }
        public Vector3D[] Offsets { get; }

        public ChannelData( string name, int id , Vector3D[] offsets )
        {
            Name = name;
            Id = id;
            Offsets = offsets;
        }
        public static ChannelData operator +(ChannelData a, ChannelData b)
        {
            for (int i = 0; i < a.Offsets.Count(); i++)
            {
                a.Offsets[i] += b.Offsets[i];
            }
            return a;
        }
        public static ChannelData operator +(ChannelData a, Vector3D[] points)
        {
            for (int i = 0; i < a.Offsets.Count(); i++)
            {
                a.Offsets[i] += points[i];
            }
            return a;
        }
        public static ChannelData operator *(ChannelData a, float val)
        {
            for (int i = 0; i < a.Offsets.Count(); i++)
            {
                a.Offsets[i] *= val;
            }
            return a;
        }
    }
}
