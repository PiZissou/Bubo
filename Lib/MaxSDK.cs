///==========================================
/// Title: Bubo - Setup Manager
/// Author: Pierre Lasbgnes
/// Date:  2012 - 2020
///==========================================
///

using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ManagedServices;
using System.Windows.Media.Media3D;

namespace Bubo
{
    public enum ViewportDisplayColor { Material, Wirecolor }

    /// <summary>
    /// @author: Pierre Lasbignes
    /// lib of 3dsmax sdk methods
    /// </summary>
    public class MaxSDK
    {
        static public IGlobal Global = GlobalInterface.Instance;
        static public IInterval Interval = Global.Interval.Create();
#if MAX_2022
        static public IInterface14 Interface = Global.COREInterface14;
#else
        static public IInterface17 Interface = Global.COREInterface19;
#endif
        static public IClass_ID SKIN_CLASSID = Global.Class_ID.Create(9815843, 87654);
        static public IClass_ID MR3_CLASS_ID = Global.Class_ID.Create(0x17bb6854, 0xa5cba2a3);
        static public IClass_ID EPOLY_CLASS_ID = Global.Class_ID.Create(469250957, 422535320);
        static public IClass_ID TriObjectClassID = Global.Class_ID.Create(0x0009, 0);
        static public IClass_ID MESHDEFORMPW_CLASS_ID = Global.Class_ID.Create(582466825, 1731904975);
        static public InterfaceID I_SKIN = (InterfaceID)0x00010000;
        static public InterfaceID I_SKIN2 = (InterfaceID)0x00030000;
        static public InterfaceID I_SKINIMPORTDATA = (InterfaceID)0x00020000;
        static public IInterface_ID MESHDEFORMPW_INTERFACE = Global.Interface_ID.Create(0xDE21A34f, 0x8A43E3D2);
        static public IInterface_ID EPOLY_INTERFACE = Global.Interface_ID.Create(0x092779, 0x634020);
        public static IHold TheHold = Global.TheHold;

        public static int CurrentTime
        {
            get
            {
                return Interface.Time;
            }
        }
        public static int CurrentFrame
        {
            get
            {
                return TimeFromTicks(CurrentTime);
            }
        }
        static MaxSDK()
        {

        }
        public static int TimeFromTicks(int t)
        {
            return t / Global.TicksPerFrame;
        }

