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
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Starting HDR-Profile";

                // On logon
                td.Triggers.Add(new Microsoft.Win32.TaskScheduler.LogonTrigger { UserId = Environment.UserName });

                // Create an action that will launch Notepad whenever the trigger fires
                td.Actions.Add(new ExecAction(filePath, null));

                if (autostart)
                    // Register the task in the root folder
                    ts.RootFolder.RegisterTaskDefinition(@"HDR-Profile", td);
                else
                    // Remove the task we just created
                    ts.RootFolder.DeleteTask("HDR-Profile");
            }
        }

    }
}

