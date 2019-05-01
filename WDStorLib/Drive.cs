using System;

namespace Stor
{
	public class Drive : IEquatable<Drive>, ICloneable
	{
		public Drive(string id, Controller c)
		{
			this.id = id;
			this.controller = c;
			this.port = new DrivePort();
			this.failedMigrate = false;
			this.category = DriveCategory.DRIVE_CATEGORY_UNKNOWN;
		}

		public virtual object Clone()
		{
			return base.MemberwiseClone();
		}

		public string Id
		{
			get
			{
				return this.id;
			}
		}

		public virtual int GetNumericId()
		{
			int result = -1;
			try
			{
				result = int.Parse(this.id);
			}
			catch (Exception)
			{
				result = -1;
			}
			return result;
		}

		public Controller Controller
		{
			get
			{
				return this.controller;
			}
			set
			{
				this.controller = value;
			}
		}

		public string Serial
		{
			get
			{
				return this.serial;
			}
			set
			{
				this.serial = value;
			}
		}

		public string Model
		{
			get
			{
				return this.model;
			}
			set
			{
				this.model = value;
			}
		}

		public string Revision
		{
			get
			{
				return this.fwrev;
			}
			set
			{
				this.fwrev = value;
			}
		}

		public ulong SectorCount
		{
			get
			{
				return this.sectorCount;
			}
			set
			{
				this.sectorCount = value;
			}
		}

		public ulong SectorSize
		{
			get
			{
				return this.sectorSize;
			}
			set
			{
				this.sectorSize = value;
			}
		}

		public ulong Capacity
		{
			get
			{
				return this.sectorCount * this.sectorSize;
			}
		}

		public bool IsSmartEnabled
		{
			get
			{
				return this.smartEnabled;
			}
			set
			{
				this.smartEnabled = value;
			}
		}

		public bool IsSystem
		{
			get
			{
				return this.systemDisk;
			}
			set
			{
				this.systemDisk = value;
			}
		}

		public DrivePort Port
		{
			get
			{
				return this.port;
			}
			set
			{
				this.port = value;
			}
		}

		public DriveStatus Status
		{
			get
			{
				return this.status;
			}
			set
			{
				this.status = value;
			}
		}

		public int Temperature
		{
			get
			{
				return this.temperature;
			}
			set
			{
				this.temperature = value;
			}
		}

		public DriveDomain Domain
		{
			get
			{
				return this.domain;
			}
			set
			{
				this.domain = value;
			}
		}

		public bool FailedMigrate
		{
			get
			{
				return this.failedMigrate;
			}
			set
			{
				this.failedMigrate = value;
			}
		}

		public DriveCategory Category
		{
			get
			{
				return this.category;
			}
			set
			{
				this.category = value;
			}
		}

		public virtual StorApiStatus ATACmd(ref ATACmdInfo ata)
		{
			return StorApiStatusEnum.STOR_NOT_SUPPORTED;
		}

		public virtual StorApiStatus GetSmart(ref SmartInfo smart)
		{
			return StorApiStatusEnum.STOR_NOT_SUPPORTED;
		}

		public virtual StorApiStatus GetSmartPrediction(ref SmartPrediction smartPredict)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			ATACmdInfo atacmdInfo = new ATACmdInfo();
			atacmdInfo.datalen = 0u;
			atacmdInfo.registers[0] = 218;
			atacmdInfo.registers[3] = 79;
			atacmdInfo.registers[4] = 194;
			atacmdInfo.registers[6] = 176;
			atacmdInfo.data = null;
			storApiStatus = this.ATACmd(ref atacmdInfo);
			if (storApiStatus == StorApiStatusEnum.STOR_NO_ERROR)
			{
				if (smartPredict == null)
				{
					smartPredict = new SmartPrediction();
				}
				if (atacmdInfo.registers[3] == 244 && atacmdInfo.registers[4] == 44)
				{
					smartPredict.predictFailure = true;
				}
				else
				{
					smartPredict.predictFailure = false;
				}
				smartPredict.cylinderLow = atacmdInfo.registers[3];
				smartPredict.cylinderHigh = atacmdInfo.registers[4];
			}
			return storApiStatus;
		}

