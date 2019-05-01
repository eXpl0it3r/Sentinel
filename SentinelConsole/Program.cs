using System;

namespace SentinelConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var sentinelService = new SentinelService();
            sentinelService.Beep(18000, TimeSpan.FromSeconds(1));

            var systemInfo = sentinelService.GetSystemInfo();
            var numberOfProcessors = sentinelService.GetNumberOfProcessors();
            Console.Out.WriteLine($"Motherboard Id: {systemInfo.BackplaneId}");
            Console.Out.WriteLine($"Number of Bays: {systemInfo.NumberOfBays}");
            Console.Out.WriteLine($"Number of CPUs: {numberOfProcessors}");

            for (var index = 0; index < numberOfProcessors; index++)
            {
                var temperatureData = sentinelService.GetProcessorTemperature(index);
                Console.Out.WriteLine($"  Processor #{index}: {temperatureData.InterprettedDtsValue}°C\n" +
                                      $"    Raw: {temperatureData.RawValue}\n" +
                                      $"    DTS Value: {temperatureData.DtsValue}\n" +
                                      $"    Valid: {temperatureData.Valid}\n" +
                                      $"    Status: {temperatureData.TemperatureStatus}\n" +
                                      $"    Critical Status: {temperatureData.CriticalTemperatureStatus}");
            }
        }
    }
}
