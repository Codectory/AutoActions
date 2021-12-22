/*
  LICENSE
  -------
  Copyright (C) 2007 Ray Molenkamp

  This source code is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this source code or the software it produces.

  Permission is granted to anyone to use this source code for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this source code must not be misrepresented; you must not
     claim that you wrote the original source code.  If you use this source code
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original source code.
  3. This notice may not be removed or altered from any source distribution.
*/
// modified for NAudio

using System.Runtime.InteropServices;
using AudioSwitcher.AudioApi.CoreAudio.Interfaces;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    /// <summary>
    ///     Audio Endpoint Volume Volume Range
    /// </summary>
    internal class AudioEndpointVolumeVolumeRange
    {
        private readonly float _volumeIncrementdB;
        private readonly float _volumeMaxdB;
        private readonly float _volumeMindB;

        internal AudioEndpointVolumeVolumeRange(IAudioEndpointVolume parent)
        {
            Marshal.ThrowExceptionForHR(parent.GetVolumeRange(out _volumeMindB, out _volumeMaxdB, out _volumeIncrementdB));
        }

        /// <summary>
        ///     Minimum Decibels
        /// </summary>
        public float MinDecibels
        {
            get { return _volumeMindB; }
        }

        /// <summary>
        ///     Maximum Decibels
        /// </summary>
        public float MaxDecibels
        {
            get { return _volumeMaxdB; }
        }

        /// <summary>
        ///     Increment Decibels
        /// </summary>
        public float IncrementDecibels
        {
            get { return _volumeIncrementdB; }
        }
    }
}