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
// updated for Audio Switcher

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AudioSwitcher.AudioApi.CoreAudio.Interfaces;
using AudioSwitcher.AudioApi.CoreAudio.Threading;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    /// <summary>
    ///     Multimedia Device Collection
    /// </summary>
    internal class MMDeviceCollection : IEnumerable<IMMDevice>, IDisposable
    {
        private IMMDeviceCollection _mmDeviceCollection;

        internal MMDeviceCollection(IMMDeviceCollection parent)
        {
            _mmDeviceCollection = parent;
        }

        #region IEnumerable<MMDevice> Members

        /// <summary>
        ///     Get Controller
        /// </summary>
        /// <returns>Device enumerator</returns>
        public IEnumerator<IMMDevice> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
            {
                yield return this[index];
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        ///     Device count
        /// </summary>
        public int Count
        {
            get
            {
                return ComThread.Invoke(() =>
                {
                    uint result;
                    Marshal.ThrowExceptionForHR(_mmDeviceCollection.GetCount(out result));
                    return Convert.ToInt32(result);

                });
            }
        }

        /// <summary>
        ///     Get device by index
        /// </summary>
        /// <param name="index">Device index</param>
        /// <returns>Device at the specified index</returns>
        public IMMDevice this[int index]
        {
            get
            {
                return ComThread.Invoke(() =>
                {
                    IMMDevice result;
                    _mmDeviceCollection.Item(Convert.ToUInt32(index), out result);
                    return result;
                });
            }
        }

        public void Dispose()
        {
            _mmDeviceCollection = null;
        }
    }
}