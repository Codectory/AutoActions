using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoHDR.UWP
{
    public static class WWAHostHandler
    {
        public const int QueryLimitedInformation = 0x1000;
        public const int ERROR_INSUFFICIENT_BUFFER = 0x7a;
        public const int ERROR_SUCCESS = 0x0;

        [DllImport("kernel32.dll")]
        internal static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("kernel32.dll")]
        internal static extern Int32 GetApplicationUserModelId(
            IntPtr hProcess,
            ref UInt32 AppModelIDLength,
            [MarshalAs(UnmanagedType.LPWStr)] StringBuilder sbAppUserModelID);


        public static string GetProcessName(int pid)
        {
            string appName = string.Empty;
            IntPtr ptrProcess = OpenProcess(QueryLimitedInformation, false, pid);
            if (IntPtr.Zero != ptrProcess)
            {
                uint cchLen = 130; // Currently APPLICATION_USER_MODEL_ID_MAX_LENGTH = 130
                StringBuilder sbName = new StringBuilder((int)cchLen);
                Int32 lResult = GetApplicationUserModelId(ptrProcess, ref cchLen, sbName);
                if (ERROR_SUCCESS == lResult)
                {
                    appName = sbName.ToString();
                }
                else if (ERROR_INSUFFICIENT_BUFFER == lResult)
                {
                    sbName = new StringBuilder((int)cchLen);
                    if (ERROR_SUCCESS == GetApplicationUserModelId(ptrProcess, ref cchLen, sbName))
                    {
                        appName = sbName.ToString();
                    }
                }
                CloseHandle(ptrProcess);
            }

            if (appName.Contains(".") && appName.Contains("_"))
            {
                int length = appName.LastIndexOf('_') - appName.IndexOf('.');
                appName.Substring(appName.IndexOf('.'), length);
            }

            try
            {
                if (!CheckIfRunning(Process.GetProcessById(pid)))
                    appName = string.Empty;
            }
            catch (Exception)
            {
                appName = string.Empty;
            }

            return appName;

        }

        public static bool CheckIfRunning(Process process)
        {
            string commandline;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                commandline = objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
            }

            return commandline == null ||  commandline.ToUpperInvariant().EndsWith("BT") ? false : true;

        }

    }
}
