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
using System.Windows.Threading;

namespace AutoHDR.Displays
{
    public class DisplayManager : BaseViewModel
    {
        Thread _updateThread = null;
        readonly object _threadControlLock = new object();
        bool _monitorCancelRequested = false;

        public static readonly DisplayManager Instance = new DisplayManager();
        private DisplayManager()
        {

        }

        public static bool GlobalHDRIsActive { get; private set; } = false;

        public static event EventHandler HDRIsActiveChanged;


        public bool Monitoring { get; private set; } = false;


        public UserAppSettings Settings { get; private set; }


        public ObservableCollection<Display> Monitors => Settings.Monitors;

        public void LoadSettings(UserAppSettings settings)
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
                _updateThread = new Thread(UpdateMonitorLoop);
                _updateThread.SetApartmentState(ApartmentState.STA);
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

        private void UpdateMonitorLoop()
        {
            while (!_monitorCancelRequested)
            {
                try
                {
                    bool currentValue = false;
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        MergeMonitors(GetActiveMonitors());
                    });
                    foreach (Display monitor in Monitors)
                    {
                        monitor.UpdateHDRState();
                        if (monitor.Managed)
                            currentValue = currentValue || monitor.HDRState;
                        monitor.Resolution = GetResolution(monitor.UID);
                        monitor.RefreshRate = GetRefreshRate(monitor.ID);

                    }
                    bool changed = GlobalHDRIsActive != currentValue;
                    GlobalHDRIsActive = currentValue;
                    if (changed)
                    {
                        try { HDRIsActiveChanged?.Invoke(null, EventArgs.Empty); } catch { }
                    }
                    System.Threading.Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Globals.Logs.AddException(ex);
                    throw;
                }
            }
        }

        private void ChangeDisplaySetting(uint deviceID, Func<DEVMODE, DEVMODE> func)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);


            NativeMethods.EnumDisplayDevices(null, deviceID, ref d, 0);

            if (0 != NativeMethods.EnumDisplaySettings(
                d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
            {

                dm = func.Invoke(dm);

                DISP_CHANGE iRet = NativeMethods.ChangeDisplaySettingsEx(
                    d.DeviceName, ref dm, IntPtr.Zero,
                    DisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
            }
        }

        public void SetResolution(uint deviceID, Size resolution)
        {
            Func<DEVMODE, DEVMODE> func = (dm) =>
            {
                dm.dmPelsHeight = Convert.ToInt32(resolution.Height);
                dm.dmPelsWidth = Convert.ToInt32(resolution.Width);
                return dm;
            };

            ChangeDisplaySetting(deviceID, func);
        }

        public void SetRefreshRate(uint deviceID, int refreshRate)
        {
            Func<DEVMODE, DEVMODE> func = (dm) =>
            {
                dm.dmDisplayFrequency = refreshRate;

                return dm;
            };
            ChangeDisplaySetting(deviceID, func);
        }

        public void SetColorDepth(uint deviceID, int colorDepth)
        {
            Func<DEVMODE, DEVMODE> func = (dm) =>
            {
                dm.dmBitsPerPel = colorDepth;

                return dm;
            };
        }


        public static int GetColorDepth(uint deviceID)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);


            NativeMethods.EnumDisplayDevices(null, deviceID, ref d, 0);

            if (0 != NativeMethods.EnumDisplaySettings(
                d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
            {

                return dm.dmBitsPerPel;
            }
            return 0;
        }


        public Size GetResolution(uint deviceID)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);


            NativeMethods.EnumDisplayDevices(null, deviceID, ref d, 0);

            if (0 != NativeMethods.EnumDisplaySettings(
                d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
            {
                Size resolution = new Size(dm.dmPelsWidth, dm.dmPelsHeight);
                return resolution;

            }
            else return Size.Empty;
        }

        public int GetRefreshRate(uint deviceID)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);

            NativeMethods.EnumDisplayDevices(null, deviceID, ref d, 0);

            if (0 != NativeMethods.EnumDisplaySettings(
                d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
            {
                return dm.dmDisplayFrequency;

            }
            else return 0;
        }

        public static List<Display> GetActiveMonitors()
        {
            List<Display> monitors = new List<Display>();
            DisplayConfigTopologyId topologyId;
            var pathWraps = GetPathWraps(QueryDisplayFlags.OnlyActivePaths, out topologyId);

            List<DisplayInformation> _interopDisplays = new List<DisplayInformation>();
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);

            uint deviceID = 0;
            while (NativeMethods.EnumDisplayDevices(null, deviceID, ref d, 0))
            {
                if (0 != NativeMethods.EnumDisplaySettings(d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
                    _interopDisplays.Add(new DisplayInformation(deviceID, d, dm));
                else
                    _interopDisplays.Add(new DisplayInformation(deviceID, d));
                deviceID++;
            }

            foreach (var pathWrap in pathWraps)
            {
                var path = pathWrap.Path;
               DisplayInformation displayInformation = _interopDisplays.FirstOrDefault(di => di.Id.Equals(path.sourceInfo.id));

                Display display = new Display(displayInformation, path.targetInfo.id);
                if (!monitors.Any(m => m.ID.Equals(display.ID)))
                    monitors.Add(display);
            }

            return monitors;
        }


        private void MergeMonitors(List<Display> activeMonitors)
        {

            try
            {
                List<Display> toRemove = new List<Display>();
                foreach (Display monitor in Monitors)
                {
                    if (!activeMonitors.Any(m => m.UID.Equals(monitor.UID)))
                        toRemove.Add(monitor);
                }
                foreach (Display monitor in toRemove)
                    Monitors.Remove(monitor);
                foreach (Display monitor in activeMonitors)
                {
                    if (!Settings.Monitors.Any(m => m.UID.Equals(monitor.UID)))
                        Settings.Monitors.Add(monitor);
                    else
                    {
                        Display existingMonitor = Monitors.First(m => m.UID.Equals(monitor.UID));
                        existingMonitor.Name = monitor.Name;
                        existingMonitor.ColorDepth = monitor.ColorDepth;
                        existingMonitor.RefreshRate = monitor.RefreshRate;
                        existingMonitor.Resolution = monitor.Resolution;
                        existingMonitor.DeviceKey = monitor.DeviceKey;
                        existingMonitor.GraphicsCard = monitor.GraphicsCard;
                        existingMonitor.ID = monitor.ID;
                        existingMonitor.IsPrimary = monitor.IsPrimary;

                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logs.AddException(ex);
            }
        }




        public void ActivateHDR()
        {

            if (Settings.GlobalAutoHDR)
                HDRController.SetGlobalHDRState(true);
            else
            {
                foreach (Display monitor in Settings.Monitors)
                    if (monitor.Managed)
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
                    if (monitor.Managed)
                        DeactivateHDR(monitor);
            }
        }

        public static void ActivateHDR(Display display)
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
