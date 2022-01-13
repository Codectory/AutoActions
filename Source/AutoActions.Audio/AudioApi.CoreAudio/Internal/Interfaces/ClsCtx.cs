using System;

namespace AudioSwitcher.AudioApi.CoreAudio.Interfaces
{
    /// <summary>
    ///     is defined in WTypes.h
    /// </summary>
    [Flags]
    internal enum ClsCtx: uint
    {
        InprocServer = 0x1,
        InprocHandler = 0x2,
        LocalServer = 0x4,
        InprocServer16 = 0x8,
        RemoteServer = 0x10,
        InprocHandler16 = 0x20,
        //RESERVED1	= 0x40,
        //RESERVED2	= 0x80,
        //RESERVED3	= 0x100,
        //RESERVED4	= 0x200,
        NoCodeDownload = 0x400,
        //RESERVED5	= 0x800,
        NoCustomMarshal = 0x1000,
        EnableCodeDownload = 0x2000,
        NoFailureLog = 0x4000,
        DisableAaa = 0x8000,
        EnableAaa = 0x10000,
        FromDefaultContext = 0x20000,
        Activate32BitServer = 0x40000,
        Activate64BitServer = 0x80000,
        EnableCloaking = 0x100000,
        PsDll = 0x80000000,
        Inproc = InprocServer | InprocHandler,
        Server = InprocServer | LocalServer | RemoteServer,
        All = Server | InprocHandler
    }
}