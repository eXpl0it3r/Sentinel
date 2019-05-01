namespace SentinelConsole
{
    public class DigitalTemperatureSensorData
    {
        public uint RawValue { get; set; }
        public byte Valid { get; set; }
        public byte DtsValue { get; set; }
        public byte InterprettedDtsValue { get; set; }
        public byte CriticalTemperatureStatus { get; set; }
        public byte TemperatureStatus { get; set; }
    }
}