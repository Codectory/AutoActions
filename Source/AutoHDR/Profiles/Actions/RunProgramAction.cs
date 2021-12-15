using AutoHDR.ProjectResources;
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
    public class RunProgramAction : ProfileActionBase
    {
        public override string ActionTypeName => ProjectResources.ProjectLocales.RunAction;


        private string _filePath = "";

        [JsonProperty]
        public string FilePath { get => _filePath; set { _filePath = value; OnPropertyChanged(); } }

        private string _arguments = "";

        [JsonProperty]
        public string Arguments { get => _arguments; set { _arguments = value; OnPropertyChanged(); } }

        private bool _waitForEnd = false;

        [JsonProperty]
        public bool WaitForEnd { get => _waitForEnd; set { _waitForEnd = value; OnPropertyChanged(); } }



        public override string ActionDescription => $"{Path.GetFileName(FilePath)} {Arguments}";

        public RelayCommand GetFileCommand { get; private set; }


        public RunProgramAction()
        {
            GetFileCommand = new RelayCommand(GetFile);

        }

        public override ActionEndResult RunAction(params object[] parameter)
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    using (Process proc = new Process())
                    {
                        proc.StartInfo = new ProcessStartInfo(FilePath);
                        if (!string.IsNullOrEmpty(Arguments))
                            proc.StartInfo.Arguments = Arguments;
                        CallNewLog(new CodectoryCore.Logging.LogEntry($"Starting {FilePath}"));

                        proc.Start();
                        if (WaitForEnd)
                        {
                            CallNewLog(new CodectoryCore.Logging.LogEntry($"Wait for end of {FilePath}"));
                            proc.WaitForExit();
                            CallNewLog(new CodectoryCore.Logging.LogEntry($"Process {FilePath} ended."));
                        }
                    }
                }
                else
                {
                    CallNewLog(new CodectoryCore.Logging.LogEntry($"File {FilePath} doesn't exist.", CodectoryCore.Logging.LogEntryType.Error));
                    return new ActionEndResult(false);
                }
                return new ActionEndResult(true);
            }
            catch (Exception ex)
            {
                CallNewLog(new CodectoryCore.Logging.LogEntry($"{ ex.Message }\r\n{ ex.StackTrace}", CodectoryCore.Logging.LogEntryType.Error));
                return new ActionEndResult(false, ex.Message, ex);
            }
        }

        public void GetFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.DefaultExt = ".exe";
            fileDialog.Filter = "Executables (.exe)|*.exe| All Files| *.*";
            Nullable<bool> result = fileDialog.ShowDialog();
            string filePath = string.Empty;
            if (result == true)
                filePath = fileDialog.FileName;
            else
                return;
            if (!File.Exists(filePath))
                throw new Exception("Invalid file path.");
            FilePath = filePath;
        }

    }
}
