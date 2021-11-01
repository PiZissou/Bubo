using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;

namespace Bubo
{
    public static class SystemHelper
    {
        public static int GetCurrentDPI()
        {
            return (int)typeof(SystemParameters).GetProperty("Dpi", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
        }

        public static double GetCurrentDPIScaleFactor()
        {
            return (double)GetCurrentDPI() / 96;
        }

        public static Point GetMouseScreenPosition()
        {
            System.Drawing.Point point = Control.MousePosition;
            return new Point(point.X, point.Y);
        }

        public static long GetCurrentRam()
        {
            try
            {
                Process currentProc = Process.GetCurrentProcess();
                long memoryUsed = currentProc.PrivateMemorySize64;
                return memoryUsed;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return -1;
            }
        }

        public static long GetTotalRam()
        {
            try
            {
                ulong val = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
                return (long)val;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return -1;
            }
        }

        public static string GetUser()
        {
            try
            {
                return Environment.UserName;
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        /// <summary>
        /// Used because Directory.Move does not work across volumes
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        static public void CopyFolder(string sourceFolder, string destFolder, bool overwrite)
        {
            try
            {
                if (Directory.Exists(sourceFolder))
                {
                    if (!Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }
                    string[] files = Directory.GetFiles(sourceFolder);
                    foreach (string file in files)
                    {
                        string name = Path.GetFileName(file);
                        string dest = Path.Combine(destFolder, name);
                        File.Copy(file, dest, overwrite);
                    }
                    string[] folders = Directory.GetDirectories(sourceFolder);
                    foreach (string folder in folders)
                    {
                        string name = Path.GetFileName(folder);
                        string dest = Path.Combine(destFolder, name);
                        CopyFolder(folder, dest, overwrite);
                    }
                }
            } catch(Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }

        }

        /// <summary>
        /// Used because Directory.Move does not work across volumes
        /// </summary>
        /// <param name="sourceFolder"></param>
        /// <param name="destFolder"></param>
        static public void MoveFolder(string sourceFolder, string destFolder, bool overwrite)
        {
            try
            {
                if (Directory.Exists(sourceFolder))
                {
                    if (!Directory.Exists(destFolder))
                    {
                        Directory.CreateDirectory(destFolder);
                    }
                    string[] files = Directory.GetFiles(sourceFolder);
                    foreach (string file in files)
                    {
                        string name = Path.GetFileName(file);
                        string dest = Path.Combine(destFolder, name);
                        File.Copy(file, dest, overwrite);
                    }
                    string[] folders = Directory.GetDirectories(sourceFolder);
                    foreach (string folder in folders)
                    {
                        string name = Path.GetFileName(folder);
                        string dest = Path.Combine(destFolder, name);
                        CopyFolder(folder, dest, overwrite);
                    }
                    
                    Directory.Delete(sourceFolder, true);
                    
                }
            }
            catch (Exception ex)
            {
                Tools.FormatException(MethodBase.GetCurrentMethod(), ex);
            }

        }
    }
}
