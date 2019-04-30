using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MvApi
{
	public class MvApi
	{
		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_API_Initialize();

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_API_Finalize();

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_API_RescanAdapter();

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_Adapter_GetCount();

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_Adapter_GetInfo(byte AdapterID, ref byte Count, ref MvApi.Adapter_Info pAdapterInfo);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_PD_GetHDInfo_Ext(byte AdapterID, ref MvApi.HD_Info_Request pHdInfoReq);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_DiskHasOS(byte AdapterID, byte type, byte count, short[] ID);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_GetInfo(byte adapterId, ref MvApi.LD_Info_Request pLdInfoReq);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_GetStatus(byte adapterId, ref MvApi.LD_Status_Request pLdStatusReq);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_GetTargetLDInfo(byte adapterId, short LD_ID, ref MvApi.LD_Info pLDInfo);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_GetConfig(byte adapterId, ref MvApi.LD_Config_Request pLDConfigReq);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_BLK_GetInfo(byte adapterId, ref MvApi.Block_Info_Request pBlkInfoReq);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_PassThrough_ATA(byte AdapterID, IntPtr pAtaRegs, short ID);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_PD_GetConfig(byte adapterId, ref MvApi.HD_Config_Request pHdConfigReq);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_PD_SetConfig(byte adapterId, short hdId, IntPtr pHdConfig);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_PD_GET_SMART_INFO(byte adapterID, short PD_ID, ref MvApi.SMART_Info_Request pSmartInfo);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_PD_GetPMInfo(byte adapterId, ref MvApi.PM_Info_Request pPmInfoReq);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_StartConsistencyCheck(byte adapterId, short LD_ID, byte Type);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_StartINIT(byte adapterId, short LD_ID, byte Init_Type);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_StartMigration(byte adapterId, ref MvApi.Create_LD_Param pCreateLDParam);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_StartRebuild(byte adapterId, short LD_ID, short Rebuild_HD_ID);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_Delete(byte adapterId, short LD_ID, byte deleteMBR);

		[DllImport("mv_api.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern byte MV_LD_Create(byte adapterId, ref MvApi.Create_LD_Param pCreateLDParam);

		public static void SetDebug(bool on)
		{
			MvApi.DebugOn = on;
		}

		public static void Debug(string fmt, params object[] args)
		{
			if (MvApi.DebugOn)
			{
				Console.WriteLine(fmt, args);
			}
		}

		public static byte MV_InfoRequest_Helper<T>(Func<byte, IntPtr, byte> func, byte id, T req)
		{
			IntPtr intPtr = IntPtr.Zero;
			FieldInfo field = req.GetType().GetField("header");
			if (field == null)
			{
				return 150;
			}
			if (((MvApi.RequestHeader)field.GetValue(req)).numRequested != 1)
			{
				return 57;
			}
			intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(req));
			if (intPtr == IntPtr.Zero)
			{
				return 1;
			}
			Marshal.StructureToPtr(req, intPtr, false);
			byte b = func(id, intPtr);
			if (b == 0)
			{
				Marshal.PtrToStructure(intPtr, req);
			}
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
			return b;
		}

		public static byte MV_PassThrough_ATA_Helper(byte AdapterID, short DriveID, ref MvApi.ATA_REGS pAtaRegs, ref byte[] data)
		{
			byte b = 0;
			IntPtr intPtr = IntPtr.Zero;
			int num = 0;
			if (data != null)
			{
				num = data.Length;
			}
			if ((int)pAtaRegs.buffer_size != num)
			{
				b = 7;
				return b;
			}
			int num2 = Marshal.SizeOf(typeof(MvApi.ATA_REGS));
			if (num > 0)
			{
				num2 = num2 - 1 + num;
			}
			intPtr = Marshal.AllocHGlobal(num2);
			if (intPtr == IntPtr.Zero)
			{
				return 1;
			}
			MemoryWrapper.FillMemory(intPtr, (uint)num2, 0);
			Marshal.StructureToPtr(pAtaRegs, intPtr, false);
			if (num > 0)
			{
				MvApi.Debug("pATA = {0}, sizeof(ATA_REGS)={1}", new object[]
				{
					intPtr.ToInt64(),
					Marshal.SizeOf(typeof(MvApi.ATA_REGS))
				});
				IntPtr destination = IntPtr.Add(intPtr, Marshal.SizeOf(typeof(MvApi.ATA_REGS)) - 1);
				Marshal.Copy(data, 0, destination, num);
				MvApi.Debug("Copied {0} bytes to {1}", new object[]
				{
					num,
					destination.ToInt64()
				});
			}
			if (MvApi.DebugOn)
			{
				byte[] array = new byte[num2];
				Marshal.Copy(intPtr, array, 0, num2);
				MvApi.Debug("ATA_REGS", new object[0]);
				for (int i = 0; i < array.Length; i++)
				{
					MvApi.Debug(" {2}  {0,4}: {1:X}", new object[]
					{
						i,
						array[i],
						intPtr.ToInt64() + (long)i
					});
				}
			}
			try
			{
				b = MvApi.MV_PassThrough_ATA(AdapterID, intPtr, DriveID);
			}
			catch (Exception ex)
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
				throw ex;
			}
			MvApi.Debug("MV_PassThrough_ATA returned {0}", new object[]
			{
				b
			});
			if (MvApi.DebugOn)
			{
				byte[] array2 = new byte[Marshal.SizeOf(typeof(MvApi.ATA_REGS))];
				Marshal.Copy(intPtr, array2, 0, Marshal.SizeOf(typeof(MvApi.ATA_REGS)));
				MvApi.Debug("Returned ATA_REGS", new object[0]);
				for (int j = 0; j < array2.Length; j++)
				{
					MvApi.Debug(" {2}  {0,4}: {1:X}", new object[]
					{
						j,
						array2[j],
						intPtr.ToInt64() + (long)j
					});
				}
			}
			if (b == 0)
			{
				pAtaRegs = (MvApi.ATA_REGS)Marshal.PtrToStructure(intPtr, typeof(MvApi.ATA_REGS));
				if (num > 0)
				{
					IntPtr source = IntPtr.Add(intPtr, Marshal.SizeOf(typeof(MvApi.ATA_REGS)) - 1);
					Marshal.Copy(source, data, 0, num);
				}
			}
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
			return b;
		}

		public const byte MV_TRUE = 1;

		public const byte MV_FALSE = 0;

		public const byte ERR_AES = 240;

		public const byte ERR_API = 150;

		public const byte ERR_BGA_RUNNING = 63;

		public const byte ERR_COMMAND_NOT_SUPPORTED = 160;

		public const byte ERR_COMMAND_PHASE_ERROR = 23;

		public const byte ERR_CORE = 100;

		public const byte ERR_COUNT_OUT_OF_RANGE = 163;

		public const byte ERR_DATA_PHASE_ERROR = 24;

		public const byte ERR_DEVICE_IS_BUSY = 181;

		public const byte ERR_DG_HAS_MISSING_PD = 79;

		public const byte ERR_DISK_HAS_RUNNING_OS = 159;

		public const byte ERR_DRIVER_SENSOR = 20;

		public const byte ERR_ENTRY_NO_KEY = 250;

		public const byte ERR_ENTRY_OUT_OF_RANGE = 241;

		public const byte ERR_FAIL = 1;

		public const byte ERR_FLO = 200;

		public const byte ERR_GENERIC = 2;

		public const byte ERR_HAS_BGA_IN_DG = 82;

		public const byte ERR_HAS_BGA_IN_VD = 61;

		public const byte ERR_HAS_LD_IN_DG = 76;

		public const byte ERR_HAS_MIGRATION_ON_DG = 81;

		public const byte ERR_HD_CANNOT_SET_DOWN = 83;

		public const byte ERR_HD_IN_DIFF_CARD = 154;

		public const byte ERR_HD_IS_ASSIGNED_ALREADY = 56;

		public const byte ERR_HD_IS_NOT_SPARE = 53;

		public const byte ERR_HD_IS_SPARE = 54;

		public const byte ERR_HD_NOT_EXIST = 55;

		public const byte ERR_HD_NOT_OFFLINE = 84;

		public const byte ERR_HD_SECTOR_SIZE_MISMATCH = 74;

		public const byte ERR_HD_TYPE_MISMATCH = 73;

		public const byte ERR_INVALID_ADAPTER_ID = 13;

		public const byte ERR_INVALID_BGA_ACTION = 153;

		public const byte ERR_INVALID_BLOCK_ID = 12;

		public const byte ERR_INVALID_BLOCK_SIZE = 67;

		public const byte ERR_INVALID_BU_ID = 16;

		public const byte ERR_INVALID_DG_ID = 17;

		public const byte ERR_INVALID_ENC_ELEMENT_ID = 18;

		public const byte ERR_INVALID_ENC_ID = 15;

		public const byte ERR_INVALID_ERASE_HDD = 29;

		public const byte ERR_INVALID_EXP_ID = 10;

		public const byte ERR_INVALID_FILE = 165;

		public const byte ERR_INVALID_FLASH_ACTION = 156;

		public const byte ERR_INVALID_FLASH_DATA = 184;

		public const byte ERR_INVALID_FLASH_DESCRIPTOR = 185;

		public const byte ERR_INVALID_FLASH_TYPE = 155;

		public const byte ERR_INVALID_HD_COUNT = 57;

		public const byte ERR_INVALID_HD_ID = 9;

		public const byte ERR_INVALID_HDCOUNT = 152;

		public const byte ERR_INVALID_KEY_ABSENT = 22;

		public const byte ERR_INVALID_KEY_LENGTH = 245;

		public const byte ERR_INVALID_KEY_PRESENT = 21;

		public const byte ERR_INVALID_LD_ID = 8;

		public const byte ERR_INVALID_MATCH_ID = 151;

		public const byte ERR_INVALID_MICROCODE = 166;

		public const byte ERR_INVALID_NUM_REQUESTED = 243;

		public const byte ERR_INVALID_PARAMETER = 7;

		public const byte ERR_INVALID_PM_ID = 11;

		public const byte ERR_INVALID_RAID_MODE = 14;

		public const byte ERR_INVALID_RAID6_PARITY_DISK_COUNT = 66;

		public const byte ERR_INVALID_REQUEST = 6;

		public const byte ERR_INVALID_REQUEST_TYPE = 244;

		public const byte ERR_INVALID_SSD_NUM = 28;

		public const byte ERR_IOCTL_NO_RESOURCE = 164;

		public const byte ERR_KEY_MISMATCH = 247;

		public const byte ERR_LD_IS_FUNCTIONAL = 60;

		public const byte ERR_LD_NAME_INVALID = 72;

		public const byte ERR_LD_NO_ATAPI = 65;

		public const byte ERR_LD_NOT_EXIST = 59;

		public const byte ERR_LD_NOT_FULLY_INITED = 71;

		public const byte ERR_LD_NOT_IMPORTABLE = 80;

		public const byte ERR_LD_NOT_READY = 58;

		public const byte ERR_LD_NOT_REPORTABLE = 86;

		public const byte ERR_LD_STATUS_WRONG = 85;

		public const byte ERR_LOAD_LOKI_API_FAIL = 183;

		public const byte ERR_MIGRATION_LIMIT = 161;

		public const byte ERR_MIGRATION_NOT_NEED = 68;

		public const byte ERR_MIGRATION_NOT_SUPPORT = 70;

		public const byte ERR_NEED_RESCAN = 186;

		public const byte ERR_NO_BGA_ACTIVITY = 62;

		public const byte ERR_NO_LD_IN_DG = 75;

		public const byte ERR_NO_RESOURCE = 4;

		public const byte ERR_NO_ROOM_FOR_SPARE = 77;

		public const byte ERR_NONE = 0;

		public const byte ERR_NOT_OFFLINE_DISK = 246;

		public const byte ERR_NOT_SUPPORTED = 19;

		public const byte ERR_PASSWORD_MISMATCH = 248;

		public const byte ERR_PORT_ID_NOT_FOUND = 249;

		public const byte ERR_PORT_OUT_OF_RANGE = 242;

		public const byte ERR_RAID = 50;

		public const byte ERR_RAID_NO_AVAILABLE_ID = 64;

		public const byte ERR_RAID_NOT_REDUNDANT = 87;

		public const byte ERR_REGISTER_WRITING = 30;

		public const byte ERR_REQ_OUT_OF_RANGE = 5;

		public const byte ERR_RESCANING = 187;

		public const byte ERR_SGPIO_CONTROL_NOT_SUPPORTED = 162;

		public const byte ERR_SHELL_CMD_FAIL = 182;

		public const byte ERR_SPARE_IS_IN_MULTI_DG = 78;

		public const byte ERR_STATUS_PHASE_CMD_ERROR = 27;

		public const byte ERR_STATUS_PHASE_ERROR = 25;

		public const byte ERR_STATUS_PHASE_PHASE_ERROR = 26;

		public const byte ERR_STRIPE_BLOCK_SIZE_MISMATCH = 69;

		public const byte ERR_TARGET_IN_LD_FUNCTIONAL = 51;

		public const byte ERR_TARGET_NO_ENOUGH_SPACE = 52;

		public const byte ERR_TOO_FEW_EVENT = 157;

		public const byte ERR_UNKNOWN = 3;

		public const byte ERR_USER_INUSE = 169;

		public const byte ERR_USER_NOT_FOUND = 167;

		public const byte ERR_USER_NOT_INUSE = 168;

		public const byte ERR_VD_HAS_RUNNING_OS = 158;

		public const int NO_MORE_DATA = 65535;

		public const byte REQUEST_BY_RANGE = 1;

		public const byte REQUEST_BY_ID = 2;

		public const int MAX_NUM_ADAPTERS = 4;

		public const int MAX_HD_SUPPORTED_API = 128;

		public const int MAX_LD_SUPPORTED_API = 32;

		public const int MAX_PM_SUPPORTED_API = 8;

		public const int HD_SERIAL_NO_LEN = 20;

		public const int HD_MODEL_LEN = 40;

		public const int HD_FWREV_LEN = 8;

		public const int WWN_LEN = 8;

		public const int LD_NAME_LEN = 16;

		public const int ADV_FEATURE_EVENT_WITH_SENSE_CODE = 0;

		public const int ADV_FEATURE_BIG_STRIPE_SUPPORT = 2;

		public const int ADV_FEATURE_BIOS_OPTION_SUPPORT = 4;

		public const int ADV_FEATURE_HAS_BBU = 8;

		public const int ADV_FEATURE_CONFIG_IN_FLASH = 16;

		public const int ADV_FEATURE_CPU_EFFICIENCY_SUPPORT = 32;

		public const int ADV_FEATURE_NO_MUTIL_VD_PER_PD = 64;

		public const int ADV_FEATURE_SPC_4_BUFFER = 128;

		public const int ADV_FEATURE_SES_DIRECT = 256;

		public const int ADV_FEATURE_MODULE_CONSOLIDATE = 512;

		public const int ADV_FEATURE_IMAGE_HEALTH = 1024;

		public const int ADV_FEATURE_SATA_PHY_CTRL_BY_PORT = 2048;

		public const int ADV_FEATURE_CRYPTO_SUPPORT = 4096;

		public const int ADV_FEATURE_OS_TIME_SUPPORT = 8192;

		public const int ADV_FEATURE_NO_VD_WRITE_CACHE_SUPPORT = 16384;

		public const int ADV_FEATURE_NO_VD_READ_CACHE_SUPPORT = 32768;

		public const int ADV_FEATURE_NO_HD_SMART_SUPPORT = 65536;

		public const int ADV_FEATURE_NO_VD_ROUNDING_SUPPORT = 131072;

		public const int ADV_FEATURE_NO_HD_SETFREE_SUPPORT = 262144;

		public const int ADV_FEATURE_NO_HD_WRITE_CACHE_SUPPORT = 524288;

		public const int ADV_FEATURE_NO_ENC_SUPPORT = 1048576;

		public const int ADV_FEATURE_NO_BGA_RATE_CHANGE = 2097152;

		public const int ADV_FEATURE_AES_PARTITION = 4194304;

		public const int ADV_FEATURE_HYPERDUO_INTELLIGENT_INIT = 8388608;

		public const int ADV_FEATURE_NO_SPARE_SUPPORT = 16777216;

		public const int ADV_FEATURE_HYBRID_SWITCH_SUPPORT = 33554432;

		public const int ADV_FEATURE_HOT_PLUG_SUPPORT = 67108864;

		public const int ADV_FEATURE_ATA_PASS_THROUGH = 134217728;

		public const int ADV_FEATURE_HYPERDUO_REMAP_SUPPORT = 268435456;

		public const int ADV_FEATURE_ACCESS_REGISTER = 536870912;

		public const int LD_MODE_FREE = 102;

		public const int LD_MODE_JBOD = 15;

		public const int LD_MODE_RAID_CROSS = 10;

		public const int LD_MODE_RAID_HDD_MIRROR = 11;

		public const int LD_MODE_RAID_HDD_MIRROR_HYBRID = 12;

		public const int LD_MODE_RAID0 = 0;

		public const int LD_MODE_RAID1 = 1;

		public const int LD_MODE_RAID10 = 16;

		public const int LD_MODE_RAID1E = 17;

		public const int LD_MODE_RAID5 = 5;

		public const int LD_MODE_RAID50 = 80;

		public const int LD_MODE_RAID6 = 6;

		public const int LD_MODE_RAID60 = 96;

		public const int LD_MODE_UNKNOWN = 255;

		public const int LD_STATUS_CONFLICTED = 11;

		public const int LD_STATUS_DEGRADE = 1;

		public const int LD_STATUS_DEGRADE_PLUGIN = 12;

		public const int LD_STATUS_DELETED = 2;

		public const int LD_STATUS_FOREIGN = 6;

		public const int LD_STATUS_FUNCTIONAL = 0;

		public const int LD_STATUS_HYPER_UNINIT = 13;

		public const int LD_STATUS_HYPER_USING_TABLE = 14;

		public const int LD_STATUS_IMPORTABLE = 7;

		public const int LD_STATUS_INVALID = 255;

		public const int LD_STATUS_MIGRATION = 9;

		public const int LD_STATUS_MISSING = 3;

		public const int LD_STATUS_NOT_IMPORTABLE = 8;

		public const int LD_STATUS_OFFLINE = 4;

		public const int LD_STATUS_PARTIALLYOPTIMAL = 5;

		public const int LD_STATUS_REBUILDING = 10;

		public const int LD_BGA_NONE = 0;

		public const int LD_BGA_REBUILD = 1;

		public const int LD_BGA_CONSISTENCY_FIX = 2;

		public const int LD_BGA_CONSISTENCY_CHECK = 4;

		public const int LD_BGA_INIT_QUICK = 8;

		public const int LD_BGA_INIT_BACK = 16;

		public const int LD_BGA_MIGRATION = 32;

		public const int LD_BGA_INIT_FORE = 64;

		public const int LD_BGA_COPYBACK = 128;

		public const int LD_BGA_DEFECT_FIXING = 256;

		public const int LD_BGA_MEDIA_PATROL = 512;

		public const int LD_BGA_MIGRATION_EXT = 1024;

		public const int LD_BGA_STATE_ABORTED = 2;

		public const int LD_BGA_STATE_AUTOPAUSED = 4;

		public const int LD_BGA_STATE_INTERRUPTED = 5;

		public const int LD_BGA_STATE_NONE = 0;

		public const int LD_BGA_STATE_PAUSED = 3;

		public const int LD_BGA_STATE_RUNNING = 1;

		public const byte TARGET_TYPE_LD = 0;

		public const byte TARGET_TYPE_FREE_PD = 1;

		public const byte DEVICE_TYPE_CD_DVD = 8;

		public const byte DEVICE_TYPE_ENCLOSURE = 11;

		public const byte DEVICE_TYPE_EXPANDER = 3;

		public const byte DEVICE_TYPE_HD = 1;

		public const byte DEVICE_TYPE_I2C_ENCLOSURE = 12;

		public const byte DEVICE_TYPE_MEDIA_CHANGER = 10;

		public const byte DEVICE_TYPE_NONE = 0;

		public const byte DEVICE_TYPE_OPTICAL_MEMORY = 9;

		public const byte DEVICE_TYPE_PM = 2;

		public const byte DEVICE_TYPE_PORT = 255;

		public const byte DEVICE_TYPE_PRINTER = 5;

		public const byte DEVICE_TYPE_PROCESSOR = 6;

		public const byte DEVICE_TYPE_TAPE = 4;

		public const byte DEVICE_TYPE_WRITE_ONCE = 7;

		public const int MAX_ATTRIBUTE = 30;

		public const byte CONSISTENCYCHECK_FIX = 1;

		public const byte CONSISTENCYCHECK_ONLY = 0;

		public const byte INIT_QUICK = 0;

		public const byte INIT_FULLFOREGROUND = 1;

		public const byte INIT_FULLBACKGROUND = 2;

		public const byte INIT_NONE = 3;

		public const byte BLOCK_INVALID = 0;

		public const byte BLOCK_VALID = 1;

		public const byte BLOCK_ASSIGNED = 2;

		public const byte BLOCK_FLAG_REBUILDING = 4;

		public const byte BLOCK_FLAG_TEMP_ASSIGN = 8;

		public const byte ROUNDING_SCHEME_NONE = 0;

		public const byte ROUNDING_SCHEME_1GB = 1;

		public const byte ROUNDING_SCHEME_10GB = 2;

		public const byte CACHE_WRITEBACK_ENABLE = 0;

		public const byte CACHE_WRITETHRU_ENABLE = 1;

		public const byte CACHE_ADAPTIVE_ENABLE = 2;

		public static bool DebugOn;

		[StructLayout(LayoutKind.Explicit)]
		public struct MV_U64
		{
			public ulong ToUlong()
			{
				return this.value;
			}

				[FieldOffset(0)]
			public MvApi.MV_U64.partsType parts;

				[FieldOffset(0)]
			public ulong value;

				public struct partsType
			{
						public int low;

						public int high;
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct RequestHeader
		{
			public void Init()
			{
				this.version = 0;
				this.requestType = 1;
				this.startingIndexOrId = 0;
				this.numRequested = 0;
				this.numReturned = 0;
				this.nextStartingIndex = 0;
			}

				public byte version;

				public byte requestType;

				public short startingIndexOrId;

				public short numRequested;

				public short numReturned;

				public short nextStartingIndex;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] reserved1;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Info_Request
		{
				public MvApi.RequestHeader header;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public byte[] data;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Link_Endpoint
		{
				public short DevID;

				public byte DevType;

				public byte PhyCnt;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] PhyID;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] SAS_Address;

				public short EnclosureID;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] Reserved;
		}

		public struct Link_Entity
		{
				public MvApi.Link_Endpoint Parent;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] Reserved;

				public MvApi.Link_Endpoint Self;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Version_Info
		{
				public int VerMajor;

				public int VerMinor;

				public int VerOEM;

				public int VerBuild;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Version_Info_CIM
		{
				public short MajorVersion;

				public short MinorVersion;

				public short RevisionNumber;

				public short BuildNumber;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Adapter_Info
		{
				public MvApi.Version_Info DriverVersion;

				public MvApi.Version_Info_CIM BIOSVersion;

				public MvApi.Version_Info_CIM FirmwareVersion;

				public MvApi.Version_Info_CIM BootLoaderVersion;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public MvApi.MV_U64[] Reserved1;

				public int SystemIOBusNumber;

				public int SlotNumber;

				public int InterruptLevel;

				public int InterruptVector;

				public short VenID;

				public short SubVenID;

				public short DevID;

				public short SubDevID;

				public byte PortCount;

				public byte PortSupportType;

				public byte Features;

				public byte AlarmSupport;

				public byte RevisionID;

				public byte MaxPDPerVD;

				public short StripeSizeSupported;

				public int AdvancedFeatures;

				public byte MaxPDPerDG;

				public byte MaxVDPerDG;

				public byte MaxParityDisks;

				public byte MaxDiskGroup;

				public byte MaxTotalBlocks;

				public byte MaxBlockPerPD;

				public byte MaxHD;

				public byte MaxExpander;

				public byte MaxPM;

				public byte MaxLogicalDrive;

				public short LogicalDriverMode;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] WWN;

				public short MaxHD_Ext;

				public short MaxBufferSize;

				public short MaxTotalBlocks_V2;

				public byte MaxAESPort;

				public byte MaxHyperHdd;

				public byte MaxSSDPerHyperDuo;

				public byte MaxAESEntry;

				public byte MaxSSDSegment;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
			public byte[] Reserved3;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] SerialNo;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			public byte[] PortSasAddress;

				public byte MaxSpeed;

				public byte CurrentSpeed;

				public byte MaxLinkWidth;

				public byte CurrentLinkWidth;

				public byte img_health;

				public byte autoload_img_health;

				public byte boot_loader_img_health;

				public byte firmware_img_health;

				public byte boot_rom_img_health;

				public byte hba_info_img_health;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public byte[] Reserved4;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] ModelNumber;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Adapter_Config_V2
		{
				public byte AlarmOn;

				public byte AutoRebuildOn;

				public byte BGARate;

				public byte PollSMARTStatus;

				public byte MediaPatrolRate;

				public byte CopyBack;

				public byte SyncRate;

				public byte InitRate;

				public byte RebuildRate;

				public byte MigrationRate;

				public byte CopybackRate;

				public byte InterruptCoalescing;

				public byte ModuleConsolidate;

				public byte v_atapi_disable;

				public byte sgpio_enable;

				public byte reserved0;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] port_phy_ctrl;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] SerialNo;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] ModelNumber;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct HD_Info
		{
				public MvApi.Link_Entity Link;

				public byte AdapterID;

				public byte InitStatus;

				public byte HDType;

				public byte PIOMode;

				public byte MDMAMode;

				public byte UDMAMode;

				public byte ConnectionType;

				public byte DeviceType;

				public int FeatureSupport;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
			public byte[] Model;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] SerialNo;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] FWVersion;

				public MvApi.MV_U64 Size;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] WWN;

				public byte CurrentPIOMode;

				public byte CurrentMDMAMode;

				public byte CurrentUDMAMode;

				public byte ElementIdx;

				public uint BlockSize;

				public byte ActivityLEDStatus;

				public byte LocateLEDStatus;

				public byte ErrorLEDStatus;

				public byte SesDeviceType;

				public int sata_signature;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] Reserved4;

				public byte HD_SSD_Type;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
			public byte[] Reserved1;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct HD_Info_Request
		{
				public MvApi.RequestHeader header;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public MvApi.HD_Info[] hdInfo;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct HD_Config
		{
				public byte WriteCacheOn;

				public byte SMARTOn;

				public byte Online;

				public byte DriveSpeed;

				public byte crypto;

				public byte AESPercentage;

				public short HDID;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct HD_Config_Request
		{
				public MvApi.RequestHeader header;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public MvApi.HD_Config[] hdConfig;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LD_Info
		{
				public short ID;

				public byte Status;

				public byte BGAStatus;

				public short StripeBlockSize;

				public byte RaidMode;

				public byte HDCount;

				public byte CacheMode;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] LD_GUID;

				public byte SectorCoefficient;

				public byte AdapterID;

				public byte BlkCount;

				public short time_hi;

				public short DGID;

				public MvApi.MV_U64 Size;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] Name;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			public short[] BlockIDs;

				public byte SubLDCount;

				public byte NumParityDisk;

				public short time_low;

				public int BlockSize;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LD_Info_Request
		{
				public MvApi.RequestHeader header;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public MvApi.LD_Info[] ldInfo;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LD_Config
		{
				public byte CacheMode;

				public byte HypperWaterMark;

				public byte AutoRebuildOn;

				public byte Status;

				public short LDID;

				public byte HyperRebuildMode;

				public byte Reserved2;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] Name;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LD_Config_Request
		{
				public MvApi.RequestHeader header;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public MvApi.LD_Config[] ldConfig;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LD_Status
		{
				public byte Status;

				public byte Bga;

				public short BgaPercentage;

				public byte BgaState;

				public byte BgaExt;

				public short LDID;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LD_Status_Request
		{
				public MvApi.RequestHeader header;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public MvApi.LD_Status[] ldStatus;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Block_Info
		{
				public short ID;

				public short HDID;

				public short Flags;

				public short LDID;

				public byte Status;

				public byte Reserved;

				public short BlockSize;

				public int ReservedSpaceForMigration;

				public MvApi.MV_U64 StartLBA;

				public MvApi.MV_U64 Size;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Block_Info_Request
		{
				public MvApi.RequestHeader header;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public MvApi.Block_Info[] blockInfos;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct ATA_REGS
		{
			public void Init()
			{
				this.drive_regs = new byte[7];
				this.reserved = new byte[6];
			}

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
			public byte[] drive_regs;

				public byte bReserved;

				public short buffer_size;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] reserved;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public byte[] data;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SMART_INFO
		{
				public byte Id;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] AttributeName;

				public byte StatusFlags1;

				public byte StatusFlags2;

				public byte CurrentValue;

				public byte WorstValue;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] RawValue;

				public byte ThresholdValue;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public byte[] Reserved;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct SMART_Info_Request
		{
				public MvApi.RequestHeader header;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
			public MvApi.SMART_INFO[] SmartInfo;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct PM_Info
		{
				public MvApi.Link_Entity Link;

				public byte AdapterID;

				public byte ProductRevision;

				public byte PMSpecRevision;

				public byte NumberOfPorts;

				public short VendorId;

				public short DeviceId;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] Reserved1;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct PM_Info_Request
		{
				public MvApi.RequestHeader header;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public MvApi.PM_Info[] pmInfo;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Create_LD_Param
		{
				public byte RaidMode;

				public byte HDCount;

				public byte RoundingScheme;

				public byte SubLDCount;

				public short StripeBlockSize;

				public byte NumParityDisk;

				public byte CachePolicy;

				public byte InitializationOption;

				public byte SectorCoefficient;

				public short LDID;

				public byte SpecifiedBlkSeq;

				public byte HypperWaterMark;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public byte[] Reserved2;

				public byte ReservedForApp;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			public short[] HDIDs;

				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] Name;

				public MvApi.MV_U64 Size;
		}
	}
}
