using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bubo
{
    public class UISettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

#if MAX_2020
        public static readonly string SettingsPath = Path.Combine(INI.BuboLocalAppDataFolder, "Settings.2020.xml");
#elif MAX_2021
        public static readonly string SettingsPath = Path.Combine(INI.BuboLocalAppDataFolder, "Settings.2021.xml");
#elif MAX_2022
        public static readonly string SettingsPath = Path.Combine(INI.BuboLocalAppDataFolder, "Settings.2022.xml");
#endif

        private List<Type> _settingsType = new List<Type>() { typeof(PropertyChangedEventHandler)};
        private List<string> _notSaveSettings = new List<string>() {};

        static UISettings _instance;
        public static UISettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UISettings();
                }
                return _instance;
            }
        }

        bool _displayLog = true;
        public bool DisplayLog
        {
            get
            {
                return _displayLog;
            }
            set
            {
                _displayLog = value;
                NotifyPropertyChanged(nameof(DisplayLog));
            }
        }

        bool _displayVersion = true;
        public bool DisplayVersion
        {
            get
            {
                return _displayVersion;
            }
            set
            {
                _displayVersion = value;
                NotifyPropertyChanged(nameof(DisplayVersion));
            }
        }
        bool _smartMode = true;
        public bool SmartMode
        {
            get
            {
                return _smartMode;
            }
            set
            {
                _smartMode = value;
                NotifyPropertyChanged(nameof(SmartMode));
            }
        }
        bool _refreshOnTime = true;
        public bool RefreshOnTime
        {
            get
            {
                return _refreshOnTime;
            }
            set
            {
                _refreshOnTime = value;
                NotifyPropertyChanged(nameof(RefreshOnTime));
            }
        }

        private UISettings()
        {

        }

        public void SaveSettings()
        {
            try
            {
                string folderPath = Path.GetDirectoryName(SettingsPath);
                if (File.Exists(SettingsPath) || (Directory.Exists(Path.GetPathRoot(folderPath)) && Directory.CreateDirectory(folderPath) != null))
                {
                    XDocument docX = new XDocument();
                    XElement settingsX = new XElement("Settings");
                    foreach (PropertyInfo setting in GetType().GetProperties().Where(x => !_settingsType.Contains(x.PropertyType) && !x.GetAccessors()[0].IsStatic))
                    {
                        if (!_notSaveSettings.Contains(setting.Name))
                        {
                            XElement settingX = new XElement("Setting");
                            settingX.Add(new XAttribute(setting.Name, setting.GetValue(this)));
                            settingsX.Add(settingX);
                        }
                    }
                    docX.Add(settingsX);
                    docX.Save(SettingsPath);
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath) && Path.GetExtension(SettingsPath) == ".xml")
                {
                    XElement settingsX = XElement.Load(SettingsPath);
                    IEnumerable<PropertyInfo> properties = GetType().GetProperties().Where(x => !_settingsType.Contains(x.PropertyType) && !x.GetAccessors()[0].IsStatic);
                    foreach (XElement settingX in settingsX.Elements("Setting"))
                    {
                        if (settingX.FirstAttribute is XAttribute attribute)
                        {
                            string settingName = attribute.Name.ToString();
                            string settingValue = attribute.Value;
                            if (properties.FirstOrDefault(x => x.Name == settingName) is PropertyInfo info)
                            {
                                if (TypeDescriptor.GetConverter(info.PropertyType).ConvertFromString(settingValue) is object o)
                                {
                                    info.SetValue(this, o);
                                }
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


        public void NotifyPropertyChanged(String propName)
        {
            try
            {
                SaveSettings();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }
        }
    }
}
