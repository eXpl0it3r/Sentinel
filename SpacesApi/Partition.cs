using System;
using System.Collections.Generic;
using System.Management;

namespace SpacesApi
{
	public class Partition : StorageObject
	{
		public bool IsMbr
		{
			get
			{
				return this.MbrType != MbrTypeEnum.Unknown;
			}
		}

		public bool IsGpt
		{
			get
			{
				return !string.IsNullOrEmpty(this.Guid);
			}
		}

		public bool IsGptSys
		{
			get
			{
				return !string.IsNullOrEmpty(this.GptType) && this.GptType.Equals(Partition.GPT_SYSTEM_GUID, StringComparison.OrdinalIgnoreCase);
			}
		}

		public bool IsGptMsr
		{
			get
			{
				return !string.IsNullOrEmpty(this.GptType) && this.GptType.Equals(Partition.GPT_MSR_GUID, StringComparison.OrdinalIgnoreCase);
			}
		}

		public bool IsGptBasic
		{
			get
			{
				return !string.IsNullOrEmpty(this.GptType) && this.GptType.Equals(Partition.GPT_BASIC_DATA_GUID, StringComparison.OrdinalIgnoreCase);
			}
		}

		public bool IsGptRecovery
		{
			get
			{
				return !string.IsNullOrEmpty(this.GptType) && this.GptType.Equals(Partition.GPT_RECOVERY_GUID, StringComparison.OrdinalIgnoreCase);
			}
		}

		public override void FromManagementObject(ManagementObject m)
		{
			base.FromManagementObject(m);
			this.DiskNumber = SpacesApiUtil.GetManagementObjectValue<uint>(m, "DiskNumber");
			this.PartitionNumber = SpacesApiUtil.GetManagementObjectValue<uint>(m, "PartitionNumber");
			this.DriveLetter = SpacesApiUtil.GetManagementObjectValue<char>(m, "DriveLetter");
			this.AccessPaths = SpacesApiUtil.ManagementObjectArrayToList<string, string>(m["AccessPaths"]);
			this.OperationalStatus = (DiskOperationalStatusEnum)Enum.ToObject(typeof(DiskOperationalStatusEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "OperationalStatus"));
			this.TransitionState = (PartitionTransitionStateEnum)Enum.ToObject(typeof(PartitionTransitionStateEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "TransitionState"));
			this.Offset = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "Offset");
			this.Size = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "Size");
			this.MbrType = (MbrTypeEnum)Enum.ToObject(typeof(MbrTypeEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "MbrType"));
			this.GptType = SpacesApiUtil.GetManagementObjectValue<string>(m, "GptType");
			this.Guid = SpacesApiUtil.GetManagementObjectValue<string>(m, "Guid");
			this.IsReadOnly = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsReadOnly");
			this.IsOffline = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsOffline");
			this.IsSystem = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsSystem");
			this.IsBoot = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsBoot");
			this.IsActive = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsActive");
			this.IsHidden = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsHidden");
			this.IsShadowCopy = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsShadowCopy");
			this.NoDefaultDriveLetter = SpacesApiUtil.GetManagementObjectValue<bool>(m, "NoDefaultDriveLetter");
		}

		public static string GPT_SYSTEM_GUID = "c12a7328-f81f-11d2-ba4b-00a0c93ec93b";

		public static string GPT_MSR_GUID = "e3c9e316-0b5c-4db8-817d-f92df00215ae";

		public static string GPT_BASIC_DATA_GUID = "ebd0a0a2-b9e5-4433-87c0-68b6b72699c7";

		public static string GPT_LDM_METADATA_GUID = "5808c8aa-7e8f-42e0-85d2-e1e90434cfb3";

		public static string GPT_LDM_DATA_GUID = "af9b60a0-1431-4f62-bc68-3311714a69ad";

		public static string GPT_RECOVERY_GUID = "de94bba4-06d1-4d40-a16a-bfd50179d6ac";

		public string DiskId;

		public uint DiskNumber;

		public uint PartitionNumber;

		public char DriveLetter;

		public List<string> AccessPaths = new List<string>();

		public DiskOperationalStatusEnum OperationalStatus;

		public PartitionTransitionStateEnum TransitionState;

		public ulong Offset;

		public ulong Size;

		public MbrTypeEnum MbrType;

		public string GptType;

		public string Guid;

		public bool IsReadOnly;

		public bool IsOffline;

		public bool IsSystem;

		public bool IsBoot;

		public bool IsActive;

		public bool IsHidden;

		public bool IsShadowCopy;

		public bool NoDefaultDriveLetter;
	}
}
