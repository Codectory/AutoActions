using CodectoryCore;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace AutoHDR.Displays
{
    public interface IDisplayManagerBase
    {
        bool GlobalHDRIsActive { get; }
        GraphicsCardType GraphicsCardType { get; }
        bool ManagedThreadIsActive { get; }
        DispatchingObservableCollection<Display> Monitors { get; set; }
        bool SelectedHDR { get; set; }

        event EventHandler HDRIsActiveChanged;
        event EventHandler<Exception> ExceptionThrown;


        void ActivateHDR();
        void DeactivateHDR();
        List<Display> GetActiveMonitors();
        ColorDepth GetColorDepth(Display display);
        bool GetHDRState(Display display);
        int GetRefreshRate(Display display);
        Size GetResolution(Display display);
        uint GetUID(uint displayUD);
        void LoadKnownDisplays(IList<Display> knownMonitors);
        void SetColorDepth(Display display, ColorDepth colorDepth);
        void SetRefreshRate(Display display, int refreshRate);
        void SetResolution(Display display, Size resolution);
        void StartManagedThread();
        void StopManagedThread();
    }
}