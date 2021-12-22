using System;

namespace AudioSwitcher.AudioApi.CoreAudio
{
    internal interface IPropertyDictionary : IDisposable
    {

        AccessMode Mode { get; }

        int Count { get; }

        object this[PropertyKey key]
        {
            get;
            set;
        }

        bool Contains(PropertyKey key);

    }

    [Flags]
    internal enum AccessMode
    {
        Read,
        Write,
        ReadWrite = Read | Write
    }
}