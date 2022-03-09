using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bubo
{
    /// <summary>
    /// used to detect symetrie in polyMesh 
    /// </summary>
    public class PolySym : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        bool _isDetected = false;
        public bool IsDetected
        {
            get
            {
                return _isDetected;
            }
            set
            {
                _isDetected = value;
                NotifyPropertyChanged(nameof(IsDetected));
            }
        }
        internal List<int> Pos { get; } = new List<int>();
        internal List<int> Neg { get; } = new List<int>();
        internal List<int> Mid { get; } = new List<int>();
        internal List<int> NotSym { get; } = new List<int>();
        int _count;
        public int Count
        {
            get
            {
                return _count;
            }
        }
        public int[] Positives
        {
            get
            {
                return MaxSDK.ToMaxIndexTab(Pos);               
            }
        }
        public int[] Negatives
        {
            get
            {
                return MaxSDK.ToMaxIndexTab(Neg);
            }
        }
        public int[] Middles
        {
            get
            {
                return MaxSDK.ToMaxIndexTab(Mid);
            }
        }
        public int[] NotSymmetrical
        {
            get
            {
                return MaxSDK.ToMaxIndexTab(NotSym);
            }
        }
        public float Precision { get; set; } = 0.001f;
        public float MiddlePrecision { get; set; } = 0.001f;

        IINode _refNode;
        public IINode RefNode
        {
            get
            {
                return _refNode;
            }
            set
            {
                _refNode = value;
                NotifyPropertyChanged(nameof(RefNode));
            }
        }
        public PolySym()
        {

        }

        public void MapIndices(IINode sourceIndices, IINode destIndices)
        {

        }
        public bool DetectMirror(IINode node)
        {
            RefNode = node;
            List<IPoint3> vertices = MaxSDK.GetPolyVerts(node);
            _count = vertices.Count;
            Mid.Clear();
            Pos.Clear();
            Neg.Clear();
            NotSym.Clear();

            Tools.Print("Detect Mirror. Precision : " + Precision, DebugLevel.VERBOSE);
            Tools.Print("Detect Mirror. MiddlePrecision : " + MiddlePrecision, DebugLevel.VERBOSE);

            for (int i = 0; i < vertices.Count; i++)
            {
                NotSym.Add(i);
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                IPoint3 vert = vertices[i];
                if (vert.X > Precision) // loop through one side
                {
                    for (int j = 0; j < vertices.Count; j++)
                    {
                        if (i != j)
                        {
                            IPoint3 vert2 = vertices[j];
                            float x = vert.X + vert2.X;
                            float y = vert.Y - vert2.Y;
                            float z = vert.Z - vert2.Z;
                            if (Math.Abs(x) < Precision && Math.Abs(y) < Precision && Math.Abs(z) < Precision)
                            {
                                Pos.Add(i);
                                Neg.Add(j);
                                NotSym.Remove(i);
                                NotSym.Remove(j);
                                break;
                            }
                        }
                    }
                }
                if (Math.Abs(vert.X) <= MiddlePrecision)
                {
                    Mid.Add(i);
                    NotSym.Remove(i);
                }
            }
            IsDetected = true;
            return true;
        }
        public void Dispose()
        {
            RefNode = null;
            IsDetected = false;
        }
    }
}
