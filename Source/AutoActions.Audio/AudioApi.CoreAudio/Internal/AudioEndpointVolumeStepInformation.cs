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

using System.Runtime.InteropServices;
using AudioSwitcher.AudioApi.CoreAudio.Interfaces;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    /// <summary>
    ///     Audio Endpoint Volume Step Information
    /// </summary>
    internal class AudioEndpointVolumeStepInformation
    {
        private readonly uint _step;
        private readonly uint _stepCount;

        internal AudioEndpointVolumeStepInformation(IAudioEndpointVolume parent)
        {
            Marshal.ThrowExceptionForHR(parent.GetVolumeStepInfo(out _step, out _stepCount));
        }

        /// <summary>
        ///     Step
        /// </summary>
        public uint Step
        {
            get { return _step; }
        }

        /// <summary>
        ///     StepCount
        /// </summary>
        public uint StepCount
        {
            get { return _stepCount; }
        }
    }
}