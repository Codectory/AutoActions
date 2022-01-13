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

using System;
using System.Runtime.InteropServices;
using AudioSwitcher.AudioApi.CoreAudio.Interfaces;
using AudioSwitcher.AudioApi.CoreAudio.Threading;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    /// <summary>
    ///     Audio Meter Information Channels
    /// </summary>
    internal class AudioMeterInformationChannels
    {
        private readonly IAudioMeterInformation _audioMeterInformation;

        internal AudioMeterInformationChannels(IAudioMeterInformation parent)
        {
            _audioMeterInformation = parent;
        }

        /// <summary>
        ///     Metering Channel Count
        /// </summary>
        public int Count
        {
            get
            {
                return ComThread.Invoke(() =>
                {
                    uint result;
                    Marshal.ThrowExceptionForHR(_audioMeterInformation.GetMeteringChannelCount(out result));
                    return Convert.ToInt32(result);
                });
            }
        }

        /// <summary>
        ///     Get Peak value
        /// </summary>
        /// <param name="index">Channel index</param>
        /// <returns>Peak value</returns>
        public float this[int index]
        {
            get
            {
                return ComThread.Invoke(() =>
                {
                    var peakValues = new float[Count];
                    Marshal.ThrowExceptionForHR(
                        _audioMeterInformation.GetChannelsPeakValues(Convert.ToUInt32(peakValues.Length), peakValues));
                    return peakValues[index];
                });
            }
        }
    }
}