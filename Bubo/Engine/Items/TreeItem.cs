using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Autodesk.Max;

namespace Bubo
{
    public class TreeItem :  INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
            catch (Exception ex)
            {
                Tools.Print("TreeViewItemNotifyPropertyChangedException : " + ex.Message, DebugLevel.EXCEPTION);
            }
        }

        bool _isVisible = true;
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                NotifyPropertyChanged(nameof(IsVisible));
                NotifyId();

                if (Parent != null)
                {
                    Parent.NotifyPropertyChanged(nameof(HasChildrenVisible));
                }
            }
        }
        bool _isExpanded;
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                NotifyPropertyChanged(nameof(IsExpanded));
            }
        }
        bool _isItemSelected;
        public virtual bool IsItemSelected
        {
            get
            {
                return _isItemSelected;
            }
            set
            {
                _isItemSelected = value;
                NotifyPropertyChanged(nameof(IsItemSelected));
            }
        }
        private int _id;
        public int Id
        {
            get
            {
                return _id;
            }
        }

        public bool IsPair
        {
            get
            {
                return ( Id % 2 == 0);
            }
        }
        public bool IsParentLocked { get; }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }
        private TreeItem _parent;
        public TreeItem Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if(!IsParentLocked)
                {
                    if (_parent != null)
                    {
                        _parent.Children.Remove(this);
                        _parent.NotifyPropertyChanged(nameof(HasChildrenVisible));
                    }
                    if (value != null)
                    {
                        value.Children.Add(this);
                        value.NotifyPropertyChanged(nameof(HasChildrenVisible));
                    }
                    _parent = value;
                    NotifyPropertyChanged(nameof(Parent));
                    NotifyId();
                }
            }
        }
        public ObservableCollection<TreeItem> Children { get; } = new ObservableCollection<TreeItem>();

        public bool HasChildrenVisible
        {
            get
            {
                return Children.Any(x=>x.IsVisible);
            }
        }
        public TreeItem(string name)
        {
            Name = name;
        }
        public TreeItem(string name, bool isParentLocked)
            : this(name)
        {
            IsParentLocked = isParentLocked;
        }

        public TreeItem(string name, TreeItem parent)
            : this(name)
        {
            Parent = parent;
        }
        void NotifyId()
        {
            if (Parent != null)
            { 
                _id = Parent.Children.Where(x => x.IsVisible).ToList().IndexOf(this);
                NotifyPropertyChanged(nameof(Id));
                NotifyPropertyChanged(nameof(IsPair));
            }
        }
        public List<T> GetDescendents<T>() where T : TreeItem
        {
            List<T> descendents = new List<T>();
            foreach (TreeItem it in Children)
            {
                if (it is T t)
                {
                    descendents.Add(t);
                }
                descendents.AddRange( it.GetDescendents<T>());
            }
            return descendents;
        }
    }
}
