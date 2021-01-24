using Microsoft.Win32.TaskScheduler;
using CodectoryCore.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDRProfile
{
    public static class Tools
    {
        public static void SetAutoStart(string applicationName, string filePath, bool autostart)
        {
            RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            object existing = rk.GetValue(applicationName);
            if (filePath.Equals(existing) && autostart)
                return;

            if (autostart)
                rk.SetValue(applicationName, filePath);
            else
                rk.DeleteValue(applicationName, false);
        }

        public static void SetAutoStartInScheduler(string applicationName, string filePath, bool autostart)
        {
            using (TaskService ts = new TaskService())
            {
                string taskName = "HDR-Profile";

                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Starting HDR-Profile on logon.";
                td.Principal.RunLevel = TaskRunLevel.Highest;


                // On logon
                td.Triggers.Add(new Microsoft.Win32.TaskScheduler.LogonTrigger { UserId = Environment.UserName});
                td.Actions.Add(new ExecAction(filePath, null));

                if (autostart)
                {
                    if (ts.RootFolder.Tasks.Any(t => t.Name == taskName))
                        ts.RootFolder.DeleteTask(taskName);
                    ts.RootFolder.RegisterTaskDefinition(taskName, td);
                }
                else
                {
                    if (ts.RootFolder.Tasks.Any(t => t.Name == taskName))
                        ts.RootFolder.DeleteTask(taskName);
                }
            }
        }

    }
}

