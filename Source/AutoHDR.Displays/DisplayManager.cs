using AutoHDR.Threading;
using CCD;
using CCD.Enum;
using CCD.Struct;
using CodectoryCore;
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
    public class DisplayManager : BaseViewModel, IManagedThread
    {
        Thread _updateThread = null;
        readonly object _threadControlLock = new object();
        bool _monitorCancelRequested = false;

        public static readonly DisplayManager Instance = new DisplayManager();
        private DisplayManager()
        {

        }

        public bool GlobalHDRIsActive { get; private set; } = false;
        public bool PerMonitorHDR { get; private set; } = false;

        public event EventHandler HDRIsActiveChanged;
        private DispatchingObservableCollection<Display> _monitors = new DispatchingObservableCollection<Display>();
        public DispatchingObservableCollection<Display> Monitors { get => _monitors; set { _monitors = value; OnPropertyChanged(); } }
        public bool ManagedThreadIsActive { get; private set; } = false;


        public EventHandler<Exception> ExceptionThrown;



        public void LoadKnownDisplays(IList<Display> knownMonitors)
        {
            foreach (var monitor in knownMonitors)
                Monitors.Add(monitor);
            MergeMonitors(GetActiveMonitors());
        }

        public void StartManagedThread()
        {
            lock (_threadControlLock)
            {
                if (ManagedThreadIsActive)
                    return;
                _updateThread = new Thread(UpdateMonitorLoop);
                _updateThread.SetApartmentState(ApartmentState.STA);
                _updateThread.IsBackground = true;
                ManagedThreadIsActive = true;
                _monitorCancelRequested = false;
                _updateThread.Start();
            }
        }

        public void StopManagedThread()
        {
            lock (_threadControlLock)
            {
                if (!ManagedThreadIsActive)
                    return;
                _monitorCancelRequested = true;
                _updateThread.Join();
                _updateThread = null;
                ManagedThreadIsActive = false;
            }
        }

        private void UpdateMonitorLoop()
        {
            int exceptionCounter = 0;
            while (!_monitorCancelRequested)
            {
                try
                {
                    bool currentValue = false;
                    MergeMonitors(GetActiveMonitors());
                    foreach (Display monitor in Monitors)
                    {
                        monitor.UpdateHDRState();
                        if (monitor.Managed)
                            currentValue = currentValue || monitor.HDRState;
                    }
                    bool changed = GlobalHDRIsActive != currentValue;
                    GlobalHDRIsActive = currentValue;
                    if (changed)
                    {
                        try { HDRIsActiveChanged?.Invoke(null, EventArgs.Empty); } catch { }
                    }
                    System.Threading.Thread.Sleep(100);
                    exceptionCounter = 0;
                }
                catch (Exception ex)
                {
                    exceptionCounter++;
                    ExceptionThrown?.BeginInvoke(this,ex,null,null);
                    if (exceptionCounter >= 5)
                        throw ex;
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

        public static UInt32 GetUID(uint deviceID)
        {
            return HDRController.GetUID(deviceID);
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
            List<Display> displays = new List<Display>();
            DisplayConfigTopologyId topologyId;
            var pathWraps = GetPathWraps(QueryDisplayFlags.OnlyActivePaths, out topologyId);

            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            DEVMODE dm = new DEVMODE();
            d.cb = Marshal.SizeOf(d);

            uint deviceID = 0;
            while (NativeMethods.EnumDisplayDevices(null, deviceID, ref d, 0))
            {
                DisplayInformation displayInfo;
                if (0 != NativeMethods.EnumDisplaySettings(d.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref dm))
                    displayInfo = new DisplayInformation(deviceID, d, dm);
                else
                    displayInfo = new DisplayInformation(deviceID, d);
                Display display = new Display(displayInfo, GetUID(displayInfo.Id));
                display.ColorDepth = Convert.ToInt32(HDRController.GetColorDepth(display.UID));
                if (!displays.Any(m => m.ID.Equals(display.ID)) && pathWraps.Any(p => p.Path.sourceInfo.id.Equals(display.ID)))
                    displays.Add(display);
                deviceID++;
            }
            return displays;
        }


        private void MergeMonitors(List<Display> activeMonitors)
        {

            try
            {
                List<Display> toRemove = new List<Display>();
                foreach (Display monitor in Monitors)
                {
                    if (monitor.UID == 0 || !activeMonitors.Any(m => m.UID.Equals(monitor.UID)))
                        toRemove.Add(monitor);
                }
                foreach (Display monitor in toRemove)
                    Monitors.Remove(monitor);
                foreach (Display monitor in activeMonitors)
                {
                    if (monitor.UID != 0 && !Monitors.Any(m => m.UID.Equals(monitor.UID)))
                        Monitors.Add(monitor);
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
                        existingMonitor.Resolution = monitor.Resolution;

                    }

                }
            }
            catch (Exception ex)
            {
                ExceptionThrown?.BeginInvoke(this, ex, null, null);
            }
        }




        public void ActivateHDR()
        {

            if (!PerMonitorHDR)
                HDRController.SetGlobalHDRState(true);
            else
            {
                foreach (Display monitor in Monitors)
                    if (monitor.Managed)
                        ActivateHDR(monitor);
            }
        }

        public void DeactivateHDR()
        {
            if (!PerMonitorHDR)
                HDRController.SetGlobalHDRState(false);
            else
            {
                foreach (Display monitor in Monitors)
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

        private static IEnumerable<DisplayConfigPathWrap> GetPathWraps(QueryDisplayFlags pathType, out DisplayConfigTopologyId topologyId)
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
                var reason = string.Format("GetDisplayConfigBufferSizesFailed() failed. Status: {0}", status);
                throw new Exception(reason);
            }

            var pathInfoArray = new DisplayConfigPathInfo[numPathArrayElements];
            var modeInfoArray = new DisplayConfigModeInfo[numModeInfoArrayElements];

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
