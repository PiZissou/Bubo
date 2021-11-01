using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace Bubo
{
    public partial class MorphMod
    {
        public static XElement SerializeChannel(IModifier m, int channelID, Vector3D[] baseObjPoints )
        {
            XAttribute xName = new XAttribute("name", GetName(m, channelID));
            XAttribute xID = new XAttribute("id", channelID);
            XElement xChannel = new XElement("Channel", new XAttribute[] { xName, xID });
            Vector3D[] channelPoints = GetChannelPoints(m, channelID);

            for (int i = 0; i < channelPoints.Count(); i++)
            {
                XAttribute xPointID = new XAttribute("id", i);
                XAttribute xX = new XAttribute("x", channelPoints[i].X.ToString());
                XAttribute xY = new XAttribute("y", channelPoints[i].Y.ToString());
                XAttribute xZ = new XAttribute("z", channelPoints[i].Z.ToString());

                Vector3D offset = channelPoints[i] - baseObjPoints[i];
                XAttribute xOffsetX = new XAttribute("x", offset.X.ToString());
                XAttribute xOffsetY = new XAttribute("y", offset.Y.ToString());
                XAttribute xOffsetZ = new XAttribute("z", offset.Z.ToString());

                XElement xPoint = new XElement("Point", new XAttribute[] { xPointID, xX, xY, xZ});
                XElement xPointOffset = new XElement("Offset", new XAttribute[] { xOffsetX, xOffsetY, xOffsetZ });

                xPoint.Add(xPointOffset);
                xChannel.Add(xPoint);
            }
            return xChannel;
        }
        public static bool DeserializeChannelName(XElement xChannel, out int channelID, out string channelName)
        {
            channelID = 0;
            channelName = "";
            try
            {
                if (xChannel.Attribute("id") is XAttribute xID && int.Parse(xID.Value) is int id)
                {
                    channelID = id;
                }
                if (xChannel.Attribute("name") is XAttribute xName && xName.Value is string name)
                {
                    channelName = name;
                }
                return true;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public static bool DeserializeChannelPoints(XElement xChannel, out List<Vector3D> channelPoints, out List<Vector3D> channelOffsets)
        {
            channelPoints = new List<Vector3D>();
            channelOffsets = new  List<Vector3D>();

            try
            {
                IEnumerable<XElement> xPoints = xChannel.Elements("Point");
                int numPoints = xPoints.Count();

                foreach (XElement xPoint in xPoints)
                {
                    XElement xOffset = xPoint.Element("Offset");
                    float[] fTab = new float[3] ;
                    float[] fOffsetTab = new float[3] ;
                    string[] xyzTab = new string[] { "x", "y", "z" };
                    for (int i = 0; i < 3; i++)
                    {
                        if (xPoint.Attribute(xyzTab[i]) is XAttribute att && float.Parse(att.Value) is float val)
                        {
                            fTab[i] = val;
                        }
                        if (xOffset is XElement && xOffset.Attribute(xyzTab[i]) is XAttribute offsetAtt)
                        {
                            if (float.Parse(offsetAtt.Value) is float offsetVal)
                            {
                                fOffsetTab[i] = offsetVal;
                                
                            }
                            else
                            {
                                fOffsetTab[i] = 0f;
                                Tools.Format(MethodBase.GetCurrentMethod(), string.Format("fOffsetTab, Pasring error at {0}, value {1}", i, offsetAtt.Value), DebugLevel.ERROR);
                            }
                        }
                    }
                    channelPoints.Add( MaxSDK.ToVector(fTab));
                    channelOffsets.Add(MaxSDK.ToVector(fOffsetTab));
                }
                return true;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public static bool SaveChannels( IModifier m , IINode mphNode, string filename, IEnumerable<string> channelNames)
        {
            try
            {
                if (mphNode is IINode && mphNode.ObjectRef.FindBaseObject() is IPolyObject poly)
                {
                    int numverts = poly.Mesh.Numv;
                    int[] channelIDs = new int[] { };
                    if (channelNames.Count() > 0)
                    {
                        ITab<int> tab = GetChannelByName(m, channelNames);
                        channelIDs = MaxSDK.ToCSharpIndexTab(tab);
                    }
                    else
                    {
                        channelIDs = GetValidChannels(m);
                    }
                    
                    XDocument doc = new XDocument();
                    XAttribute xNumPoints = new XAttribute("numPoints", numverts);
                    XAttribute xName = new XAttribute("name", mphNode.Name);
                    XAttribute xSceneName = new XAttribute("sceneName", MaxSDK.GetCurrentMaxFileName());
                    XElement xRoot = new XElement("Root", new XAttribute[]{ xName, xNumPoints, xSceneName});
                    doc.Add(xRoot);
                    Vector3D[] baseObjPoints = GetBaseObjectPoints(mphNode);

                    foreach (int channelID in channelIDs)
                    {
                        xRoot.Add( SerializeChannel(m, channelID, baseObjPoints));
                    }
                    doc.Save(filename);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public static bool LoadChannels(IModifier m, IINode mphNode, string[] filenames, float[] mixValues , string[] channelNames , bool clearChannels, bool keepTargetNodes )
        {
            try
            {
                if (mphNode is IINode && mphNode.ObjectRef.FindBaseObject() is IPolyObject poly)
                {
                    LinkMax.SuspendCallbacks();

                    int numverts = poly.Mesh.Numv;
                    int numMix = filenames.Count();
                    List<XElement> xRoots = new List<XElement>();

                    foreach (string filename in filenames)
                    {
                        if (!File.Exists(filename))
                        {
                            Tools.Format(MethodBase.GetCurrentMethod(), string.Format("File not Exists : {0} !", filename));
                        }
                        else if ( XElement.Load(filename) is XElement xRoot && xRoot.Attribute("numPoints") is XAttribute xNumPoints && int.Parse(xNumPoints.Value) is int numPoints)
                        {
                            if (numPoints != numverts)
                            {
                                Tools.Format(MethodBase.GetCurrentMethod(), "STOP : numPoints != numverts", DebugLevel.ERROR);
                            }
                            else
                            {
                                xRoots.Add( xRoot );
                            }

                        }
                        else
                        {
                            Tools.Format(MethodBase.GetCurrentMethod(), string.Format("STOP : File Loading Error : {0} !", filename), DebugLevel.ERROR);
                        }
                    }
                    if (xRoots.Count != numMix)
                    {
                        return false;
                    }

                    List<MorphData> morphDatas = new List<MorphData>();
                    MorphData.InitMixSession();
                    
                    foreach (XElement xRoot in xRoots)
                    {
                        List<ChannelData> channelsImp = new List<ChannelData>();
                        foreach (XElement xChannel in xRoot.Elements("Channel"))
                        {
                            if (DeserializeChannelName(xChannel, out int channelID, out string channelName))
                            {
                                if (channelNames.Count() == 0 || Array.Exists(channelNames, x => x.ToLower() ==  channelName.ToLower()))
                                {
                                    if (DeserializeChannelPoints(xChannel, out List<Vector3D> points, out List<Vector3D> offsets))
                                    {
                                        channelsImp.Add(new ChannelData(channelName, channelID, offsets.ToArray()));
                                    }
                                }
                                else
                                {
                                    Tools.Format(MethodBase.GetCurrentMethod(), string.Format("Channel exist, but not loaded by user : {0}", channelName));
                                }
                            }
                            else
                            {
                                Tools.Format(MethodBase.GetCurrentMethod(), "STOP : DeserializeChannel not working !", DebugLevel.ERROR);
    
                            }
                        }
                        morphDatas.Add ( new MorphData(channelsImp.ToArray()));
                    }
                    if (morphDatas.Count != numMix)
                    {
                        Tools.Format(MethodBase.GetCurrentMethod(), "morphDatas.Count != xRoots.Count", DebugLevel.ERROR);
                        return false;
                    }

                    if (clearChannels)
                    {
                        DeleteChannels(m, GetValidChannels(m));
                    }

                    for (int i = 0 ; i < morphDatas.Count ; i++)
                    {
                        morphDatas[i] *= mixValues[i];
                        if (i > 0)
                        {
                            Tools.Format(MethodBase.GetCurrentMethod(), "i > 0");
                            morphDatas[0] += morphDatas[i];
                        }
                    }
                    for ( int i= 0; i< morphDatas[0].Channels.Count();i++ )
                    {
                        Vector3D p = morphDatas[0].Channels[i].Offsets[0];
                        //Tools.Format(MethodBase.GetCurrentMethod(), string.Format("from Channels : {0}, offset : {1}", i , p.ToString()));
                    }

                    BuildChannels(m, mphNode, morphDatas[0].Channels, keepTargetNodes);
                    LinkMax.RestoreCallbacks();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        static IINode[] BuildChannels (IModifier m, IINode mphNode, ChannelData[] channels, bool keepTargetNodes)
        {
            try
            {
                MaxSDK.SetCurrentObject(m);
                MaxSDK.GetModEnabled(m, out bool enabled, out bool inView, out bool inRender);
                MaxSDK.SetModEnabled(m, true, true, true);

                List<IINode> newTargetNodes = new List<IINode>();
                foreach (ChannelData channel in channels)
                {
                    if (BuildChannelWithOffsets(m, mphNode, channel.Name, channel.Offsets) is IINode node)
                    {
                        newTargetNodes.Add(node);
                    }
                }
                MaxSDK.SetModEnabled(m, enabled, inView, inRender);

                if (!keepTargetNodes && newTargetNodes.Count > 0)
                {
                    MaxSDK.DeleteNodes(newTargetNodes);
                    return new IINode[0];
                }
                return newTargetNodes.ToArray();
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new IINode[0];
            }
        }
        static IINode BuildChannelWithOffsets(IModifier m, IINode mphNode, string channelName, Vector3D[] offsets)
        {
            try
            {
                int channelID = FindChannelIndex(m, channelName);
                return BuildChannelWithOffsets( channelID , m, mphNode, channelName, offsets);
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        static IINode BuildChannelWithOffsets(int channelID , IModifier m, IINode mphNode ,string channelName, Vector3D[] offsets)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "mph");
                MaxSDK.ToMaxScript(mphNode, "mphNode");
                IINode channelNode = ExtractBaseTarget(mphNode , channelName);

                if (channelNode is IINode && channelNode.ObjectRef.FindBaseObject() is IPolyObject poly)
                {

                    //Tools.Format(MethodBase.GetCurrentMethod(), string.Format("channelID : {0}, offset :{1} ", channelID, offsets.ToString()); 
                    channelNode.Name = channelName;
                    for (int i = 0; i < poly.Mesh.Numv; i++)
                    {
                        // *** don't use poly.Mesh.V(i).P.Add(offsets[i]) *** impredictible results ;
                        poly.Mesh.V(i).P.X += (float)offsets[i].X;
                        poly.Mesh.V(i).P.Y += (float)offsets[i].Y;
                        poly.Mesh.V(i).P.Z += (float)offsets[i].Z;

                    }
                    
                    if (channelID == -1 ){
                        Tools.Format(MethodBase.GetCurrentMethod(), "Cannot find valid index to build the channel!", DebugLevel.ERROR) ;
                        return null;
                    }
                    poly.Mesh.InvalidateGeomCache();
                    BuildFromNode(m, channelID, channelNode, false);
                    return channelNode;
                }
                return null;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public static ITab<int> GetChannelByName ( IModifier m , IEnumerable<string> channelNames )
        {
            try
            {
                MaxSDK.ToMaxScript(m, "mph");
                MaxSDK.ToMaxScript(channelNames, "channelNames");
                return MaxSDK.ExecuteMxs("(MorphJob.GetChannelByName mph channelNames)").ITab;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return MaxSDK.Global.Tab.Create<int>();
            }
        }
        public static int[] GetValidChannels(IModifier m)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "mph");
                ITab<int> tab = MaxSDK.ExecuteMxs("(for i = 1 to mph.numSubs where  WM3_MC_HasData  mph i collect i)").ITab;
                int[] channelIDs = MaxSDK.ToCSharpIndexTab(tab);
                return channelIDs;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new int[] { };
            }
        }
        public static int FindChannelIndex( IModifier m, string channelName, bool freeChannel = true)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "mph");
                MaxSDK.ToMaxScript(channelName, "channelName");
                int channelID = MaxSDK.ExecuteMxs(string.Format("MorphJob.FindChannelIndex mph channelName {1}", channelName, freeChannel)).I;
                return  channelID - 1;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return -1;
            }
        }
        public static bool DeleteChannels(IModifier m , IEnumerable<int> indices)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "mph");
                foreach (int index in indices)
                {
                    MaxSDK.ExecuteMxs(string.Format("WM3_MC_Delete mph {0}", index + 1));
                }
                return true;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public static bool BuildFromNode(IModifier mph, int ChannelID, IINode targetNode, bool setDefault)
        {
            try
            {
                MaxSDK.ToMaxScript(mph, "mph");
                MaxSDK.ToMaxScript(targetNode, "targetNode");
                return MaxSDK.ExecuteMxs(string.Format("MorphJob.BuildFromNode mph {0} targetNode setDefault:{1}", ChannelID + 1, setDefault )).B;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public static IINode ExtractBaseTarget(IINode mphNode, string channelName , string layerName = "0")
        {
            try
            {
                MaxSDK.ToMaxScript(mphNode, "mphNode");
                MaxSDK.ToMaxScript(layerName, "layerName");
                return MaxSDK.ExecuteMxs(string.Format("MorphJob.ExtractBaseTarget  mphNode  layerName")).N;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public static string GetName( IModifier m , int channelID)
        {
            try
            {
                MaxSDK.ToMaxScript(m , "mph");
                return MaxSDK.ExecuteMxs(string.Format("WM3_MC_GetName mph {0}", channelID + 1)).S;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return "";
            }
        }
        public static int GetChannelNumPoints ( IModifier m, int channelID)
        {
            try
            {
                MaxSDK.ToMaxScript(m, "mph");
                return MaxSDK.ExecuteMxs(string.Format("WM3_MC_NumPts mph {0}", channelID + 1)).I;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return 0;
            }
        }
        public static Vector3D[] GetChannelPoints(IModifier m , int channelID)
        {
            try
            {
                int numPoints = GetChannelNumPoints( m , channelID);
                Vector3D[] channelPoints = new Vector3D[numPoints];
                
                MaxSDK.ToMaxScript(m, "mph");
                for (int i = 0; i < numPoints; i++)
                {
                    // Warning WM3_MC_GetMorphPoint pointId is base0 in this max function.
                    ITab<float> fTab = MaxSDK.ExecuteMxs(string.Format("MorphJob.GetMorphPoint mph {0} {1}", channelID + 1 , i )).FTab;
                    channelPoints[i] = new Vector3D(fTab[0], fTab[1], fTab[2]);
                }
                return channelPoints.ToArray();
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return new Vector3D[] {};
            }
        }
        public static Vector3D[] GetBaseObjectPoints ( IINode mphNode)
        {
            return MaxSDK.ToVector((MaxSDK.GetPolyVerts(mphNode)).ToArray());
        }

        public static void CurrentToMaxDelta( IModifier m , IINode mphNode, int[] channelIDs, float delta )
        {
            try
            {
                Vector3D[] basePoints = GetBaseObjectPoints(mphNode);
                
                foreach (int channelID in channelIDs)
                {
                    Vector3D[] channelPoints = GetChannelPoints(m, channelID);
                    Vector3D[] points = new Vector3D[channelPoints.Count()];

                    for (int i = 0; i < basePoints.Count(); i++)
                    {
                        points[i]  = (channelPoints[i] - basePoints[i]) * delta;
                    }
                    IINode targetNode = BuildChannelWithOffsets(channelID, m, mphNode, GetName(m, channelID), points);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public static bool TransfertVertexPos( IINode sourceN, IINode destN)
        {
            try
            {
                MaxSDK.ToMaxScript(sourceN, "sourceN");
                MaxSDK.ToMaxScript(destN, "destN");
                return MaxSDK.ExecuteMxs(string.Format("PolyJob.TransfertVertexPos  sourceN  destN")).B;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
    }
}
