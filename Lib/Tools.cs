using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace Bubo
{
    public enum DebugLevel { SILENCE, EXCEPTION, ERROR, INFO, WARNING, VERBOSE, ULTRAVERBOSE }

    public static partial class Tools
    {
        static Regex _rgx = new Regex("");
        static public IGlobal Global = GlobalInterface.Instance;
        public static bool DisplayLog { get; set; } = true;
        public static MethodBase CurrentMethod { get; set; }
        public static T VisualUpwardSearch<T>(DependencyObject source) where T : DependencyObject
        {
            try
            {
                while (source != null && !(source is T))
                    source = VisualTreeHelper.GetParent(source);

                return (source as T);
            }
            catch (Exception ex)
            {
                Print("VisualUpwardSearchException : " + ex.Message, DebugLevel.EXCEPTION);
                return null;
            }
        }
        public static void PrintCollection<T>(IEnumerable<T> collection, string title, DebugLevel level = DebugLevel.INFO)
        {
            try
            {
                if (title != "")
                {
                    Print(title, level);
                }
                foreach (var item in collection)
                {
                    if (item is IINode n)
                    {
                        Print("      " + n.Name, level);
                    }
                    else
                    {
                        Print("      " + item.ToString(), level);
                    }
                }
            }
            catch (Exception ex)
            {
                Print("PrintCollectionException : " + ex.Message, DebugLevel.EXCEPTION);
            }
        }
        public static string CleanStringForMxs(string s)
        {
            try
            {
                s = s.Replace("\\", "\\\\");
                s = s.Replace("\"", "'");
                s = s.Replace("&gt;", ">");
                s = s.Replace("&lt;", "<");
                s = s.Replace("&amp;", "&");
                return s;
            }
            catch (Exception ex)
            {
                Print("CleanStringForMxsException : " + ex.Message, DebugLevel.EXCEPTION);
                return s;
            }
        }
        public static void Format(MethodBase method, string msg , DebugLevel level = DebugLevel.VERBOSE)
        {
            Format( method.DeclaringType.Name , method.Name, msg , level);
        }
        public static void Format(string declaringType, string methodName, string msg, DebugLevel level = DebugLevel.VERBOSE)
        {
            Print(String.Format("[{0}].{1} : {2}", declaringType, methodName, msg), level);
        }
        public static void FormatException(MethodBase method, Exception ex, string infos = "")
        {
            Print(String.Format("[{0}].{1}Exception : {2} {3}", method.DeclaringType.Name, method.Name, ex.Message , infos), DebugLevel.EXCEPTION);
        }
        public static void ThrowNoImplementation(MethodBase method, string infos = "")
        {
            Print(String.Format("[{0}].{1}NoImplementation : {2}", method.DeclaringType.Name, method.Name, infos), DebugLevel.WARNING);
        }
        public static void Print(string s, DebugLevel level = DebugLevel.INFO)
        {
            if (DisplayLog)
            {
                s = CleanStringForMxs(s);

                string mxs = "";
                IFPValue fpv = ExecuteMxs("TatLoggerInstance != undefined");
                if (fpv.B is bool LoggerExists && LoggerExists)
                {
                    switch (level)
                    {
                        case DebugLevel.SILENCE:
                            mxs = "";
                            break;
                        case DebugLevel.INFO:
                            mxs = "TatLoggerInstance.LogInfo \"[Bubo] " + s + "\"\n";
                            break;
                        case DebugLevel.ULTRAVERBOSE:
                            mxs = "TatLoggerInstance.LogUltraVerbose \"[Bubo] " + s + "\"\n";
                            break;
                        case DebugLevel.ERROR:
                        case DebugLevel.EXCEPTION:
                            mxs = "TatLoggerInstance.LogError \"[Bubo] " + s + "\"\n";
                            break;
                        case DebugLevel.WARNING:
                            mxs = "TatLoggerInstance.LogWarning \"[Bubo] " + s + "\"\n";
                            break;
                        default:
                            mxs = "TatLoggerInstance.LogTxt \"[Bubo] " + s + "\"\n";
                            break;
                    }
                    ExecuteMxs(mxs);
                }
                else
                {
                    mxs = "[Bubo] " + s + "\n";
                    PrintInListener(mxs);
                }
            }
        }
        public static void PrintInListener(string s)
        {
            s += "\n";
            Global.TheListener.SetStyle(1);
            Global.TheListener.EditStream.Wputs(s);
            Global.TheListener.EditStream.Flush();
        }

        public static IFPValue ExecuteMxs(string mxs, bool quietError = false)
        {
            try
            {
                IFPValue fpv = Global.FPValue.Create();
#if MAX_2022
                Global.ExecuteMAXScriptScript(mxs, Autodesk.Max.MAXScript.ScriptSource.NonEmbedded, quietError, fpv, quietError);
#else
                Global.ExecuteMAXScriptScript(mxs, quietError, fpv, quietError);
#endif
                return fpv;
            }
            catch (Exception ex)
            {
                FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public static string GetBasename(string s)
        {
            try
            {
                string rule = @"(^[A-Z]_[A-Z][0-9a-zA-Z]+(\([0-9]+\))?)_";
                if (Regex.Match(s, rule) is Match m && m.Success)
                {
                    return m.Groups[0].Value;
                }
                return s;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public static string ReplaceBasename(string s, string replaceS)
        {
            try
            {
                string rule = @"(^[A-Z]_[A-Z][0-9a-zA-Z]+(\([0-9]+\))?)_";
                if (Regex.Match(s, rule) is Match matchS && matchS.Success)
                {
                    if (Regex.Match(replaceS, rule) is Match matchReplaceS && matchReplaceS.Success)
                    {
                        return Regex.Replace(s, matchS.Groups[0].Value, matchReplaceS.Groups[0].Value);
                    }
                }
                return s;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
    }
}
