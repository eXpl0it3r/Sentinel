using System;
using WDHWLibDn;

namespace SentinelConsole
{
    public class HardwareHandler : IDisposable
    {
        public IntPtr Handle { get; private set; }

        public HardwareHandler()
        {
            Handle = WDHWLib.HWOpenHandle();

            if (Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Could not retrieve a HWLib handle");
            }
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                WDHWLib.HWCloseHandle(Handle);
            }
        }
    }
}