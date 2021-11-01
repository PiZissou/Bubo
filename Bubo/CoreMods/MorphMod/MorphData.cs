using Autodesk.Max;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Bubo
{
    public class MorphData
    {
        public ChannelData[] Channels { get; } = new ChannelData[] { };

        public static List<string> ChannelsMixSession { get; } = new List<string>();
        public MorphData ( ChannelData[] channels)
        {
            Channels = channels;
            GrowMixSession(channels);
        }
        public static void InitMixSession()
        {
            ChannelsMixSession.Clear();
        }
        static void GrowMixSession(ChannelData[] channels)
        {
            string[] channelNames = channels.Select(x => x.Name).ToArray();
            foreach ( string newChannel in channelNames.Except(ChannelsMixSession))
            {
                ChannelsMixSession.Add(newChannel);
                Tools.Format(MethodBase.GetCurrentMethod(), string.Format("Append new Channel : {0}", newChannel));
            }
        }
        public static MorphData operator +(MorphData a, MorphData b)
        {
            List<ChannelData> result = new List<ChannelData>();
            BitArray bresult = new BitArray(b.Channels.Count());
            
            foreach (ChannelData aChannel in a.Channels)
            {
                if ( Array.FindIndex(b.Channels, x => x.Name == aChannel.Name) is int bindex && bindex != -1)
                {
                    result.Add(aChannel + b.Channels[bindex]);
                    bresult.Set(bindex, true);
                }
                else
                {
                    result.Add(aChannel);
                }
            }
            for (int i = 0; i < b.Channels.Count(); i++)
            {
                if( !bresult[i])
                {
                    result.Add(b.Channels[i]);
                }
            }
            return new MorphData(result.ToArray());
        }
        public static MorphData operator +(MorphData a, Vector3D[] points)
        {
            for (int i = 0; i < a.Channels.Count(); i++)
            {
                a.Channels[i] = a.Channels[i] + points;
            }
            return a;
        }
        public static MorphData operator *(MorphData a, float val)
        {
            for (int i = 0; i < a.Channels.Count(); i++)
            {
                a.Channels[i] = a.Channels[i] * val;
            }
            return a;
        }
    }
}
