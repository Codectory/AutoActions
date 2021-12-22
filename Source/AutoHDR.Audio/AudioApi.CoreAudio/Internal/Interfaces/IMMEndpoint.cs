using System.Runtime.InteropServices;

namespace AudioSwitcher.AudioApi.CoreAudio.Interfaces
{
    [Guid(ComIIds.IMM_ENDPOINT_IID)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMEndpoint
    {
        int GetDataFlow([Out] [MarshalAs(UnmanagedType.I4)] out EDataFlow dataFlow);
    }
}