        public static void SetViewportDisplay(ViewportDisplayColor display )
        {
            if (display == ViewportDisplayColor.Material)
            {
                ExecuteMxs("displayColor.shaded = #material");
            }
            if (display == ViewportDisplayColor.Wirecolor)
            {
                ExecuteMxs("displayColor.shaded = #object ");
            }
        }
        public static string GetCurrentMaxFileName()
        {
            return Interface.CurFileName;
        }
        public static string GetSaveFileName(string filename = null, string caption = null, string types = null, string historyCategory = null)
        {
            
            string mxsvar = "GetSaveFileName";
            if (filename != null)
            {
                mxsvar += String.Format(" filename:@\"{0}\"", filename);
            }
            if (caption != null)
            {
                mxsvar += String.Format(" caption:@\"{0}\"", caption);
            }
            if (types != null)
            {
                mxsvar += String.Format(" types:@\"{0}\"", types);
            }
            if (historyCategory != null)
            {
                mxsvar += String.Format(" historyCategory:@\"{0}\"", historyCategory);
            }
            if (filename == null && caption == null && types == null && historyCategory == null)
            {
                mxsvar = "GetSaveFileName()";
            }
            Tools.Format(MethodBase.GetCurrentMethod(), mxsvar);
            IFPValue fpv = ExecuteMxs(mxsvar);
            if (fpv.Type == ParamType2.String)
            {
                return fpv.S as string;
            }
            return null;
        }
        public static string GetOpenFileName(string filename=null , string caption=null, string types = null, string historyCategory = null)
        {
            string mxsvar = "getOpenFileName";
            if (filename != null )
            {
                mxsvar += String.Format(" filename:@\"{0}\"", filename);
            }
            if (caption != null)
            {
                mxsvar += String.Format(" caption:@\"{0}\"", caption);
            }
            if (types != null)
            {
                mxsvar += String.Format(" types:@\"{0}\"", types);
            }
            if (historyCategory != null)
            {
                mxsvar += String.Format(" historyCategory:@\"{0}\"", historyCategory);
            }
            if (filename == null && caption == null && types == null && historyCategory == null)
            {
                mxsvar = "getOpenFileName()";
            }
            IFPValue fpv = ExecuteMxs(mxsvar);
            if (fpv.Type == ParamType2.String)
            {
                return fpv.S as string;
            }
            return null;
        }
        static public void GetModEnabled ( IModifier m , out bool enabled, out bool inView, out bool inRender )
        {
            enabled = m.IsEnabled;
            inView = m.IsEnabledInViews;
            inRender = m.IsEnabledInRender;
        }
        static public void SetModEnabled(IModifier m,  bool enabled,  bool inView,  bool inRender)
        {
            if (enabled)
                m.EnableMod();
            else
                m.DisableMod();

            if (inView)
                m.EnableModInViews();
            else
                m.DisableModInViews();

            if (inRender)
                m.EnableModInRender();
            else
                m.DisableModInRender();
        }
        static public IModifier GetModifier(IINode nodeToSearch, IClass_ID cid, string name)
        {
            IIDerivedObject dobj = nodeToSearch.ObjectRef as IIDerivedObject;
            while (dobj != null)
            {
                int nmods = dobj.NumModifiers;
                for (int i = 0; i < nmods; i++)
                {
                    IModifier mod = dobj.GetModifier(i);
                    if ((mod.ClassID.PartA == cid.PartA) && (mod.ClassID.PartB == cid.PartB) && mod.Name == name)
                        return mod;
                }
                dobj = dobj.ObjRef as IIDerivedObject;
            }
            return null;
        }
        public static IEPoly GetIEPolyInterface(IINode node)
        {
            if (node is IINode && node.ObjectRef.FindBaseObject() is IPolyObject poly)
            {
                return (IEPoly)poly.GetInterface(EPOLY_INTERFACE);
            }
            return null;
        }
        public static IColor GetVertexColor( IINode node, IEPoly polyOp , int mapChannel , int vtx)
        {
            IntPtr ptr = new IntPtr();
            bool val=true;
            ToMaxScript(node, "n");
            ToMaxScript(new int[] { vtx }, "sel", true);
            ExecuteMxs("polyop.setVertSelection n sel");
            return polyOp.GetVertexColor(val, ptr, mapChannel, 1 , CurrentTime);
        }
        public static List<int> MapVertIndices(IINode sNode , IINode dNode , bool objectTm = true)
        {
            if (sNode is IINode && dNode is IINode)
            {
                List<int> indices = new List<int>();

                IPoint3[] sVertices = GetPolyVerts(sNode, objectTm).ToArray();
                IPoint3[] dVertices = GetPolyVerts(dNode, objectTm).ToArray();

                if (dVertices.Count() > 0)
                {
                    for (int i = 0; i < dVertices.Count(); i++)
                    {
                        indices.Add(GetClosestPoint(dVertices[i], sVertices));
                    }
                }
                return indices;
            }
            return new List<int>();
        }
        public static IPoint3 ToObjectTM(IINode node , IPoint3 point)
        {
            IMatrix3 mat =node.GetObjectTM(CurrentTime, Interval);
            mat.PreTranslate(point);
            return mat.Trans;
        }
        public static int GetClosestPoint(IPoint3 point, IPoint3[] points)
        {
            List<float> distances = new List<float>();
            float length = 0f;
            int closest = -1;
            for (int i = 0 ; i < points.Count(); i++)
            {
                float d = points[i].Subtract(point).Length;
                distances.Add(d);
                if (i == 0 || length > d)
                {
                    length = d;
                    closest = i;
                }
            }
            return closest;
        }
        public static int GetNumMapChannels(IINode node)
        {
            if (node is IINode && node.ObjectRef.FindBaseObject() is IPolyObject poly)
            {
                return poly.NumMapsUsed;
            }
            return 0;
        }
        public static List<int> GetMapChannels(IINode node)
        {
            List<int> maps = new List<int>();
            if (node is IINode && node.ObjectRef.FindBaseObject() is IPolyObject poly)
            {
                ToMaxScript(node, "sdknode");
                for (int i = 0; i < poly.NumMapsUsed; i++)
                {
                    if (MaxSDK.ExecuteMxs( string.Format("polyop.getMapSupport sdknode.baseobject {0}", i.ToString())).B)
                    {
                        Tools.Format(MethodBase.GetCurrentMethod(), i.ToString());
                        maps.Add(i);
                    }
                }
            }
            return maps;
        }
        public static List<IPoint3> GetPolyVerts(IINode node , bool objectTm = false )
        {
            if (node is IINode && node.ObjectRef is IObject && node.ObjectRef.FindBaseObject() is IPolyObject poly)
            {

                List<IPoint3> vertices = new List<IPoint3>();
                for (int i = 0; i < poly.Mesh.Numv; i++)
                {
                    IPoint3 p = poly.Mesh.V(i).P;
                    if (objectTm)
                    {
                        p = ToObjectTM(node, p);
                    }
                    vertices.Add(p);
                }
                return vertices;
            }
            return new List<IPoint3>();
        }
        public static List<IPoint3> GetMeshVerts(IINode node, bool ObjectTm = false)
        {
            IMesh mesh = GetMeshFromNode(node);
            List<IPoint3> res = new List<IPoint3>();
            if (mesh != null)
            {
                for (int i = 0; i < mesh.NumVerts; i++)
                {
                    IPoint3 p = mesh.GetVert(i);
                    if (ObjectTm)
                    {
                        p = ToObjectTM(node, p);
                    }
                    res.Add(p);
                }
            }
            return res;
        }
        public static IMesh GetMeshFromNode(IINode node)
        {
            IObjectState iState = node.EvalWorldState(CurrentTime, true);
            if (iState.Obj.ConvertToType(CurrentTime, TriObjectClassID) is ITriObject iTri)
            {
                return iTri.Mesh;
            }
            return null;
        }
        public static IObject CreateObject(uint classIDA, uint classIDB)
        {
            IClass_ID cid = Global.Class_ID.Create(classIDA, classIDB);
            if (Interface.CreateInstance(SClass_ID.Geomobject, cid) is IObject obj)
            {
                return obj;
            }
            return null;
        }
        public static IINode CreateNode(uint classIDA, uint classIDB)
        {
            if (CreateObject(classIDA, classIDB) is IObject obj && Interface.CreateObjectNode(obj) is IINode node)
            {
                return node;
            }
            return null;
        }
        static public bool ModifierExists( IModifier m , IINode nodeToSearch)
        {
            IMXS_Editor_Interface v = Global.MXS_Editor_Interface.Create();
            if (nodeToSearch != null && m != null)
            {
                IIDerivedObject dobj = nodeToSearch.ObjectRef as IIDerivedObject;
                while (dobj != null)
                {
                    int nmods = dobj.NumModifiers;
                    for (int i = 0; i < nmods; i++)
                    {
                        IModifier mod = dobj.GetModifier(i);
                        if (IsEquals(m, mod))
                        {
                            return true;
                        }

                    }
                    dobj = dobj.ObjRef as IIDerivedObject;
                }
            }
            return false;
        }
        static public IModifier GetModifier(IINode nodeToSearch, IClass_ID cid)
        {
            if (nodeToSearch == null)
                return null;
            IIDerivedObject dobj = nodeToSearch.ObjectRef as IIDerivedObject;

            while (dobj != null)
            {
                int nmods = dobj.NumModifiers;
                for (int i = 0; i < nmods; i++)
                {
                    IModifier mod = dobj.GetModifier(i);

                    if ((mod.ClassID.PartA == cid.PartA) && (mod.ClassID.PartB == cid.PartB))
                    {
                        return mod;
                    }
                }
                dobj = dobj.ObjRef as IIDerivedObject;
            }

            return null;
        }
        public static List<IModifier> GetModifiers(IINode node)
        {
            List<IModifier> res = new List<IModifier>();
            if (node.WSMDerivedObject is IIDerivedObject dobWSM)
            {
                res.AddRange(dobWSM.Modifiers.ToList());
            }
            // Don't know why but there are modifiers in modifiers ...
            IIDerivedObject derObj = node.ObjectRef as IIDerivedObject;
            while (derObj != null)
            {
                res.AddRange(derObj.Modifiers.ToList());
                derObj = derObj.ObjRef as IIDerivedObject;
            }
            return res;
        }
        public static IObject GetBaseObject(IINode n)
        {
            if (n != null && n.ObjectRef is IObject obj)
            {
                return obj.FindBaseObject();
            }
            return null;
        }
        public static void EnableModifier(IModifier m, bool? onOff, bool? inViews)
        {
            if (m != null)
            {
                if (onOff.HasValue)
                {
                    if (onOff.Value)
                    {
                        //m.EnableModInRender();
                        m.EnableMod();
                    }
                    else
                    {
                        //m.DisableModInRender();
                        m.DisableMod();
                    }
                }

                if (inViews.HasValue)
                {
                    if (inViews.Value)
                    {
                        m.EnableModInViews();
                    }
                    else
                    {
                        m.DisableModInViews();
                    }
                }
            }
        }
        public static void DeleteModifier(IINode n, IModifier m)
        {
            if (n != null && m != null)
            {
                MaxSDK.TheHold.Begin();
                Interface.DeleteModifier(n, m);
                MaxSDK.TheHold.Accept("Bubo Delete Morpher");
            }
        }

