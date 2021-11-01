using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Max;
using Bubo;
using Microsoft.SqlServer.Server;

namespace Bubo
{
    public  class SkinData 
    {
        public virtual string NodeName { get; } = "";
        public virtual List<string> BoneNList { get; }= new List<string>();
        public virtual List<SkinVtx> Vertices { get; }= new List<SkinVtx>();
        public SkinData()
        { }
        public SkinData(SkinData b)
        {
            NodeName = b.NodeName;
            BoneNList = new List<string>(b.BoneNList);
            Vertices = new List<SkinVtx>(b.Vertices);
        }
        public SkinData (string filename , string skinNodeName )
        {
            try
            {
                if (File.Exists(filename))
                {
                    if (XElement.Load(filename) is XElement root && root.Attribute("name") is XAttribute rootAtt && rootAtt.Value is string rootName)
                    {
                        NodeName = rootName;
                        string basename = Tools.GetBasename(rootName);
                        Tools.Format(MethodBase.GetCurrentMethod(), rootName);
                        string newBasename = Tools.GetBasename(skinNodeName);
                        Tools.Format(MethodBase.GetCurrentMethod(), newBasename);

                        foreach (XElement el in root.Elements("BoneName"))
                        {
                            if (el.Attribute("name") is XAttribute att && att.Value is string name)
                            {
                                string nameInSk = Regex.Replace(name, basename , newBasename);
                                BoneNList.Add(nameInSk);
                                //Tools.Format(MethodBase.GetCurrentMethod() , string.Format("BoneList.Add {0}", nameInSk));
                            }
                        }
                        foreach (XElement xVtx in root.Elements("Vtx"))
                        {
                            if (xVtx.Attribute("id") is XAttribute xIdVtx && int.Parse(xIdVtx.Value) is int vtx)
                            {
                                float dualQ = 0f;
                                if (xVtx.Attribute("dualQ") is XAttribute xDualQ && float.Parse(xDualQ.Value) is float v)
                                {
                                    //Tools.Format(MethodBase.GetCurrentMethod(),"dualQ: " + dualQ.ToString());
                                    dualQ = v;
                                }
                                List<string> bones = new List<string>();
                                List<int> boneIds = new List<int>();
                                List<float> weights = new List<float>();
                                foreach (XElement xBone in xVtx.Elements("Bone"))
                                {
                                    if (xBone.Attribute("name") is XAttribute xname && xname.Value is string name)
                                    {
                                        bones.Add(Regex.Replace(name, basename, newBasename));
                                    }
                                    if (xBone.Attribute("weight") is XAttribute xWeight && float.Parse(xWeight.Value) is float weight)
                                    {
                                        weights.Add(weight);
                                    }
                                }
                                Vertices.Add(new SkinVtx(vtx, bones, weights, dualQ));
                                //Tools.Format(MethodBase.GetCurrentMethod(), string.Format("Vertices.Add {0}", vtx));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public bool Save(string filename)
        {
            try
            {
                XDocument doc = new XDocument();
                XAttribute xName = new XAttribute("name", NodeName );
                XAttribute xSceneName = new XAttribute("sceneName", MaxSDK.GetCurrentMaxFileName());
                XElement root = new XElement("Root", new XAttribute[] { xName, xSceneName });
                doc.Add(root);

                foreach ( string bname in BoneNList)
                {
                    XAttribute bone = new XAttribute("name", bname);
                    root.Add(new XElement("BoneName", new XAttribute[] { bone }));
                }
                foreach (SkinVtx vtx in Vertices )
                {
                    XAttribute id = new XAttribute("id", vtx.Vtx.ToString());
                    XAttribute dualQ = new XAttribute("dualQ", vtx.DualQ.ToString("0.0000"));
                    XElement vertex = new XElement("Vtx", new XAttribute[] { id, dualQ });
                    for (int i = 0; i < vtx.BonesN.Count; i++)
                    {
                        XAttribute bone = new XAttribute("name", vtx.BonesN[i]);
                        XAttribute weight = new XAttribute("weight", vtx.Weights[i].ToString("0.0000"));
                        vertex.Add(new XElement("Bone", new XAttribute[] { bone, weight }));
                    }
                    root.Add(vertex);
                }
                doc.Save(filename);
                return false;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public List<IINode> GetBoneNodeList()
        {
            return MaxSDK.GetINodeListbyName(BoneNList);
        }
        public static SkinData operator +(SkinData a , SkinData b)
        {
            try
            {
                SkinData r = new SkinData(a);
                foreach (string boneName in b.BoneNList.Except(r.BoneNList))
                {
                    r.BoneNList.Add(boneName);
                }
                for (int i = 0; i < b.Vertices.Count; i++)
                {
                    r.Vertices[i] += b.Vertices[i];
                }
                return r;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public static SkinData operator *(SkinData a, float val)
        {
            try
            {
                SkinData r = new SkinData(a);
                for (int i = 0; i < r.Vertices.Count; i++)
                {
                    r.Vertices[i] *= val;
                }
                return r;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

    }
}
