using AutoHDR.Threading;
using CCD;
using CCD.Enum;
using CCD.Struct;
using CodectoryCore;
using CodectoryCore.UI.Wpf;
using NvAPIWrapper.Display;
using NvAPIWrapper.Native;
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
    public abstract class DisplayManagerBase : BaseViewModel, IManagedThread, IDisplayManagerBase
    {
        public abstract GraphicsCardType GraphicsCardType { get; }
        Thread _updateThread = null;
        readonly object _threadControlLock = new object();
        bool _monitorCancelRequested = false;
        bool _selectedHDR = false;
 

        readonly object _lockExceptionThrown = new object();
        public EventHandler<Exception> _exceptionThrown;

        event EventHandler<Exception> IDisplayManagerBase.ExceptionThrown
        {
            add
            {
                lock (_lockExceptionThrown)
                {
                    _exceptionThrown += value;
                }
            }

            remove
            {
                lock (_lockExceptionThrown)
                {
                    _exceptionThrown -= value;
                }
            }
        }

        public bool GlobalHDRIsActive { get; private set; } = false;
        public bool SelectedHDR { get => _selectedHDR; set { _selectedHDR = value; OnPropertyChanged(); } } 

        public event EventHandler HDRIsActiveChanged;

        private DispatchingObservableCollection<Display> _monitors = new DispatchingObservableCollection<Display>();
        public DispatchingObservableCollection<Display> Monitors { get => _monitors; set { _monitors = value; OnPropertyChanged(); } }
        public bool ManagedThreadIsActive { get; private set; } = false;



        protected DisplayManagerBase()
        {
        }

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

        protected void UpdateMonitorLoop()
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
                    System.Threading.Thread.Sleep(1000);
                    exceptionCounter++;
                    _exceptionThrown?.BeginInvoke(this, ex, null, null);
                    if (exceptionCounter >= 5)
                        throw ex;
                }
            }
        }

        public abstract void SetResolution(Display display, Size resolution);

        public abstract void SetRefreshRate(Display display, int refreshRate);

        public abstract void SetColorDepth(Display display, ColorDepth colorDepth);

        public abstract bool GetHDRState(Display display);

        public abstract ColorDepth GetColorDepth(Display display);

        public abstract UInt32 GetUID(uint displayUD);

        public abstract Size GetResolution(Display display);

        public abstract int GetRefreshRate(Display display);

        public abstract List<Display> GetActiveMonitors();

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
                        existingMonitor.GraphicsCard = monitor.GraphicsCard;
                        existingMonitor.ID = monitor.ID;
                        existingMonitor.IsPrimary = monitor.IsPrimary;
                        existingMonitor.Resolution = monitor.Resolution;
                        existingMonitor.Tag = monitor.Tag;


                    }

                }
            }
            catch (Exception ex)
            {
                _exceptionThrown?.BeginInvoke(this, ex, null, null);
            }
        }

        protected abstract void ActivateHDR(Display display);

        protected abstract void DeactivateHDR(Display display);

        public  void ActivateHDR()
        {
            if (!SelectedHDR)
                HDRController.SetGlobalHDRState(true);
            else
            {
                foreach (Display display in Monitors)
                    if (display.Managed)
                        ActivateHDR(display);
            }
        }

        public void DeactivateHDR()
        {
            if (!SelectedHDR)
                HDRController.SetGlobalHDRState(false);
            else
            {
                foreach (Display display in Monitors)
                    if (display.Managed)
                        DeactivateHDR(display);

            }
        }
    }
}
