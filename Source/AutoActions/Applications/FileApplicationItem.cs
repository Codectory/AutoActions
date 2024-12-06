using AutoActions.Applications;
using AutoActions.Core;
using AutoActions.UWP;
using CodectoryCore.UI.Wpf;
using CodectoryCore.Windows;
using CodectoryCore.Windows.FileSystem;
using CodectoryCore.Windows.Icons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace AutoActions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FileApplicationItem : ApplicationItemBase
    {

        public override void StartApplication()
        {
            Globals.Logs.Add($"Start application {ApplicationName}", false);
            try
            {

                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(ApplicationFilePath);
                process.Start();
                System.Threading.Thread.Sleep(2500);
                var processes = Process.GetProcessesByName(ApplicationName).ToList();
                if (processes.Count > 0)
                {
                    Process foundProcess = new Process();
                    Globals.Logs.Add($"Bring application to front: {ApplicationName}", false);
                    foundProcess = processes[0];
                    if (!foundProcess.HasExited && foundProcess.Responding)
                        Window.BringMainWindowToFront(foundProcess.ProcessName);
                }
                else
                    Globals.Logs.Add($"No started application found: {ApplicationName}", false);

            }
            catch (Exception ex)
            {
                Globals.Logs.AddException(ex);
            }
        }



    }
}
