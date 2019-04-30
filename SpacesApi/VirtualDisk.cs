using System;
using System.Collections.Generic;
using System.Management;

namespace SpacesApi
{
	public class VirtualDisk : StorageObject
	{
		public override void FromManagementObject(ManagementObject m)
		{
			base.FromManagementObject(m);
			this.FriendlyName = SpacesApiUtil.GetManagementObjectValue<string>(m, "FriendlyName");
			this.Name = SpacesApiUtil.GetManagementObjectValue<string>(m, "Name");
			this.NameFormat = (NameFormatEnum)Enum.ToObject(typeof(NameFormatEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "NameFormat"));
			this.UniqueIdFormat = (UniqueIdFormatEnum)Enum.ToObject(typeof(UniqueIdFormatEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "UniqueIdFormat"));
			this.Usage = (VirtualDiskUsage)Enum.ToObject(typeof(VirtualDiskUsage), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "Usage"));
			this.OperationalStatus = SpacesApiUtil.ManagementObjectArrayToList<OperationalStatusEnum, ushort>(m["OperationalStatus"]);
			this.HealthStatus = (HealthStatusEnum)Enum.ToObject(typeof(HealthStatusEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "HealthStatus"));
			this.ResiliencySettingName = SpacesApiUtil.GetManagementObjectValue<string>(m, "ResiliencySettingName");
			this.Size = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "Size");
			this.AllocatedSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "AllocatedSize");
			this.PhysicalSectorSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "PhysicalSectorSize");
			this.LogicalSectorSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "LogicalSectorSize");
			this.FootprintOnPool = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "FootprintOnPool");
			this.ProvisioningType = (ProvisioningTypeEnum)Enum.ToObject(typeof(ProvisioningTypeEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "ProvisioningType"));
			this.NumberOfDataCopies = SpacesApiUtil.GetManagementObjectValue<ushort>(m, "NumberOfDataCopies");
			this.PhysicalDiskRedundancy = SpacesApiUtil.GetManagementObjectValue<ushort>(m, "PhysicalDiskRedundancy");
			this.ParityLayout = (ParityLayoutEnum)Enum.ToObject(typeof(ParityLayoutEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "ParityLayout"));
			this.NumberOfColumns = SpacesApiUtil.GetManagementObjectValue<ushort>(m, "NumberOfColumns");
			this.Interleave = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "Interleave");
			this.RequestNoSinglePointOfFailure = SpacesApiUtil.GetManagementObjectValue<bool>(m, "RequestNoSinglePointOfFailure");
			this.Access = (AccessEnum)Enum.ToObject(typeof(AccessEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "Access"));
			this.IsSnapshot = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsSnapshot");
			this.IsManualAttach = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsManualAttach");
			this.IsDeduplicationEnabled = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsDeduplicationEnabled");
			this.IsEnclosureAware = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsEnclosureAware");
			this.NumberOfAvailableCopies = SpacesApiUtil.GetManagementObjectValue<ushort>(m, "NumberOfAvailableCopies");
			this.DetachedReason = (DetachedReasonEnum)Enum.ToObject(typeof(DetachedReasonEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "DetachedReason"));
		}

		public string FriendlyName;

		public string Name;

		public NameFormatEnum NameFormat;

		public UniqueIdFormatEnum UniqueIdFormat;

		public VirtualDiskUsage Usage;

		public HealthStatusEnum HealthStatus;

		public List<OperationalStatusEnum> OperationalStatus;

		public string ResiliencySettingName;

		public ulong Size;

		public ulong AllocatedSize;

		public ulong LogicalSectorSize;

		public ulong PhysicalSectorSize;

		public ulong FootprintOnPool;

		public ProvisioningTypeEnum ProvisioningType;

		public ushort NumberOfDataCopies;

		public ushort PhysicalDiskRedundancy;

		public ParityLayoutEnum ParityLayout;

		public ushort NumberOfColumns;

		public ulong Interleave;

		public bool RequestNoSinglePointOfFailure;

		public AccessEnum Access;

		public bool IsSnapshot;

		public bool IsManualAttach;

		public bool IsDeduplicationEnabled;

		public bool IsEnclosureAware;

		public ushort NumberOfAvailableCopies;

		public DetachedReasonEnum DetachedReason;
	}
}
