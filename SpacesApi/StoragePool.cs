using System;
using System.Collections.Generic;
using System.Management;

namespace SpacesApi
{
	public class StoragePool : StorageObject
	{
		public override void FromManagementObject(ManagementObject m)
		{
			base.FromManagementObject(m);
			this.FriendlyName = SpacesApiUtil.GetManagementObjectValue<string>(m, "FriendlyName");
			this.Name = SpacesApiUtil.GetManagementObjectValue<string>(m, "Name");
			this.OtherUsageDescription = SpacesApiUtil.GetManagementObjectValue<string>(m, "OtherUsageDescription");
			this.Usage = (PoolUsage)Enum.ToObject(typeof(PoolUsage), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "Usage"));
			this.IsPrimordial = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsPrimordial");
			this.HealthStatus = (HealthStatusEnum)Enum.ToObject(typeof(HealthStatusEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "HealthStatus"));
			this.OperationalStatus = SpacesApiUtil.ManagementObjectArrayToList<OperationalStatusEnum, ushort>(m["OperationalStatus"]);
			this.OtherOperationalStatusDescription = SpacesApiUtil.GetManagementObjectValue<string>(m, "OtherOperationalStatusDescription");
			this.Size = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "Size");
			this.AllocatedSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "AllocatedSize");
			this.PhysicalSectorSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "PhysicalSectorSize");
			this.LogicalSectorSize = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "LogicalSectorSize");
			this.ProvisioningTypeDefault = (ProvisioningTypeEnum)Enum.ToObject(typeof(ProvisioningTypeEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "ProvisioningTypeDefault"));
			this.SupportedProvisioningTypes = SpacesApiUtil.ManagementObjectArrayToList<ProvisioningTypeEnum, ushort>(m["SupportedProvisioningTypes"]);
			this.ResiliencySettingNameDefault = SpacesApiUtil.GetManagementObjectValue<string>(m, "ResiliencySettingNameDefault");
			this.IsReadOnly = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsReadOnly");
			this.ReadOnlyReason = (DetachedReasonEnum)Enum.ToObject(typeof(DetachedReasonEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "ReadOnlyReason"));
			this.SupportsDeduplication = SpacesApiUtil.GetManagementObjectValue<bool>(m, "SupportsDeduplication");
			this.IsClustered = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsClustered");
			this.ThinProvisioningAlertThresholds = SpacesApiUtil.ManagementObjectArrayToList<ushort, ushort>(m["ThinProvisioningAlertThresholds"]);
			this.ClearOnDeallocate = SpacesApiUtil.GetManagementObjectValue<bool>(m, "ClearOnDeallocate");
			this.IsPowerProtected = SpacesApiUtil.GetManagementObjectValue<bool>(m, "IsPowerProtected");
			this.EnclosureAwareDefault = SpacesApiUtil.GetManagementObjectValue<bool>(m, "EnclosureAwareDefault");
			this.RetireMissingPhysicalDisks = (RetireMissingPhysicalDisksEnum)Enum.ToObject(typeof(RetireMissingPhysicalDisksEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "RetireMissingPhysicalDisks"));
		}

		public string FriendlyName;

		public string Name;

		public PoolUsage Usage;

		public string OtherUsageDescription;

		public bool IsPrimordial;

		public HealthStatusEnum HealthStatus;

		public List<OperationalStatusEnum> OperationalStatus = new List<OperationalStatusEnum>();

		public string OtherOperationalStatusDescription;

		public ulong Size;

		public ulong AllocatedSize;

		public ulong LogicalSectorSize;

		public ulong PhysicalSectorSize;

		public ProvisioningTypeEnum ProvisioningTypeDefault;

		public List<ProvisioningTypeEnum> SupportedProvisioningTypes = new List<ProvisioningTypeEnum>();

		public string ResiliencySettingNameDefault;

		public bool IsReadOnly;

		public DetachedReasonEnum ReadOnlyReason;

		public bool IsClustered;

		public bool SupportsDeduplication;

		public List<ushort> ThinProvisioningAlertThresholds = new List<ushort>();

		public bool ClearOnDeallocate;

		public bool IsPowerProtected;

		public bool EnclosureAwareDefault;

		public RetireMissingPhysicalDisksEnum RetireMissingPhysicalDisks;
	}
}
