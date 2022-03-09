using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Autodesk.Max;

namespace Bubo
{
    /// <summary>
    /// inherit from BuboEngine  
    ///  - manage morph treeview actions 
    /// </summary>
    public class MorphEngine : BuboEngine
    {
        #region override properties

        public override IClass_ID ClassID { get; } = MaxSDK.MR3_CLASS_ID;
        public override TreeItem TreeRoot { get; } = new TreeItem("RootMorph", true);
        public override string KeyConfigRoot { get; } = "Morph";
        public override string KeyTreeViewCM { get; } = "TreeViewMorphCM";

        #endregion
        float _unusedTolerance = 0.01f;
        public float UnusedTolerance
        {
            get
            {
                return _unusedTolerance;
            }
            set
            {
                _unusedTolerance = value;
                NotifyPropertyChanged(nameof(UnusedTolerance));
            }
        }
        float _currentToMaxDelta = 50f;
        public float CurrentPercentToMax
        {
            get
            {
                return _currentToMaxDelta;
            }
            set
            {
                _currentToMaxDelta = value;
                NotifyPropertyChanged(nameof(CurrentPercentToMax));
            }
        }
        public List<int> SelectedIndices
        {
            get
            {
                if (CurrentMod != null)
                {
                    return CurrentMod.MaxItemSel.Select(x => x.MaxIndex).ToList();
                }
                else
                {
                    return new List<int>();
                }
            }
        }
        public List<int> Indices
        {
            get
            {
                return CurrentMod.MaxItems.Select(x => x.MaxIndex).ToList();
            }
        }
        public List<int> ProcessingIndices
        {
            get
            {
                if (Process == ProcessingOption.Indices)
                {
                    return SelectedIndices;
                }
                else
                {
                    return Indices;
                }
            }
        }
        ProcessingOption _process = ProcessingOption.All;
        public ProcessingOption Process
        {
            get
            {
                return _process;
            }
            set
            {
                _process = value;
                NotifyPropertyChanged(nameof(_process));
            }
        }
        bool _activeOnly = false;
        public bool ActiveOnly
        {
            get
            {
                return _activeOnly;
            }
            set
            {
                _activeOnly = value;
                NotifyPropertyChanged(nameof(ActiveOnly));
            }
        }

        bool _morphIncludeScript = false;
        public bool MorphIncludeScript
        {
            get
            {
                return _morphIncludeScript;
            }
            set
            {
                _morphIncludeScript = value;
                NotifyPropertyChanged(nameof(MorphIncludeScript));
            }
        }

        #region override methods
        public override IBuboMod CreateValidMod(IModifier m, IINode node)
        {
            base.CreateValidMod(m, node);
            if (MaxSDK.IsClassOf(m, ClassID))
            {
                return new MorphMod(m, node);
            }
            return null;
        }
        public override void OnTimeChanged()
        {
            if (CurrentMod != null)
            {
                CurrentMod.RedrawUI(RedrawUIOption.RefreshMorphValues);
            }
        }
        public override InModNotification InMod(IModifier m, IINode node)
        {
            try
            {
                InModNotification notify;
                if (base.InMod(m, node) == InModNotification.InMod)
                {
                    if (CurrentMod.Selected != _maxItemSelected)
                    {
                        notify = InModNotification.MaxItemSel;
                    }
                    else
                    {
                        CurrentMod.RedrawUI(RedrawUIOption.Full);
                        notify = InModNotification.InModRedraw;
                    }
                }
                else
                {
                    notify = InModNotification.NotMod;
                }
                Tools.Format(MethodBase.GetCurrentMethod(), notify.ToString());
                return notify;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return InModNotification.ExceptionInMod;
            }
        }
        #endregion
        public void ForceProcessInSelection(MorphItem item)
        {
            if (CurrentMod != null && CurrentMod is MorphMod mod)
            {
                List<TreeItem> sel = new List<TreeItem>(SelectedItems);
                if (!sel.Exists(x => x.Equals(item)))
                {
                    SelectItem(item, SelectionItem.Select, false);
                }
            }
        }

    }
}