		public virtual StorApiStatus EnableSmart()
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			ATACmdInfo atacmdInfo = new ATACmdInfo();
			atacmdInfo.datalen = 0u;
			atacmdInfo.registers[0] = 216;
			atacmdInfo.registers[3] = 79;
			atacmdInfo.registers[4] = 194;
			atacmdInfo.registers[6] = 176;
			atacmdInfo.data = null;
			return this.ATACmd(ref atacmdInfo);
		}

		public virtual StorApiStatus GetIdentity(ref short[] identity)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			ATACmdInfo atacmdInfo = new ATACmdInfo();
			atacmdInfo.datalen = 512u;
			atacmdInfo.registers[6] = 236;
			atacmdInfo.data = new byte[512];
			storApiStatus = this.ATACmd(ref atacmdInfo);
			if (storApiStatus == StorApiStatusEnum.STOR_NO_ERROR)
			{
				identity = Ata.ByteArrayToShortArray(atacmdInfo.data);
			}
			return storApiStatus;
		}

		public virtual StorApiStatus InterpretIdentity(ref short[] identity)
		{
			if (identity == null)
			{
				return StorApiStatusEnum.STOR_INVALID_PARAM;
			}
			if (identity.Length < 256)
			{
				return StorApiStatusEnum.STOR_INVALID_PARAM;
			}
			this.IsSmartEnabled = ((identity[85] & 1) != 0);
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public virtual StorApiStatus IsWriteCacheOn(ref bool on)
		{
			return StorApiStatusEnum.STOR_NOT_SUPPORTED;
		}

		public virtual StorApiStatus SetWriteCache(bool on)
		{
			return StorApiStatusEnum.STOR_NOT_SUPPORTED;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Drive drive = obj as Drive;
			return drive != null && drive.serial == this.serial;
		}

		public virtual bool Equals(Drive d)
		{
			return d != null && d.serial == this.serial;
		}

		public override int GetHashCode()
		{
			return this.serial.GetHashCode();
		}

		public virtual int GetTemperatureFromSMARTData(SmartInfo smartInfo)
		{
			foreach (SmartAttribute smartAttribute in smartInfo.attributes)
			{
				if (smartAttribute.id == 194)
				{
					return (int)smartAttribute.raw[0];
				}
			}
			return -1;
		}

		public virtual StorApiStatus GetSSC(ref bool enabled)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			enabled = false;
			short[] array = new short[256];
			storApiStatus = this.GetIdentity(ref array);
			if (storApiStatus == StorApiStatusEnum.STOR_NO_ERROR)
			{
				enabled = ((array[137] & 2048) != 0);
			}
			return storApiStatus;
		}

		public virtual StorApiStatus SetSSC(bool enabled)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			ATACmdInfo atacmdInfo = new ATACmdInfo();
			atacmdInfo.datalen = 0u;
			atacmdInfo.registers[0] = (byte)(enabled ? 81 : 209);
			atacmdInfo.registers[3] = 199;
			atacmdInfo.registers[4] = 169;
			atacmdInfo.registers[6] = 143;
			atacmdInfo.data = null;
			return this.ATACmd(ref atacmdInfo);
		}

		public virtual StorApiStatus GetErrorRecoveryTime(bool isReadTimer, ref ushort value)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			ATACmdInfo atacmdInfo = new ATACmdInfo();
			atacmdInfo.datalen = 512u;
			atacmdInfo.registers[0] = 214;
			atacmdInfo.registers[1] = 1;
			atacmdInfo.registers[2] = 224;
			atacmdInfo.registers[3] = 79;
			atacmdInfo.registers[4] = 194;
			atacmdInfo.registers[6] = 176;
			COMMAND_ERROR_RECOVERY_CONTROL t = new COMMAND_ERROR_RECOVERY_CONTROL
			{
				Control = 3,
				Function = 2,
				Selection = (ushort)(isReadTimer ? 1 : 2),
				Value = 0
			};
			atacmdInfo.data = Ata.StructToByteArray<COMMAND_ERROR_RECOVERY_CONTROL>(t, 512);
			storApiStatus = this.ATACmd(ref atacmdInfo);
			if (storApiStatus == StorApiStatusEnum.STOR_NO_ERROR)
			{
				value = (ushort)((int)atacmdInfo.registers[2] << 8 | (int)atacmdInfo.registers[1]);
			}
			return storApiStatus;
		}

		public virtual StorApiStatus SetErrorRecoveryTime(bool isReadTimer, ushort value)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			ATACmdInfo atacmdInfo = new ATACmdInfo();
			atacmdInfo.datalen = 512u;
			atacmdInfo.registers[0] = 214;
			atacmdInfo.registers[1] = 1;
			atacmdInfo.registers[2] = 224;
			atacmdInfo.registers[3] = 79;
			atacmdInfo.registers[4] = 194;
			atacmdInfo.registers[6] = 176;
			COMMAND_ERROR_RECOVERY_CONTROL t = new COMMAND_ERROR_RECOVERY_CONTROL
			{
				Control = 3,
				Function = 1,
				Selection = (ushort)(isReadTimer ? 1 : 2),
				Value = value
			};
			atacmdInfo.data = Ata.StructToByteArray<COMMAND_ERROR_RECOVERY_CONTROL>(t, 512);
			return this.ATACmd(ref atacmdInfo);
		}

		public virtual StorApiStatus EnableVSC(bool enable)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			ATACmdInfo atacmdInfo = new ATACmdInfo();
			atacmdInfo.datalen = 0u;
			atacmdInfo.registers[0] = (byte)(enable ? 69 : 68);
			atacmdInfo.registers[1] = 0;
			atacmdInfo.registers[2] = 0;
			atacmdInfo.registers[3] = 68;
			atacmdInfo.registers[4] = 87;
			atacmdInfo.registers[6] = 128;
			atacmdInfo.data = null;
			return this.ATACmd(ref atacmdInfo);
		}

		public virtual StorApiStatus GetAC55(byte feat, byte parm1, ref uint value)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			storApiStatus = this.EnableVSC(true);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				return storApiStatus;
			}
			ATACmdInfo atacmdInfo = new ATACmdInfo();
			atacmdInfo.datalen = 512u;
			atacmdInfo.registers[0] = 214;
			atacmdInfo.registers[1] = 1;
			atacmdInfo.registers[2] = 190;
			atacmdInfo.registers[3] = 79;
			atacmdInfo.registers[4] = 194;
			atacmdInfo.registers[6] = 176;
			KEY_SECT t = new KEY_SECT
			{
				ActionCode = 55,
				FunctionCode = 11,
				Parm1 = 48879,
				Parm2 = 57005,
				Parm3 = (ushort)feat,
				Parm4 = (ushort)parm1
			};
			atacmdInfo.data = Ata.StructToByteArray<KEY_SECT>(t, 512);
			storApiStatus = this.ATACmd(ref atacmdInfo);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				this.EnableVSC(false);
				return storApiStatus;
			}
			atacmdInfo.datalen = 512u;
			atacmdInfo.registers[0] = 213;
			atacmdInfo.registers[1] = 1;
			atacmdInfo.registers[2] = 191;
			atacmdInfo.registers[3] = 79;
			atacmdInfo.registers[4] = 194;
			atacmdInfo.registers[6] = 176;
			atacmdInfo.data = new byte[512];
			storApiStatus = this.ATACmd(ref atacmdInfo);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				this.EnableVSC(false);
				return storApiStatus;
			}
			value = Ata.ByteArrayToStruct<FEAT_ID>(atacmdInfo.data).Data;
			this.EnableVSC(false);
			return storApiStatus;
		}

		public virtual StorApiStatus SetAC55(byte feat, byte parm1, byte type, byte size, uint value)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			storApiStatus = this.EnableVSC(true);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				return storApiStatus;
			}
			ATACmdInfo atacmdInfo = new ATACmdInfo();
			atacmdInfo.datalen = 512u;
			atacmdInfo.registers[0] = 214;
			atacmdInfo.registers[1] = 1;
			atacmdInfo.registers[2] = 190;
			atacmdInfo.registers[3] = 79;
			atacmdInfo.registers[4] = 194;
			atacmdInfo.registers[6] = 176;
			KEY_SECT t = new KEY_SECT
			{
				ActionCode = 55,
				FunctionCode = 2,
				Parm1 = 48879,
				Parm2 = 57005,
				Parm3 = (ushort)feat,
				Parm4 = (ushort)parm1,
				Parm5 = (ushort)type,
				Parm6 = (ushort)size,
				Parm7 = (ushort)value
			};
			atacmdInfo.data = Ata.StructToByteArray<KEY_SECT>(t, 512);
			storApiStatus = this.ATACmd(ref atacmdInfo);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				this.EnableVSC(false);
				return storApiStatus;
			}
			this.EnableVSC(false);
			return storApiStatus;
		}

		public static bool CompareId(Drive d1, Drive d2)
		{
			return d1.id == d2.id;
		}

		public static bool CompareInfo(Drive d1, Drive d2)
		{
			return d1.id == d2.id && d1.fwrev == d2.fwrev && d1.sectorCount == d2.sectorCount && d1.sectorSize == d2.sectorSize && d1.smartEnabled == d2.smartEnabled && d1.port.IsSame(d2.port) && d1.systemDisk == d2.systemDisk && d1.domain == d2.domain;
		}

		public static bool CompareAndCopyStatus(Drive d1, Drive d2)
		{
			bool result = Drive.CompareInfo(d1, d2);
			d2.status = d1.status;
			d2.temperature = d1.temperature;
			d2.failedMigrate = d1.failedMigrate;
			d2.category = d1.category;
			return result;
		}

		public static bool CompareStatus(Drive d1, Drive d2)
		{
			return d1.status == d2.status && d1.temperature == d2.temperature;
		}

		protected string id;

		protected Controller controller;

		protected string serial;

		protected string model;

		protected string fwrev;

		protected ulong sectorCount;

		protected ulong sectorSize;

		protected bool smartEnabled;

		protected bool systemDisk;

		protected DrivePort port;

		protected DriveStatus status;

		protected int temperature;

		protected DriveDomain domain;

		protected bool failedMigrate;

		protected DriveCategory category;
	}
}
