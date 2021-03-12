using CCD;
using CCD.Enum;
using CCD.Struct;
using CodectoryCore.UI.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

using System.Threading;

namespace HDRProfile.Displays
{

    [Flags()]
    public enum DisplayDeviceStateFlags : int
    {
        /// <summary>The device is part of the desktop.</summary>
        AttachedToDesktop = 0x1,
        MultiDriver = 0x2,
        /// <summary>The device is part of the desktop.</summary>
        PrimaryDevice = 0x4,
        /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
        MirroringDriver = 0x8,
        /// <summary>The device is VGA compatible.</summary>
        VGACompatible = 0x16,
        /// <summary>The device is removable; it cannot be the primary display.</summary>
        Removable = 0x20,
        /// <summary>The device has more display modes than its output devices support.</summary>
        ModesPruned = 0x8000000,
        Remote = 0x4000000,
        Disconnect = 0x2000000
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        [MarshalAs(UnmanagedType.U4)]
        public DisplayDeviceStateFlags StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }



    public class DisplayManager : BaseViewModel
    {
        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);


        Thread _updateThread = null;
        readonly object _threadControlLock = new object();
        bool _monitorCancelRequested = false;



        public static bool GlobalHDRIsActive { get; private set; } = false;

        public static event EventHandler HDRIsActiveChanged;


        public bool Monitoring { get; private set; } = false;


        public readonly HDRProfileSettings Settings;

        public event EventHandler AutoHDRChanged;


        public ObservableCollection<Display> Monitors => Settings.Monitors;

        public DisplayManager(HDRProfileSettings settings)
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
                bool currentValue = false;

