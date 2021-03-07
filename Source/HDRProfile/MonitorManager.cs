using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;

using System.Threading;

namespace HDRProfile
{
    public class MonitorManager : BaseViewModel
    {

        Thread _updateThread = null;
        readonly object _threadControlLock = new object();
        bool _monitorCancelRequested = false;



        public static bool GlobalHDRIsActive { get; private set; } = false;

        public static event EventHandler HDRIsActiveChanged;


        public bool Monitoring { get; private set; } = false;


        public readonly HDRProfileSettings Settings;

        public event EventHandler AutoHDRChanged;

        //private List<Monitor> _monitors = new List<Monitor>();

        //public List<Monitor> Monitors { get => _monitors; set { _monitors = value; OnPropertyChanged(); } }

        public MonitorManager(HDRProfileSettings settings)
        {
            Settings = settings;
            MergeMonitors(GetActiveMonitors());
        }

        public void StartMonitoring()
        {
            lock (_threadControlLock)
            {
                if (Monitoring)
                    return;
                _updateThread = new Thread(HDRMonitorLoop);
                _updateThread.IsBackground = true;
                Monitoring = true;
                _monitorCancelRequested = false;
                _updateThread.Start();
            }
        }

        public void StopMonitoring()
        {
            lock (_threadControlLock)
            {
                if (!Monitoring)
                    return;
                _monitorCancelRequested = true;
                _updateThread.Join();
                _updateThread = null;
                Monitoring = false;
            }
        }

        private void HDRMonitorLoop()
        {
            while (!_monitorCancelRequested)
            {
                foreach (Monitor monitor in Settings.Monitors)
                    monitor.HDRState = HDRController.GetHDRState(monitor.UID);

                bool currentValue = HDRController.GetGlobalHDRState();
                bool changed = GlobalHDRIsActive != currentValue;
                GlobalHDRIsActive = currentValue;
                if (changed)
                {
                    try { HDRIsActiveChanged?.Invoke(null, EventArgs.Empty); } catch { }
                }
                System.Threading.Thread.Sleep(50);
            }
        }

        private List<Monitor> GetActiveMonitors()
        {
            List<Monitor> monitors = new List<Monitor>();
            SelectQuery Sq = new SelectQuery("Win32_DesktopMonitor");
            ManagementObjectSearcher objOSDetails = new ManagementObjectSearcher(Sq);
            ManagementObjectCollection osDetailsCollection = objOSDetails.Get();
            StringBuilder sb = new StringBuilder();
            foreach (ManagementObject mo in osDetailsCollection)
            {
                string pnpDeviceID = @"DISPLAY\SAM7058\5&61C49FC&2&UID28931";
                pnpDeviceID = (string)mo["PNPDeviceID"];
                int indexOfUIDStart = pnpDeviceID.IndexOf("UID");
                UInt32 uid = UInt32.Parse(pnpDeviceID.Substring(indexOfUIDStart + 3, pnpDeviceID.Length - (indexOfUIDStart + 3)));
                Monitor monitor = new Monitor((string)mo["Name"], (string)mo["Caption"], (string)mo["DeviceID"], uid);
                monitors.Add(monitor);

                //sb.AppendLine(string.Format("Name : {0}", ));
                //sb.AppendLine(string.Format("Availability: {0}", (ushort)mo["Availability"]));
                //sb.AppendLine(string.Format("Caption: {0}", ));
                //sb.AppendLine(string.Format("InstallDate: {0}", Convert.ToDateTime(mo["InstallDate"]).ToString()));
                //sb.AppendLine(string.Format("ConfigManagerUserConfig: {0}", mo["ConfigManagerUserConfig"].ToString()));
                //sb.AppendLine(string.Format("CreationClassName : {0}", (string)mo["CreationClassName"]));
                //sb.AppendLine(string.Format("Description: {0}", (string)mo["Description"]));
                //sb.AppendLine(string.Format("DeviceID : {0}", );
                //sb.AppendLine(string.Format("ErrorCleared: {0}", (string)mo["ErrorCleared"]));
                //sb.AppendLine(string.Format("ErrorDescription : {0}", (string)mo["ErrorDescription"]));
                //sb.AppendLine(string.Format("ConfigManagerUserConfig: {0}", mo["ConfigManagerUserConfig"].ToString()));
                //sb.AppendLine(string.Format("LastErrorCode : {0}", mo["LastErrorCode"]).ToString());
                //sb.AppendLine(string.Format("MonitorManufacturer : {0}", mo["MonitorManufacturer"]).ToString());
                //sb.AppendLine(string.Format("PNPDeviceID: {0}", (string)mo["PNPDeviceID"]));
                //sb.AppendLine(string.Format("MonitorType: {0}", (string)mo["MonitorType"]));
                //sb.AppendLine(string.Format("PixelsPerXLogicalInch : {0}", mo["PixelsPerXLogicalInch"].ToString()));
                //sb.AppendLine(string.Format("PixelsPerYLogicalInch: {0}", mo["PixelsPerYLogicalInch"].ToString()));
                //sb.AppendLine(string.Format("ScreenHeight: {0}", mo["ScreenHeight"].ToString()));
                //sb.AppendLine(string.Format("ScreenWidth : {0}", mo["ScreenWidth"]).ToString());
                //sb.AppendLine(string.Format("Status : {0}", (string)mo["Status"]));
                //sb.AppendLine(string.Format("SystemCreationClassName : {0}", (string)mo["SystemCreationClassName"]));
                //sb.AppendLine(string.Format("SystemName: {0}", (string)mo["SystemName"]));

            }
            return monitors;

        }


        private void MergeMonitors(List<Monitor> monitors)
        {

            List<Monitor> toRemove = new List<Monitor>();
            foreach (Monitor monitor in Settings.Monitors)
            {
                if (!monitors.Any(m => m.UID.Equals(monitor.UID)))
                    toRemove.Add(monitor);
            }
            foreach (Monitor monitor in toRemove)
                Settings.Monitors.Remove(monitor);
            foreach (Monitor monitor in monitors)
            {
                if (!Settings.Monitors.Any(m => m.UID.Equals(monitor.UID)))
                    Settings.Monitors.Add(monitor);
            }

            foreach (Monitor monitor in Settings.Monitors)
            {
                monitor.PropertyChanged += Monitor_PropertyChanged;
            }
        }

        private void Monitor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Monitor.AutoHDR))
                AutoHDRChanged?.Invoke(this, EventArgs.Empty);
        }


        public void ActivateHDR()
        {

            if (Settings.GlobalAutoHDR)
                HDRController.SetGlobalHDRState(true);
            else
            {
                foreach (Monitor monitor in Settings.Monitors)
                    if (monitor.AutoHDR)
                        ActivateHDR(monitor);
            }
        }

        public void DeactivateHDR()
        {
            if (Settings.GlobalAutoHDR)
                HDRController.SetGlobalHDRState(false);
            else
            {
                foreach (Monitor monitor in Settings.Monitors)
                    if (monitor.AutoHDR)
                        DeactivateHDR(monitor);
            }
        }

        public void ActivateHDR(Monitor display)
        {
            HDRController.SetHDRState(display.UID, true);
        }

        public static void DeactivateHDR(Monitor display)
        {
            HDRController.SetHDRState(display.UID, false);
        }

    }
}
    

