using AutoHDR.ProjectResources;
using CodectoryCore.Logging;
using CodectoryCore.UI.Wpf;
using Microsoft.Win32;
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

namespace AutoHDR.Profiles.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CloseProgramAction : ProfileActionBase
    {
        public override string ActionTypeName => ProjectResources.Locale_Texts.CloseProgram;


        private string _processName = "";

        [JsonProperty]
        public string ProcessName { get => _processName; set { _processName = value; OnPropertyChanged(); } }

        private bool _force = false;

        [JsonProperty]
        public bool Force { get => _force; set { _force = value; OnPropertyChanged(); } }



        public override string ActionDescription => $"{Locale_Texts.Close} {ProcessName}";

        public RelayCommand GetFileCommand { get; private set; }


        public CloseProgramAction()
        {

        }

        public override ActionEndResult RunAction(params object[] parameter)
        {
            try
            {

                Process[] runningProcesses = Process.GetProcesses();
                foreach (Process process in runningProcesses)
                {
                    if (process.ProcessName == ProcessName)
                        try
                        {
                            CallNewLog(new LogEntry($"Closing {ProcessName}..."));

                            bool result = process.CloseMainWindow();
                            if (!result)

                                process.Close();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Closing {ProcessName} failed", ex);
                        }
                    if (!process.HasExited)
                    {
                        if (Force)
                        {
                            CallNewLog(new LogEntry($"Killing {ProcessName}..."));
                            try
                            {
                                process.Kill();
                                CallNewLog(new LogEntry($"Process  {ProcessName} killed.."));

                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Killing {ProcessName} failed", ex);
                            }
                        }
                    }
                    else
                        CallNewLog(new LogEntry($"Process  {ProcessName} closed."));


                }
                return new ActionEndResult(true);
            }
            catch (Exception ex)
            {
                CallNewLog(new CodectoryCore.Logging.LogEntry($"{ ex.Message }\r\n{ ex.StackTrace}", CodectoryCore.Logging.LogEntryType.Error));
                return new ActionEndResult(false, ex.Message, ex);
            }
        }
    }
}
