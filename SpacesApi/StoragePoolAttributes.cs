using System.Collections.Generic;

namespace SpacesApi
{
	public class StoragePoolAttributes
	{
		public AttributeProperty<bool> IsReadOnly;

		public AttributeProperty<bool> ClearOnDeallocate;

		public AttributeProperty<bool> IsPowerProtected;

		public AttributeProperty<RetireMissingPhysicalDisksEnum> RetireMissingPhysicalDisks;

		public AttributeProperty<List<ProvisioningTypeEnum>> ThinProvisioningAlertThresholds;

		public string ExtendedStatus;
	}
}
