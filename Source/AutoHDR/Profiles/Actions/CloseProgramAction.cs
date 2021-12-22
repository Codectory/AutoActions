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
        public override string ActionTypeName => ProjectResources.ProjectLocales.CloseProgramAction;


        private string _processName = "";

        [JsonProperty]
        public string ProcessName { get => _processName; set { _processName = value; OnPropertyChanged(); } }

        private bool _force = false;

        [JsonProperty]
        public bool Force { get => _force; set { _force = value; OnPropertyChanged(); } }


        public override string ActionDescription => $"{ProjectLocales.Close} {ProcessName}";

        public RelayCommand GetFileCommand { get; private set; }


        public CloseProgramAction() : base()
        {

        }

        public override ActionEndResult RunAction(params object[] parameter)
        {
            try
            {
                bool result = false;
                Process[] runningProcesses = Process.GetProcesses();
                bool processFound = false;
                CallNewLog(new LogEntry($"Searching for{ProcessName}..."));
                string searchName = ProcessName;
                if (searchName.ToUpperInvariant().EndsWith(".EXE"))
                    searchName = searchName.Substring(0, searchName.Length - 4);
                foreach (Process process in runningProcesses)
                {
                    if (process.ProcessName == searchName)
                    {
                        try
                        {
                            CallNewLog(new LogEntry($"Closing {ProcessName}..."));

                            result = process.CloseMainWindow();
                            process.WaitForExit(3000);
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
                                    result = true;

                                }
                                catch (Exception ex)
                                {
                                    throw new Exception($"Killing {ProcessName} failed", ex);
                                }
                            }
                            else
                                result = false;
                        }
                        else
                            CallNewLog(new LogEntry($"Process  {ProcessName} closed."));
                    }
                }
                if (!processFound)
                    CallNewLog(new LogEntry($"Process {ProcessName} not found."));

                return new ActionEndResult(result);
            }
            catch (Exception ex)
            {
                CallNewLog(new CodectoryCore.Logging.LogEntry($"{ ex.Message }\r\n{ ex.StackTrace}", CodectoryCore.Logging.LogEntryType.Error));
                return new ActionEndResult(false, ex.Message, ex);
            }
        }
    }
}