        public static void DeleteNodes(IEnumerable<IINode> nodes)
        {
            MaxSDK.Interface.DeleteNodes(ToNodeTab(nodes), true, true, false);
        }
        public static bool IsClassOf(IReferenceTarget m , IClass_ID classID)
        {
            if (m == null)
                return false;
            if ((m.ClassID.PartA == classID.PartA) && (m.ClassID.PartB == classID.PartB))
            {
                return true;
            } 
            
            return false;
        }
        public static bool IsMorpher(IModifier m)
        {
            return IsClassOf( m , MR3_CLASS_ID);
        }
        public static bool IsSkinWrap(IModifier m)
        {
            return IsClassOf(m, MESHDEFORMPW_CLASS_ID);
        }
        public static void ConvertToSkin(IModifier m)
        {
            if(IsSkinWrap(m) && m.GetInterface(MESHDEFORMPW_INTERFACE) is IIMeshDeformPWMod meshDeformOps )
            {
                meshDeformOps.ConvertToSkin(true);
            }
            if (m is IIMeshDeformPWMod ops)
            {
                ops.ConvertToSkin(true);
            }
        }
        public static bool IsSkin(IModifier m)
        {
            return IsClassOf(m, SKIN_CLASSID);
        }
        public static T GetInterfaceForSkin<T>(IModifier m, InterfaceID id)
        {
            if ((m.ClassID.PartA == SKIN_CLASSID.PartA) && (m.ClassID.PartB == SKIN_CLASSID.PartB))
            {
                if (m.GetInterface(id) is T sk)
                {
                    return (T)sk;
                }
                return default;
            }
            return default;
        }
        public static IISkin GetSkin(IModifier m )
        {
            return GetInterfaceForSkin<IISkin>( m , I_SKIN);
        }
        public static IISkin2 GetSkin2(IModifier m)
        {
            return GetInterfaceForSkin<IISkin2>(m, I_SKIN2);
        }
        public static IISkinImportData GetSkinImportData(IModifier m)
        {
            return GetInterfaceForSkin<IISkinImportData>(m, I_SKINIMPORTDATA);
        }
        public static IFPValue ExecuteMxs(string mxs, bool quietError = false)
        {
            IFPValue fpv = Global.FPValue.Create();
#if MAX_2022
                Global.ExecuteMAXScriptScript(mxs, Autodesk.Max.MAXScript.ScriptSource.NonEmbedded, quietError, fpv, quietError);
#else
            Global.ExecuteMAXScriptScript(mxs, quietError, fpv, quietError);
#endif
            return fpv;
        }
        public static List<IINode> GetMaxSelection()
        {
            IINodeTab tab = CreateNodeTab();
            Interface.GetSelNodeTab(tab);
            return ToNodeList(tab);
        }
        public static void SetMaxSelection (List<IINode> nodes)
        {
            Interface.SelectNodeTab(ToNodeTab(nodes), true, true);
        }
        public static IINode GetMaxSelection( int index )
        {
            List<IINode> sel = GetMaxSelection();
            if (index >= 0 && index < sel.Count)
            {
                return sel[index];
            }
            return null;
        }
        public static void AppendNodeTab(IINodeTab nodetab, ITab<IINode> nTab)
        {
            for (int i = 0; i < nTab.Count; i++)
            {
                nodetab.AppendNode(nTab[i], false, 1);
            }
        }
        public static List<IINode> ToNodeList(IValue nodesValue)
        {
            List<IINode> inodeList = new List<IINode>();
            if (!nodesValue._IsCollection)
            {
                return inodeList;
            }
            IFPValue FpReturn = Global.FPValue.Create();
            nodesValue.ToFpvalue(FpReturn);

            ITab<IINode> tab = FpReturn.NTab;

            for (int i = 0; i < tab.Count; i++)
            {
                if (tab[i] is IINode n)
                {
                    inodeList.Add(n);
                }
            }
            return inodeList;
        }
        public static List<IINode> ToNodeList(ITab<IINode> nodetab)
        {
            List<IINode> iNodes = new List<IINode>();

            for (int i = 0; i < nodetab.Count; i++)
            {
                iNodes.Add(nodetab[i]);
            }
            return iNodes;
        }
        public static List<IINode> ToNodeList(IINodeTab nodetab)
        {
            List<IINode> iNodes = new List<IINode>();

            for (int i = 0; i < nodetab.Count; i++)
            {
                iNodes.Add(nodetab[i]);
            }
            return iNodes;
        }
        public static List<IINode> ToNodeList(int[] handles)
        {
            List<IINode> res = new List<IINode>();
            for (int i = 0; i < handles.Length; i++)
            {
                if (GetAnimByHandle(handles[i]) is IINode n)
                {
                    res.Add(n);
                }
            }
            return res;
        }
        public static List<T> TabToList<T>(ITab<T> nodetab)
        {
            List<T> iNodes = new List<T>();
            for (int i = 0; i < nodetab.Count; i++)
            {
                iNodes.Add(nodetab[i]);
            }
            return iNodes;
        }
        public static ITab<IINode> ToTabNode(List<IINode> nodes)
        {
            return ToNodeTab(nodes);
        }
        public unsafe static ITab<float> ToTabFloat(float[] array)
        {
            fixed (float* pArray = array)
            {
                // pArray now has the pointer to the array. You can get an IntPtr
                //by casting to void, and passing that in.

                IntPtr intPtr = new IntPtr((void*)pArray);
                ITab<float> tab = Global.Tab.Create<float>();
                tab.Append(array.Count(), intPtr, 0);
                return tab;
            }
        }
        public static ITab<float> ToTabFloat(List<float> list)
        {
            ExecuteMxs("tmp=#()");
            foreach (float fl in list)
            {
                string mxs = string.Format("append tmp {0}", fl.ToString("0.0000"));
                //string mxs = string.Format("append tmp {0}" ,  fl.ToString("0.0000").Replace(",","."));
                Tools.Format(MethodBase.GetCurrentMethod(), mxs, DebugLevel.SILENCE);
                ExecuteMxs(mxs);
            }
            IFPValue fpv = ExecuteMxs("tmp");
            if (fpv.Type == ParamType2.FloatTab)
            {
                return fpv.FTab;
            }
            return Global.Tab.Create<float>();
        }
        public static IINodeTab CreateNodeTab()
        {
            return Global.INodeTab.Create();
        }
        public static IINodeTab ToNodeTab(IEnumerable<IINode> iNodes)
        {
            IINodeTab nodetab = CreateNodeTab();
            foreach (IINode n in iNodes)
            {
                nodetab.AppendNode(n, false, 1);
            }
            return nodetab;
        }
        public static IINodeTab ToNodeTab(ITab<IINode> iNodes)
        {
            IINodeTab nodetab = CreateNodeTab();
            for (int i = 0; i < iNodes.Count; i++)
            {
                nodetab.AppendNode(iNodes[i], false, 1);
            }
            return nodetab;
        }
        public static List<IINode> ToNodeList(IEnumerable<uint> handles)
        {
            List<IINode> iNodes = new List<IINode>();
            foreach (uint handle in handles)
            {
                if (GetINode(handle) is IINode inode)
                {
                    iNodes.Add(inode);
                }
            }
            return iNodes;
        }
        public static List<IINode> ToNodeList(uint[] handleTab)
        {
            List<IINode> iNodes = new List<IINode>();

            for (int i = 0; i < handleTab.Count(); i++)
            {
                IINode inode = GetINode((uint)i);
                if (inode != null)
                {
                    iNodes.Add(inode);
                }
            }
            return iNodes;
        }
        public static IINode GetINode(uint handle)
        {
            return Interface.GetINodeByHandle(handle);
        }
        public static IINode GetINodebyName (string name)
        {
            return Interface.GetINodeByName(name);
        }
        public static List<IINode> GetINodeListbyName(List<string> names)
        {
            List<IINode> nodes = new List<IINode>();
            foreach (string name in names)
            {
                if (MaxSDK.GetINodebyName(name) is IINode n)
                    nodes.Add(n);
            }
            return nodes;
        }
       
