using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bubo
{
    public class ListTree<T> : List<T>
    {
        public void RemoveTree(T t) 
        {
            if (t is TreeItem item)
            {
                item.Parent = null;
            }
            Remove(t);
        }
        public void ClearTree()
        {
            foreach (T t in this )
            {
                if (t is TreeItem item)
                {
                    item.Parent = null;
                }
            }
            Clear();
        }
    }
}
