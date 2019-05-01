using System;
using System.Runtime.InteropServices;

namespace Stor
{
	public class Ata
	{
		public static short[] ByteArrayToShortArray(byte[] barray)
		{
			short[] array = null;
			if (barray == null)
			{
				return array;
			}
			int num = barray.Length / 2;
			array = new short[num];
			for (int i = 0; i < num; i++)
			{
				int num2 = i * 2;
				array[i] = (short)((int)barray[num2 + 1] << 8 | (int)barray[num2]);
				Storage.Debug("short[{0}] = {1:X}  =>  byte[{2},{3}] = {4:X} {5:X}", new object[]
				{
					i,
					array[i],
					num2 + 1,
					num2,
					barray[num2 + 1],
					barray[num2]
				});
			}
			return array;
		}

		public static byte[] StructToByteArray<T>(T t, int byteArraySize)
		{
			IntPtr intPtr = IntPtr.Zero;
			if (Marshal.SizeOf(t) > byteArraySize)
			{
				return null;
			}
			byte[] array = new byte[byteArraySize];
			intPtr = Marshal.AllocHGlobal(byteArraySize);
			MemoryWrapper.FillMemory(intPtr, (uint)byteArraySize, 0);
			Marshal.StructureToPtr(t, intPtr, false);
			Marshal.Copy(intPtr, array, 0, byteArraySize);
			Marshal.FreeHGlobal(intPtr);
			return array;
		}

		public static T ByteArrayToStruct<T>(byte[] bbuf)
		{
			int num = Marshal.SizeOf(typeof(T));
			if (bbuf.Length < num)
			{
				return default(T);
			}
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			Marshal.Copy(bbuf, 0, intPtr, num);
			T result = (T)((object)Marshal.PtrToStructure(intPtr, typeof(T)));
			Marshal.FreeHGlobal(intPtr);
			return result;
		}

		public const short ONE_PAGE = 512;

		public const short ONE_PAGE_WORDS = 256;

		public const byte ATA_CMD_SMART = 176;

		public const byte ATA_CMD_IDENTIFY = 236;

		public const int ID_COMMANDS_AND_FEATURES = 85;

		public const int ID_COMMANDS_AND_FEATURES_NO_WORDS = 1;

		public const int ID_SMART_ENABLED = 1;

		public const int ID_SSC_STATUS_OFFSET = 137;

		public const int ID_SSC_STATUS_OFFSET_NO_WORDS = 1;

		public const int ID_SSC_ENABLED_BIT = 2048;

		public const byte SMART_CTRL_KEY = 190;

		public const byte SMART_DATA_KEY = 191;

		public const byte kAC_DriveConfig = 55;

		public const byte eSetUserKeyMode = 2;

		public const byte eGetUserKeyMode = 11;

		public const byte TLER_FEAT = 5;

		public const byte TLER_FEAT_ENABLE = 1;

		public const byte TLER_FEAT_ENABLE_TYPE = 1;

		public const byte TLER_FEAT_ENABLE_SIZE = 1;

		public const byte TLER_FEAT_WRITER = 2;

		public const byte TLER_FEAT_READER = 3;

		public const byte TLER_FEAT_RDWR_TYPE = 3;

		public const byte TLER_FEAT_RDWR_SIZE = 2;
	}
}
