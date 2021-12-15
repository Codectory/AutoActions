using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodectoryCore.UI.Wpf;

namespace AutoHDR.Audio
{
    public static class AudioManager 
    {
        static readonly object _lockInitialize = new object();


        public static bool AudioDeviceUpdatesRunning { get; private set; } = false;

        public static CoreAudioController Controller { get; private set; } =null;

        public static IReadOnlyList<CoreAudioDevice> OutputAudioDevices { get; private set;  }

        public static IReadOnlyList<CoreAudioDevice> InputAudioDevices { get; private set;} 

        public static bool Initialized { get; private set; } = false;

        public static void Initialize()
        {
            lock (_lockInitialize)
            {
                if (Initialized)
                    return;
                Controller = new CoreAudioController();
                OutputAudioDevices  = Controller.GetPlaybackDevices().ToList().AsReadOnly();
                InputAudioDevices = Controller.GetCaptureDevices().ToList().AsReadOnly();
                Initialized = true;
            }
        }

        
    }
}
