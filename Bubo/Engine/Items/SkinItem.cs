using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

namespace Bubo
{
    /// <summary>
    /// inherit from MaxItem
    /// used in Skin treeview items
    /// </summary>
    public class SkinItem : MaxItem
    {
        public IINode Bone{ get; }
        public override bool IsItemSelected
        {
            get
            {
                if (Main.Instance.IsDisposed)
                {
                    if (BuboMod is SkinMod skinmod)
                    {
                        return MaxSDK.IsEquals(skinmod.SelectedBone, Bone);
                    }
                    return false;
                }
                else
                {
                    return base.IsItemSelected;
                }
            }
            set
            {
                base.IsItemSelected = value;
            }
        }
        bool _isHold;
        public bool IsHold
        {
            get
            {
                if (Main.Instance.IsDisposed)
                {
                    return SkinMod.IsHoldBone(Bone); 
                }
                else
                {
                    return _isHold;
                }
                
            }
            set
            {
                _isHold = value;
                NotifyPropertyChanged(nameof(IsHold));
            }
        }
        public SkinItem(string name, IBuboMod mod, IINode bone)
             : base(name, mod)
        {
            Bone = bone;
            IsHold = SkinMod.IsHoldBone(Bone);
        }
        public SkinItem(string name, IBuboMod mod, IINode bone, int boneId)
            : this(name, mod, bone)
        {
            _maxIndex = boneId;
        }

        public override void Refresh()
        {
            Name = Bone.Name;
        }
    }
}
