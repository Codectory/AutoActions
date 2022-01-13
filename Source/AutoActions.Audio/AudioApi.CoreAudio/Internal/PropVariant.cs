﻿/*
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
// adapted for use in NAudio

using System;
using System.IO;
using System.Runtime.InteropServices;
using AudioSwitcher.AudioApi.CoreAudio.Interfaces;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    /// <summary>
    ///     from Propidl.h.
    ///     http://msdn.microsoft.com/en-us/library/aa380072(VS.85).aspx
    ///     contains a union so we have to do an explicit layout
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariant
    {
        [FieldOffset(0)] private short vt;
        [FieldOffset(2)] private readonly short wReserved1;
        [FieldOffset(4)] private readonly short wReserved2;
        [FieldOffset(6)] private readonly short wReserved3;
        [FieldOffset(8)] private readonly sbyte cVal;
        [FieldOffset(8)] private readonly byte bVal;
        [FieldOffset(8)] private readonly short iVal;
        [FieldOffset(8)] private readonly ushort uiVal;
        [FieldOffset(8)] private readonly int lVal;
        [FieldOffset(8)] private readonly uint ulVal;
        [FieldOffset(8)] private readonly int intVal;
        [FieldOffset(8)] private readonly uint uintVal;
        [FieldOffset(8)] private long hVal;
        [FieldOffset(8)] private readonly long uhVal;
        [FieldOffset(8)] private readonly float fltVal;
        [FieldOffset(8)] private readonly double dblVal;
        [FieldOffset(8)] private readonly bool boolVal;
        [FieldOffset(8)] private readonly int scode;
        //CY cyVal;
        [FieldOffset(8)] private readonly DateTime date;
        [FieldOffset(8)] private readonly FILETIME filetime;
        //CLSID* puuid;
        //CLIPDATA* pclipdata;
        //BSTR bstrVal;
        //BSTRBLOB bstrblobVal;
        [FieldOffset(8)] private Blob blobVal;
        //LPSTR pszVal;
        [FieldOffset(8)] private IntPtr pointerValue; //LPWSTR 
        //IUnknown* punkVal;
        /*IDispatch* pdispVal;
        IStream* pStream;
        IStorage* pStorage;
        LPVERSIONEDSTREAM pVersionedStream;
        LPSAFEARRAY parray;
        CAC cac;
        CAUB caub;
        CAI cai;
        CAUI caui;
        CAL cal;
        CAUL caul;
        CAH cah;
        CAUH cauh;
        CAFLT caflt;
        CADBL cadbl;
        CABOOL cabool;
        CASCODE cascode;
        CACY cacy;
        CADATE cadate;
        CAFILETIME cafiletime;
        CACLSID cauuid;
        CACLIPDATA caclipdata;
        CABSTR cabstr;
        CABSTRBLOB cabstrblob;
        CALPSTR calpstr;
        CALPWSTR calpwstr;
        CAPROPVARIANT capropvar;
        CHAR* pcVal;
        UCHAR* pbVal;
        SHORT* piVal;
        USHORT* puiVal;
        LONG* plVal;
        ULONG* pulVal;
        INT* pintVal;
        UINT* puintVal;
        FLOAT* pfltVal;
        DOUBLE* pdblVal;
        VARIANT_BOOL* pboolVal;
        DECIMAL* pdecVal;
        SCODE* pscode;
        CY* pcyVal;
        DATE* pdate;
        BSTR* pbstrVal;
        IUnknown** ppunkVal;
        IDispatch** ppdispVal;
        LPSAFEARRAY* pparray;
        PROPVARIANT* pvarVal;
        */

        /// <summary>
        ///     Creates a new PropVariant containing a long value
        /// </summary>
        public static PropVariant FromLong(long value)
        {
            return new PropVariant {vt = (short) VarEnum.VT_I8, hVal = value};
        }

        /// <summary>
        ///     Helper method to gets blob data
        /// </summary>
        private byte[] GetBlob()
        {
            var blob = new byte[blobVal.Length];
            Marshal.Copy(blobVal.Data, blob, 0, blob.Length);
            return blob;
        }

        /// <summary>
        ///     Interprets a blob as an array of structs
        /// </summary>
        public T[] GetBlobAsArrayOf<T>()
        {
            int blobByteLength = blobVal.Length;
            var singleInstance = (T) Activator.CreateInstance(typeof (T));
            int structSize = Marshal.SizeOf(singleInstance);
            if (blobByteLength%structSize != 0)
            {
                throw new InvalidDataException(String.Format("Blob size {0} not a multiple of struct size {1}",
                    blobByteLength, structSize));
            }
            int items = blobByteLength/structSize;
            var array = new T[items];
            for (int n = 0; n < items; n++)
            {
                array[n] = (T) Activator.CreateInstance(typeof (T));
                Marshal.PtrToStructure(new IntPtr((long) blobVal.Data + n*structSize), array[n]);
            }
            return array;
        }

        /// <summary>
        ///     Gets the type of data in this PropVariant
        /// </summary>
        public VarEnum DataType
        {
            get { return (VarEnum) vt; }
        }

        /// <summary>
        ///     Property value
        /// </summary>
        public object Value
        {
            get
            {
                VarEnum ve = DataType;
                switch (ve)
                {
                    case VarEnum.VT_I1:
                        return bVal;
                    case VarEnum.VT_I2:
                        return iVal;
                    case VarEnum.VT_I4:
                        return lVal;
                    case VarEnum.VT_I8:
                        return hVal;
                    case VarEnum.VT_INT:
                        return iVal;
                    case VarEnum.VT_UI4:
                        return ulVal;
                    case VarEnum.VT_UI8:
                        return uhVal;
                    case VarEnum.VT_BOOL:
                        return boolVal;
                    case VarEnum.VT_LPWSTR:
                        return Marshal.PtrToStringUni(pointerValue);
                    //case VarEnum.VT_BLOB:
                    //case VarEnum.VT_VECTOR:
                    //case VarEnum.VT_UI1:
                    //    return GetBlob();
                    case VarEnum.VT_CLSID:
                        return (Guid) Marshal.PtrToStructure(pointerValue, typeof (Guid));
                }
                throw new NotSupportedException("PropVariant " + ve);
            }
            set
            {
                if (value is string && DataType == VarEnum.VT_LPWSTR)
                {
                    pointerValue = Marshal.StringToBSTR(value as string);
                }
            }
        }

        public bool IsSupported()
        {
            switch (DataType)
            {
                case VarEnum.VT_I1:
                    return true;
                case VarEnum.VT_I2:
                    return true;
                case VarEnum.VT_I4:
                    return true;
                case VarEnum.VT_I8:
                    return true;
                case VarEnum.VT_INT:
                    return true;
                case VarEnum.VT_UI4:
                    return true;
                case VarEnum.VT_UI8:
                    return true;
                case VarEnum.VT_BOOL:
                    return true;
                case VarEnum.VT_LPWSTR:
                    return true;
                case VarEnum.VT_CLSID:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     allows freeing up memory, might turn this into a Dispose method?
        /// </summary>
        public void Clear()
        {
            NativeMethods.PropVariantClear(ref this);
        }
    }
}