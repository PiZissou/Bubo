using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bubo
{

    public enum RedrawUIOption { Full, RefreshItems, RefreshMorphValues }
    public enum SelectionItem { Select, SelectMore, SelectToggle }
    public enum ProcessingOption { Indices, All }
    public enum Spinner { Min, Max }
    public enum SelectionEngine { Skin, Morph }
    public enum GetNameOptions { All, Numbers };
    public enum ModPanelNotification { InitLoad , AddLoad , SelfLoad , NotLoad , AutoLoadTrue , AutoLoadFalse , ExceptionLoad }
    public enum LoadModNotification { NULL_Input, NULL_CurrentMod, CLEAR_CurrentMod }
    public enum InModNotification { InMod , InModRedraw , NotMod , MaxItemCount , MaxItemSel, ExceptionInMod, SkinVertSel, MuteProperties }
    public class INI
    {
        public static string Version { get; set; } = "";
        public bool DisplayLog
        {
            get
            {
                return Tools.DisplayLog;
            }
            set
            {
                Tools.DisplayLog = value;
            }
        }
        public static string ConfigFile { get; set; } = "";
        public static string SkinDir { get; set; } = "";

        public static readonly string BuboLocalAppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TAT", "Bubo");
        public static readonly string[] VignetteExtensions = new string[] { ".png", ".PNG", ".jpg", ".JPG", ".jpeg", ".gif" };
        public static readonly string XMLFileFilter = "Xml file (*.xml)|*.xml";
        public static readonly string ImageFilter = "All supported graphics|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png|";
    }
}
