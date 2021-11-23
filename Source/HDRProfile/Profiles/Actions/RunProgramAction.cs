using AutoHDR.ProjectResources;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.Profiles.Actions
{

    public class RunProgramAction : ProfileActionBase
    {
        public override string ActionTypeName => ProjectResources.Locale_Texts.RunAction;


        private string _filePath = "";
        public string FilePath { get => _filePath; set { _filePath = value; OnPropertyChanged(); } }

        private string _arguments = "";
        public string Arguments { get => _arguments; set { _arguments = value; OnPropertyChanged(); } }

        private bool _waitForEnd = false;
        public bool WaitForEnd { get => _waitForEnd; set { _waitForEnd = value; OnPropertyChanged(); } }



        public override string ActionDescription => $"{Path.GetFileName(FilePath)} {Arguments}";

        public RunProgramAction()
        {
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
                        proc.Start();
                        if (WaitForEnd)
                            proc.WaitForExit();
                    }
                }
                else
                    Tools.Logs.Add($"File {FilePath} not found.", true);
                return new ActionEndResult(true);
            }
            catch (Exception ex)
            {
                return new ActionEndResult(false, ex.Message, ex);
            }
        }
    }
}