        public static Vector3D[] ToVector(IPoint3[] points)
        {
            Vector3D[] vects = new Vector3D[points.Count()];
            for (int i = 0; i < vects.Count(); i++)
            {
                vects[i] = ToVector(points[i]);
            }
            return vects;
        }
        public static Vector3D ToVector(IPoint3 p)
        {
            return new Vector3D(p.X, p.Y, p.Z);
        }
        public static Vector3D ToVector(float[] tab )
        {
            if (tab.Count()== 3)
            {
                return new Vector3D(tab[0], tab[1], tab[2]);
            }
            else
            {
                return new Vector3D();
            }
        }
        public static IINodeTab ToHandleTab(int[] handleTab)
        {
            IINodeTab nodetab = CreateNodeTab();
            for (int i = 0; i < handleTab.Count(); i++)
            {
                IINode inode = GetINode((uint)i);

                if (inode != null)
                {
                    nodetab.AppendNode(inode, false, 1);
                }
            }
            return nodetab;
        }
        public static int[] ToHandleArray(IEnumerable<IINode> nodes)
        {
            return nodes.Select(x => (int)GetHandleByAnim(x)).ToArray();
        }

        static int[] OffsetIntTab (IEnumerable<int> indices, int offset)
        {
            IEnumerator<int> it = indices.GetEnumerator();
            int[] intTab = new int[indices.Count()];
            for (int i = 0; i < intTab.Count(); i++)
            {
                it.MoveNext();
                intTab[i] = it.Current + offset;
            }
            return intTab;
        }
        public static int[] ToMaxIndexTab(IEnumerable<int> indices)
        {
            return OffsetIntTab(indices, 1);
        }
        public static int[] ToCSharpIndexTab(IEnumerable<int> indices)
        {
            return OffsetIntTab(indices, -1);
        }
        public static int[] ToCSharpIndexTab(ITab<int> indices)
        {
            return OffsetIntTab(TabToList(indices), -1);
        }
        public static UIntPtr GetHandleByAnim(IAnimatable obj)
        {
            return Global.Animatable.GetHandleByAnim(obj);
        }
        public static bool IsEquals(IAnimatable obj1 , IAnimatable obj2)
        {
            return Global.Animatable.GetHandleByAnim(obj1) == Global.Animatable.GetHandleByAnim(obj2);
        }
        public static IAnimatable GetAnimByHandle(int handle)
        {
            return Global.Animatable.GetAnimByHandle((UIntPtr)handle);
        }
        public static IINode PickNode()
        {
            IFPValue fpv = ExecuteMxs("pickObject()");
            if (fpv.Type == ParamType2.Inode)
            {
                return fpv.N;
            }
            return null;
        }
        public static void ToMaxScript(IAnimatable obj, string mxsVar)
        {
            UIntPtr handle = GetHandleByAnim(obj);
            string Mxs = string.Format("{0} = GetAnimByHandle {1}", mxsVar, handle);
            ExecuteMxs(Mxs, false);
        }
        public static void ToMaxScript(IEnumerable<IAnimatable> iNodes, string mxsVar)
        {
            string Mxs = string.Format("{0} = #()", mxsVar);
            ExecuteMxs(Mxs, false);
            foreach (IINode node in iNodes)
            {
                Mxs = string.Format("append {0} ( MaxOps.GetNodeByHandle {1})", mxsVar, node.Handle.ToString());
                ExecuteMxs(Mxs, false);
            }
        }
        public static void ToMaxScript(string var, string varName)
        {
            ExecuteMxs(string.Format("{0} = @\"{1}\"", varName, var), false);
        }
        public static void ToMaxScript(IEnumerable<int> datas, string mxsVar , bool indexForMax )
        {
            string Mxs = string.Format("{0} = #()", mxsVar);
            ExecuteMxs(Mxs, false);
            foreach (int i in datas)
            {
                Mxs = string.Format("append {0} ({1})", mxsVar, (indexForMax) ? i + 1 : i);
                ExecuteMxs(Mxs, false);
            }
        }
        public static void ToMaxScript(IEnumerable<string> datas, string mxsVar)
        {
            string Mxs = string.Format("{0} = #()", mxsVar);
            ExecuteMxs(Mxs, false);
            foreach (string data in datas)
            {
                Mxs = string.Format("append {0}  @\"{1}\"", mxsVar, data);
                ExecuteMxs(Mxs, false);
            }
        }
        static public IModifier GetCurrentModifier()
        {
            IFPValue fpv = ExecuteMxs("modPanel.getcurrentObject()");
            if (fpv.Type == ParamType2.Reftarg)
            {
                return fpv.R as IModifier;
            }
            return null;
        }
        static public IReferenceTarget Clone( IAnimatable obj )
        {
            ToMaxScript(obj , "CopiedObj");
            IFPValue fpv = ExecuteMxs("copy CopiedObj");
            if ( obj is IINode )
            {
                return fpv.N;
            }
            else if (fpv.Type == ParamType2.Reftarg)
            {
                return fpv.R as IReferenceTarget;
            }
            return null;
        }
        static public IModifier CreateMorpher(IINode node=null)
        {
            IModifier mod = ExecuteMxs("newMorpher = morpher()").R as IModifier;
            if(node!= null)
            {

            }
            return mod;
        }
        static public int GetSubObjectLevel()
        {
            return Interface.SubObjectLevel;
        }
        static public void SetCurrentObject(IReferenceTarget obj)
        {
            if (obj != null)
            {
                ToMaxScript(obj, "obj");
                ExecuteMxs("if modPanel.getcurrentObject() != obj then ( max Modify mode ; modPanel.setcurrentObject obj )");
            }
        }
        static public IReferenceTarget GetCurrentObject()
        {
            return ExecuteMxs( "modPanel.getcurrentObject()").R;
        }
        static public void RedrawViews(IReferenceTarget obj)
        {
            Interface.RedrawViews(MaxSDK.CurrentTime, RedrawFlags.Normal, obj);
        }
        static public void RedrawViews()
        {
            Interface.ForceCompleteRedraw(true);
        }

        static public bool MatchPattern(string s, string pattern)
        {
            ToMaxScript(s, "input");
            ToMaxScript(pattern, "pattern");
            return ExecuteMxs("matchPattern input pattern:pattern").B;
        }
    }
}
