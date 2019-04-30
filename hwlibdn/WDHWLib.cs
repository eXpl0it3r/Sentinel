using System;
using System.Runtime.InteropServices;

namespace WDHWLibDn
{
	public class WDHWLib
	{
		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr HWOpenHandle();

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void HWCloseHandle(IntPtr handle);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWBeep(IntPtr handle, uint freq, uint duration_ms);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetSysInfo(IntPtr handle, ref WDHWLib.SYS_INFO info);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetGenPortData(IntPtr handle, ref WDHWLib.GEN_IO_PORT_DATA data);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWSetGenPortData(IntPtr handle, ref WDHWLib.GEN_IO_PORT_DATA data);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWSetParPortData(IntPtr handle, byte data);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWSetParPortControl(IntPtr handle, byte ctrl);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetPowerSupplyStatus(IntPtr handle, WDHWLib.POWER_SUPPLY_TYPE type, ref bool ps);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetButtonStatus(IntPtr handle, WDHWLib.BUTTON_TYPE type, ref bool btn);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWWaitForButtonChange(IntPtr handle, ref WDHWLib.READ_BUTTON_DATA data);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWCancelWaitForButtonChange(IntPtr handle);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetNumCpu(IntPtr handle, ref int cpus);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetCpuDTS(IntPtr handle, int cpu, ref WDHWLib.DTS_DATA dts);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWIsPowerButtonPressed(IntPtr handle, ref bool pwrbtn);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetLed(IntPtr handle, int led, WDHWLib.LED_TYPE type, WDHWLib.LED_COLOR color, ref bool on);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWSetLed(IntPtr handle, int led, WDHWLib.LED_TYPE type, WDHWLib.LED_COLOR color, bool on);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetLedBlink(IntPtr handle, int led, WDHWLib.LED_TYPE type, WDHWLib.LED_COLOR color, ref bool on);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWSetLedBlink(IntPtr handle, int led, WDHWLib.LED_TYPE type, WDHWLib.LED_COLOR color, bool on);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetTemperature(IntPtr handle, int tempIndex, WDHWLib.TEMPERATURE_TYPE type, ref float temp);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetFanRPM(IntPtr handle, int fan, WDHWLib.FAN_TYPE type, ref int rpm);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWSetFanSpeed(IntPtr handle, int fan, WDHWLib.FAN_TYPE type, byte speed);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetLcdBacklit(IntPtr handle, ref byte backlit);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWSetLcdBacklit(IntPtr handle, byte backlit);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetBIOSType(IntPtr handle, ref WDHWLib.BIOS_TYPE type);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWSetBIOSType(IntPtr handle, WDHWLib.BIOS_TYPE type);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetBMCInfo(IntPtr handle, ref WDHWLib.BMC_INFO info);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetBMCMacAddress(IntPtr handle, ref ulong mac);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetBootDriveBayStatus(IntPtr handle, uint i, ref WDHWLib.DRIVE_BAY_STATUS_DATA status);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWGetDriveBayStatus(IntPtr handle, uint i, ref WDHWLib.DRIVE_BAY_STATUS_DATA status);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWSetDriveBayPower(IntPtr handle, uint i, bool on);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWWaitForDriveBayChange(IntPtr handle, ref WDHWLib.DRIVE_BAY_PRESENCE_DATA data);

		[DllImport("hwlib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern WDHWLib.HWStatus HWCancelWaitForDriveBayChange(IntPtr handle);

		public const byte GEN_IO_PORT_TYPE_UCHAR = 0;

		public const byte GEN_IO_PORT_TYPE_USHORT = 1;

		public const byte GEN_IO_PORT_TYPE_ULONG = 2;

		public enum HWStatus
		{
			HW_STATUS_OK,
			HW_STATUS_INVALID_HANDLE,
			HW_STATUS_ERROR,
			HW_STATUS_TIMEOUT,
			HW_STATUS_FAIL_OPEN_DRIVER,
			HW_STATUS_INVALID_PARAM
		}

		public enum POWER_SUPPLY_TYPE
		{
			POWER_SUPPLY_1,
			POWER_SUPPLY_2
		}

		public enum BUTTON_TYPE
		{
			BUTTON_1,
			BUTTON_2
		}

		public enum LED_TYPE
		{
			LED_SYSTEM,
			LED_DRIVE
		}

		public enum LED_COLOR
		{
			LED_PRIMARY,
			LED_SECONDARY
		}

		public enum TEMPERATURE_TYPE
		{
			SYS_TEMPERATURE = 1,
			CHIPSET_TEMPERATURE,
			MEMORY_TEMPERATURE,
			RAID_CHIP_TEMPERATURE
		}

		public enum FAN_TYPE
		{
			SYS_FAN,
			CPU_FAN
		}

		public enum BIOS_TYPE
		{
			BIOS_MAIN,
			BIOS_BACKUP
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct GEN_IO_PORT_DATA
		{
			[FieldOffset(0)]
			public uint port;

			[FieldOffset(4)]
			public byte type;

			[FieldOffset(5)]
			public uint dataUlong;

			[FieldOffset(5)]
			public ushort dataUshort;

			[FieldOffset(5)]
			public byte dataUchar;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct READ_BUTTON_DATA
		{
			public WDHWLib.BUTTON_TYPE button;

			public uint reserved;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct DTS_DATA
		{
			public uint rawValue;

			public byte valid;

			public byte dtsValue;

			public byte interprettedDtsValue;

			public byte critTempStatus;

			public byte tempStatus;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct DRIVE_BAY_STATUS_DATA
		{
			public uint bay;

			public byte present;

			public byte power;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct DRIVE_BAY_PRESENCE_DATA
		{
			public uint presenceChanged;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SYS_INFO
		{
			public byte backplaneId;

			public uint numBays;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BMC_INFO
		{
			public byte version;

			public byte mailboxVersion;
		}
	}
}
