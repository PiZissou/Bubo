using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bubo
{
    public class MorphItem : MaxItem
    {
        public MorphMod CurrentMod{
            get
            {
                return BuboMod as MorphMod;
            }    
        }
        public override bool IsRight
        {
            get
            {
                return Name.Contains("D_");
            }
        }
        public override bool IsLeft
        {
            get
            {
                return Name.Contains("G_");
            }
        }
        bool _isActive = true;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                SetChActive(_isActive);
                NotifyPropertyChanged(nameof(IsActive));
            }
        }

        bool _hasTargets = true;
        public bool HasTargets
        {
            get
            {
                return _hasTargets;
            }
            set
            {
                _hasTargets = value;
                NotifyPropertyChanged(nameof(HasTargets));
            }
        }
        bool _isEditableValue = true;
        public bool IsEditableValue
        {
            get
            {
                return _isEditableValue;
            }
            set
            {
                _isEditableValue = value;
                NotifyPropertyChanged(nameof(IsEditableValue));
            }
        }
        float _value;
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (IsEditableValue)
                {
                    SetChValue(value);
                    _value = value;
                }
                else
                {
                    _value = GetChValue();
                }
                NotifyPropertyChanged(nameof(Value));
            }
        }
        bool _withAnimOff;
        public MorphItem(string name,IBuboMod mod, int maxIndex )
          : base(name, mod)
        {
            _maxIndex = maxIndex;
            Refresh();
        }

        public override void Refresh()
        {
            try
            {
                Name = GetName();
                IsEditableValue = CheckEditableValue();
                HasTargets = CheckHasTargets();

                _isActive = CheckIsActive();
                _value = GetChValue();

                NotifyPropertyChanged(nameof(IsActive));
                NotifyPropertyChanged(nameof(Value));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        public void RefreshValue()
        {
            try
            {
                _value = GetChValue();
                NotifyPropertyChanged(nameof(Value));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
        void SetChActive(bool val)
        {
            MaxSDK.ExecuteMxs(string.Format("WM3_MC_SetActive {0} {1} {2}", BuboMod.MxsModifier, MaxIndex + 1, val));
        }
        bool CheckIsActive()
        {
            try
            {
                return MaxSDK.ExecuteMxs(string.Format("WM3_MC_IsActive {0} {1}", BuboMod.MxsModifier, MaxIndex + 1)).B;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        bool CheckEditableValue()
        {
            try
            {
                bool isEditable = MaxSDK.ExecuteMxs(string.Format("MorphJob.IsEditableChannel {0} {1}", BuboMod.MxsModifier, MaxIndex + 1)).B;
                return isEditable;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public string SetNameFromTarget()
        {
            try
            {
                if (HasTargets)
                {
                    string targetName = MaxSDK.ExecuteMxs(string.Format("(WM3_GetProgressiveMorphNode {0} {1} 1).name", BuboMod.MxsModifier, MaxIndex + 1)).S;
                    SetName(targetName);
                    return targetName;
                }
                return "";
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return "";
            }
        }
        public string GetName()
        {
            try
            {
                return MaxSDK.ExecuteMxs(string.Format("WM3_MC_GetName {0} {1}", BuboMod.MxsModifier, MaxIndex + 1)).S;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return "";
            }
        }
        public bool SetName(string name)
        {
            try
            {
                Name = name;
                return MaxSDK.ExecuteMxs(string.Format("WM3_MC_SetName {0} {1} \"{2}\"", BuboMod.MxsModifier, MaxIndex + 1 , name)).B; 
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        bool CheckHasTargets()
        {
            try
            {
                return MaxSDK.ExecuteMxs(string.Format("WM3_GetProgressiveMorphNode {0} {1} 1 != undefined", BuboMod.MxsModifier, MaxIndex + 1)).B;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        void SetChValue(float val)
        {
                
            if (_withAnimOff )
            {
                MaxSDK.ExecuteMxs(string.Format("with anim off ( WM3_MC_SetValue {0} {1} {2} )", BuboMod.MxsModifier, MaxIndex + 1, val.ToString("0.0000")));
            }
            else
            {
                MaxSDK.ExecuteMxs(string.Format("WM3_MC_SetValue {0} {1} {2}", BuboMod.MxsModifier, MaxIndex + 1, val.ToString("0.0000")));
            }
        }
        float GetChValue()
        {
            try
            {
                float val = MaxSDK.ExecuteMxs(string.Format("WM3_MC_GetValue {0} {1}", BuboMod.MxsModifier, MaxIndex + 1)).F;
                val = (val < 0) ? 0 : val;
                return val;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return 0.0f;
            }
        }
    }
}