                foreach (Display monitor in Monitors)
                {
                    monitor.UpdateHDRState();
                    if (monitor.AutoHDR)
                        currentValue = currentValue || monitor.HDRState;
                }
                bool changed = GlobalHDRIsActive != currentValue;
                GlobalHDRIsActive = currentValue;
                if (changed)
                {
                    try { HDRIsActiveChanged?.Invoke(null, EventArgs.Empty); } catch { }
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        //private List<Monitor> GetActiveMonitors()
        //{

        //    var device = new DISPLAY_DEVICE();
        //    device.cb = Marshal.SizeOf(device);
        //    try
        //    {
        //        for (uint id = 0; EnumDisplayDevices(null, id, ref device, 0); id++)
        //        {
        //            device.cb = Marshal.SizeOf(device);
        //            EnumDisplayDevices(device.DeviceName, 0, ref device, 0);
        //            device.cb = Marshal.SizeOf(device);

        //            Console.WriteLine("id={0}, name={1}, devicestring={2}", id, device.DeviceName, device.DeviceString);
        //            if (device.DeviceName == null || device.DeviceName == "") continue;
        //        }
        //        string x = Console.ReadLine();
        //    }






        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(String.Format("{0}", ex.ToString()));
        //    }

        //    return new List<Monitor>();
        //}



        private List<Display> GetActiveMonitors()
        {
            List<Display> monitors = new List<Display>();

            DisplayConfigTopologyId topologyId;
            var pathWraps = GetPathWraps(QueryDisplayFlags.OnlyActivePaths, out topologyId);

            foreach (var pathWrap in pathWraps)
            {
                var path = pathWrap.Path;
                var sourceModeInfo = pathWrap.Modes.First(x => x.infoType == DisplayConfigModeInfoType.Source);

                var resolution = new Size
                {
                    Width = sourceModeInfo.sourceMode.width,
                    Height = sourceModeInfo.sourceMode.height
                };

                var refreshRate =
                    (int)Math.Round((double)path.targetInfo.refreshRate.numerator / path.targetInfo.refreshRate.denominator);
                var rotationOriginal = path.targetInfo.rotation;


                DisplayConfigSourceDeviceName displayConfigSourceDeviceName;

                var displayName = "<Unknown>"; 
                var nameStatus = GetDisplayConfigSourceDeviceName(sourceModeInfo,
                    out displayConfigSourceDeviceName);

                if (nameStatus == StatusCode.Success)
                    displayName = displayConfigSourceDeviceName.viewGdiDeviceName;

                Display monitor = new Display(displayName, path.targetInfo.id, resolution,  refreshRate);
                monitors.Add(monitor);
            }
            return monitors;
        }


        private void MergeMonitors(List<Display> monitors)
        {

            List<Display> toRemove = new List<Display>();
            foreach (Display monitor in Monitors)
            {
                if (!monitors.Any(m => m.UID.Equals(monitor.UID)))
                    toRemove.Add(monitor);
            }
            foreach (Display monitor in toRemove)
                Monitors.Remove(monitor);
            foreach (Display monitor in monitors)
            {
                if (!Settings.Monitors.Any(m => m.UID.Equals(monitor.UID)))
                    Settings.Monitors.Add(monitor);
                else
                {
                   Display existingMonitor = Monitors.First(m => m.UID.Equals(monitor.UID));
                    existingMonitor.Name = monitor.Name;
                    existingMonitor.RefreshRate = monitor.RefreshRate;
                    existingMonitor.Resolution = monitor.Resolution;
                }
            }

            foreach (Display monitor in Monitors)
            {
                monitor.PropertyChanged += Monitor_PropertyChanged;
            }
        }

        private void Monitor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Display.AutoHDR))
                AutoHDRChanged?.Invoke(this, EventArgs.Empty);
        }


        public void ActivateHDR()
        {

            if (Settings.GlobalAutoHDR)
                HDRController.SetGlobalHDRState(true);
            else
            {
                foreach (Display monitor in Settings.Monitors)
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
                foreach (Display monitor in Settings.Monitors)
                    if (monitor.AutoHDR)
                        DeactivateHDR(monitor);
            }
        }

        public void ActivateHDR(Display display)
        {
            HDRController.SetHDRState(display.UID, true);
        }

        public static void DeactivateHDR(Display display)
        {
            HDRController.SetHDRState(display.UID, false);
        }

        private static IEnumerable<DisplayConfigPathWrap> GetPathWraps(
    QueryDisplayFlags pathType,
    out DisplayConfigTopologyId topologyId)
        {
            topologyId = DisplayConfigTopologyId.Zero;

            int numPathArrayElements;
            int numModeInfoArrayElements;

            var status = Wrapper.GetDisplayConfigBufferSizes(
                pathType,
                out numPathArrayElements,
                out numModeInfoArrayElements);

            if (status != StatusCode.Success)
            {
                // TODO; POSSIBLY HANDLE SOME OF THE CASES.
                var reason = string.Format("GetDisplayConfigBufferSizesFailed() failed. Status: {0}", status);
                throw new Exception(reason);
            }

            var pathInfoArray = new DisplayConfigPathInfo[numPathArrayElements];
            var modeInfoArray = new DisplayConfigModeInfo[numModeInfoArrayElements];

            // topology ID only valid with QDC_DATABASE_CURRENT
            var queryDisplayStatus = pathType == QueryDisplayFlags.DatabaseCurrent
                ? Wrapper.QueryDisplayConfig(
                    pathType,
                    ref numPathArrayElements, pathInfoArray,
                    ref numModeInfoArrayElements, modeInfoArray, out topologyId)
                : Wrapper.QueryDisplayConfig(
                    pathType,
                    ref numPathArrayElements, pathInfoArray,
                    ref numModeInfoArrayElements, modeInfoArray);
            //////////////////////

            if (queryDisplayStatus != StatusCode.Success)
            {
                // TODO; POSSIBLY HANDLE SOME OF THE CASES.
                var reason = string.Format("QueryDisplayConfig() failed. Status: {0}", queryDisplayStatus);
                throw new Exception(reason);
            }

            var list = new List<DisplayConfigPathWrap>();
            foreach (var path in pathInfoArray)
            {
                var outputModes = new List<DisplayConfigModeInfo>();
                foreach (var modeIndex in new[]
                {
                    path.sourceInfo.modeInfoIdx,
                    path.targetInfo.modeInfoIdx
                })
                {
                    if (modeIndex < modeInfoArray.Length)
                        outputModes.Add(modeInfoArray[modeIndex]);
                }

                list.Add(new DisplayConfigPathWrap(path, outputModes));
            }
            return list;
        }

        /// <summary>
        ///     This method give you access to monitor device name.
        ///     Such as "\\DISPLAY1"
        /// </summary>
        /// <param name="sourceModeInfo"></param>
        /// <param name="displayConfigSourceDeviceName"></param>
        /// <returns></returns>
        private static StatusCode GetDisplayConfigSourceDeviceName(
            DisplayConfigModeInfo sourceModeInfo,
            out DisplayConfigSourceDeviceName displayConfigSourceDeviceName)
        {
            displayConfigSourceDeviceName = new DisplayConfigSourceDeviceName
            {
                header = new DisplayConfigDeviceInfoHeader
                {
                    adapterId = sourceModeInfo.adapterId,
                    id = sourceModeInfo.id,
                    size =
                        Marshal.SizeOf(
                            typeof(DisplayConfigSourceDeviceName)),
                    type = DisplayConfigDeviceInfoType.GetSourceName
                }
            };
            return Wrapper.DisplayConfigGetDeviceInfo(ref displayConfigSourceDeviceName);
        }


    }
}
    

