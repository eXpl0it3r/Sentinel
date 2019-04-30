using System;
using System.Collections.Generic;
using System.Management;

namespace SpacesApi
{
	public class PhysicalDisk : StorageObject
	{
		public override void FromManagementObject(ManagementObject m)
		{
			base.FromManagementObject(m);
			this.DeviceId = SpacesApiUtil.GetManagementObjectValue<string>(m, "DeviceId");
			this.FriendlyName = SpacesApiUtil.GetManagementObjectValue<string>(m, "FriendlyName");
			this.Usage = (StorageUsage)Enum.ToObject(typeof(StorageUsage), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "Usage"));
			this.Description = SpacesApiUtil.GetManagementObjectValue<string>(m, "Description");
			this.Manufacturer = SpacesApiUtil.GetManagementObjectValue<string>(m, "Manufacturer");
			this.Model = SpacesApiUtil.GetManagementObjectValue<string>(m, "Model");
			this.SerialNumber = SpacesApiUtil.GetManagementObjectValue<string>(m, "SerialNumber").Trim();
			this.FirmwareVersion = SpacesApiUtil.GetManagementObjectValue<string>(m, "FirmwareVersion");
			this.OperationalStatus = SpacesApiUtil.ManagementObjectArrayToList<OperationalStatusEnum, ushort>(m["OperationalStatus"]);
			this.HealthStatus = (HealthStatusEnum)Enum.ToObject(typeof(HealthStatusEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "HealthStatus"));
			this.Size = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "Size");
			this.AllocatedSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "AllocatedSize");
			this.BusType = (BusTypeEnum)Enum.ToObject(typeof(BusTypeEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "BusType"));
			this.PhysicalSectorSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "PhysicalSectorSize");
			this.LogicalSectorSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "LogicalSectorSize");
			this.PhysicalLocation = SpacesApiUtil.GetManagementObjectValue<string>(m, "PhysicalLocation");
			this.EnclosureNumber = SpacesApiUtil.GetManagementObjectValue<ushort>(m, "EnclosureNumber");
			this.SlotNumber = SpacesApiUtil.GetManagementObjectValue<ushort>(m, "SlotNumber");
			this.CanPool = SpacesApiUtil.GetManagementObjectValue<bool>(m, "CanPool");
			this.CannotPoolReasons = SpacesApiUtil.ManagementObjectArrayToList<CannotPoolReasonEnum, ushort>(m["CannotPoolReason"]);
			this.IsPartial = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsPartial");
		}

		public string DeviceId;

		public string FriendlyName;

		public StorageUsage Usage;

		public string Description;

		public string Manufacturer;

		public string Model;

		public string SerialNumber;

		public string FirmwareVersion;

		public List<OperationalStatusEnum> OperationalStatus = new List<OperationalStatusEnum>();

		public HealthStatusEnum HealthStatus;

		public ulong Size;

		public ulong AllocatedSize;

		public BusTypeEnum BusType;

		public ulong LogicalSectorSize;

		public ulong PhysicalSectorSize;

		public string PhysicalLocation;

		public ushort EnclosureNumber;

		public ushort SlotNumber;

		public bool CanPool;

		public List<CannotPoolReasonEnum> CannotPoolReasons = new List<CannotPoolReasonEnum>();

		public bool IsPartial;
	}
}
