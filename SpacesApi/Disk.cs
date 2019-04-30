using System;
using System.Management;

namespace SpacesApi
{
	public class Disk : StorageObject
	{
		public override void FromManagementObject(ManagementObject m)
		{
			base.FromManagementObject(m);
			this.FriendlyName = SpacesApiUtil.GetManagementObjectValue<string>(m, "FriendlyName");
			this.Path = SpacesApiUtil.GetManagementObjectValue<string>(m, "Path");
			this.Location = SpacesApiUtil.GetManagementObjectValue<string>(m, "Location");
			this.UniqueIdFormat = (UniqueIdFormatEnum)Enum.ToObject(typeof(UniqueIdFormatEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "UniqueIdFormat"));
			this.Number = SpacesApiUtil.GetManagementObjectValue<uint>(m, "Number");
			this.Manufacturer = SpacesApiUtil.GetManagementObjectValue<string>(m, "Manufacturer");
			this.Model = SpacesApiUtil.GetManagementObjectValue<string>(m, "Model");
			this.SerialNumber = SpacesApiUtil.GetManagementObjectValue<string>(m, "SerialNumber");
			this.FirmwareVersion = SpacesApiUtil.GetManagementObjectValue<string>(m, "FirmwareVersion");
			this.TotalSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "TotalSize");
			this.AllocatedSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "AllocatedSize");
			this.PhysicalSectorSize = SpacesApiUtil.GetManagementObjectValue<uint>(m, "PhysicalSectorSize");
			this.LogicalSectorSize = SpacesApiUtil.GetManagementObjectValue<uint>(m, "LogicalSectorSize");
			this.LargestFreeExtent = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "LargestFreeExtent");
			this.NumberOfPartitions = SpacesApiUtil.GetManagementObjectValue<uint>(m, "NumberOfPartitions");
			this.ProvisioningType = (ProvisioningTypeEnum)Enum.ToObject(typeof(ProvisioningTypeEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "ProvisioningType"));
			this.OperationalStatus = (DiskOperationalStatusEnum)Enum.ToObject(typeof(DiskOperationalStatusEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "OperationalStatus"));
			this.HealthStatus = (DiskHealthStatusEnum)Enum.ToObject(typeof(DiskHealthStatusEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "HealthStatus"));
			this.BusType = (BusTypeEnum)Enum.ToObject(typeof(BusTypeEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "BusType"));
			this.PartitionStyle = (PartitionStyleEnum)Enum.ToObject(typeof(PartitionStyleEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "PartitionStyle"));
			this.Signature = SpacesApiUtil.GetManagementObjectValue<uint>(m, "Signature");
			this.Guid = SpacesApiUtil.GetManagementObjectValue<string>(m, "Guid");
			this.OfflineReason = (OfflineReasonEnum)Enum.ToObject(typeof(OfflineReasonEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "OfflineReason"));
			this.IsReadOnly = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsReadOnly");
			this.IsSystem = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsSystem");
			this.IsClustered = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsClustered");
			this.IsBoot = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsBoot");
			this.BootFromDisk = SpacesApiUtil.GetManagementObjectValue<bool>(m, "BootFromDisk");
		}

		public string Path;

		public string Location;

		public string FriendlyName;

		public UniqueIdFormatEnum UniqueIdFormat;

		public uint Number;

		public string SerialNumber;

		public string FirmwareVersion;

		public string Manufacturer;

		public string Model;

		public ulong TotalSize;

		public ulong AllocatedSize;

		public uint LogicalSectorSize;

		public uint PhysicalSectorSize;

		public ulong LargestFreeExtent;

		public uint NumberOfPartitions;

		public ProvisioningTypeEnum ProvisioningType;

		public DiskOperationalStatusEnum OperationalStatus;

		public DiskHealthStatusEnum HealthStatus;

		public BusTypeEnum BusType;

		public PartitionStyleEnum PartitionStyle;

		public uint Signature;

		public string Guid;

		public bool IsOffline;

		public OfflineReasonEnum OfflineReason;

		public bool IsReadOnly;

		public bool IsSystem;

		public bool IsClustered;

		public bool IsBoot;

		public bool BootFromDisk;
	}
}
