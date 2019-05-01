using System;
using System.Threading;
using WDHWLibDn;

namespace SentinelConsole
{
    public class SentinelService
    {
        private const uint RetryStrategy = 3;

        public void Beep(uint frequency, TimeSpan duration)
        {
            if (frequency > 32000)
            {
                throw  new ArgumentException($"The provided frequency '{frequency}'Hz is outside of the accepted range [0-32000]Hz");
            }
            if (duration <= TimeSpan.Zero || duration > TimeSpan.FromSeconds(10))
            {
                throw new ArgumentException($"The provided duration '{duration.Milliseconds}'ms is outside of the allowed range ]0-10]s");
            }

            using (var handler = new HardwareHandler())
            {
                WDHWLib.HWBeep(handler.Handle, frequency, (uint)Math.Abs(duration.Milliseconds));
            }
        }

        public SystemInfo GetSystemInfo()
        {
            using (var handler = new HardwareHandler())
            {
                var systemInfo = default(WDHWLib.SYS_INFO);
                WDHWLib.HWGetSysInfo(handler.Handle, ref systemInfo);
                return new SystemInfo
                {
                    BackplaneId = systemInfo.backplaneId,
                    NumberOfBays = systemInfo.numBays
                };
            }
        }

        public int GetNumberOfProcessors()
        {
            using (var handler = new HardwareHandler())
            {
                return NumberOfProcessors(handler);
            }
        }

        public DigitalTemperatureSensorData GetProcessorTemperature(int processorIndex)
        {
            using (var handler = new HardwareHandler())
            {
                var numberOfProcessors = NumberOfProcessors(handler);
                if (processorIndex >= numberOfProcessors)
                {
                    throw new ArgumentException($"The processor index '{processorIndex}' exceeds the maximum number of processors {numberOfProcessors}");
                }

                var hardwareStatus = WDHWLib.HWStatus.HW_STATUS_ERROR;
                var digitalTemperatureSensorData = default(WDHWLib.DTS_DATA);

                for (var i = 0; i < RetryStrategy; i++)
                {
                    hardwareStatus = WDHWLib.HWGetCpuDTS(handler.Handle, processorIndex, ref digitalTemperatureSensorData);
                    if (hardwareStatus == WDHWLib.HWStatus.HW_STATUS_OK)
                    {
                        break;
                    }
                    Thread.Sleep(100);
                }
                if (hardwareStatus != WDHWLib.HWStatus.HW_STATUS_OK)
                {
                    throw new TimeoutException($"Processor temperature for CPU #'{processorIndex}' could not be retrieved after {RetryStrategy} retries");
                }

                return new DigitalTemperatureSensorData
                {
                    RawValue = digitalTemperatureSensorData.rawValue,
                    Valid = digitalTemperatureSensorData.valid,
                    DtsValue = digitalTemperatureSensorData.dtsValue,
                    InterprettedDtsValue = digitalTemperatureSensorData.interprettedDtsValue,
                    CriticalTemperatureStatus = digitalTemperatureSensorData.critTempStatus,
                    TemperatureStatus = digitalTemperatureSensorData.tempStatus,
                };
            }
        }

        private static int NumberOfProcessors(HardwareHandler handler)
        {
            var numberOfProcessors = default(int);
            WDHWLib.HWGetNumCpu(handler.Handle, ref numberOfProcessors);
            return numberOfProcessors;
        }
    }
}
