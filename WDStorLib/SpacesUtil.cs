using SpacesApi;

namespace Stor
{
	public class SpacesUtil
	{
		public static StorApiStatus Initialize()
		{
			SpacesApiError spacesApiError = SpacesApiError.Success;
			if (!SpacesUtil.apiInitialized)
			{
				spacesApiError = SpacesApi.SpacesApi.Initialize();
				if (spacesApiError == SpacesApiError.Success)
				{
					SpacesUtil.apiInitialized = true;
				}
			}
			return SpacesUtil.ToStorApiStatus(spacesApiError);
		}

		public static StorApiStatus Finalize()
		{
			return SpacesUtil.ToStorApiStatus(SpacesApi.SpacesApi.Finalize());
		}

		public static StorApiStatus ToStorApiStatus(SpacesApiError apierr)
		{
			StorApiStatus storApiStatus = new StorApiStatus(StorApiStatusEnum.STOR_NO_ERROR);
			storApiStatus.internalIntData = (int)apierr;
			switch (apierr)
			{
			case SpacesApiError.Success:
				storApiStatus.status = StorApiStatusEnum.STOR_NO_ERROR;
				return storApiStatus;
			case SpacesApiError.NotSupported:
				storApiStatus.status = StorApiStatusEnum.STOR_NOT_SUPPORTED;
				return storApiStatus;
			case SpacesApiError.Unknown:
				storApiStatus.status = StorApiStatusEnum.STOR_UNKNOWN_ERROR;
				return storApiStatus;
			case SpacesApiError.Timeout:
				storApiStatus.status = StorApiStatusEnum.STOR_TIME_OUT;
				return storApiStatus;
			case SpacesApiError.Failed:
				break;
			case SpacesApiError.InvalidParameter:
				storApiStatus.status = StorApiStatusEnum.STOR_INVALID_PARAM;
				return storApiStatus;
			default:
				if (apierr == SpacesApiError.NotEnoughResources)
				{
					storApiStatus.status = StorApiStatusEnum.STOR_OUT_OF_MEMORY;
					return storApiStatus;
				}
				break;
			}
			storApiStatus.status = StorApiStatusEnum.STOR_API_ERROR;
			return storApiStatus;
		}

		public static DriveStatus ToStorApiDriveStatus(PhysicalDisk pd)
		{
			DriveStatus driveStatus = DriveStatus.DRIVE_UNKNOWN;
			switch (pd.HealthStatus)
			{
			case HealthStatusEnum.Healthy:
				driveStatus = DriveStatus.DRIVE_GOOD;
				goto IL_27;
			case HealthStatusEnum.Unhealthy:
				driveStatus = DriveStatus.DRIVE_FAILED;
				goto IL_27;
			}
			driveStatus = DriveStatus.DRIVE_UNKNOWN;
			IL_27:
			if (driveStatus != DriveStatus.DRIVE_FAILED)
			{
				foreach (OperationalStatusEnum operationalStatusEnum in pd.OperationalStatus)
				{
					if (operationalStatusEnum == OperationalStatusEnum.PredictiveFailure)
					{
						driveStatus = DriveStatus.DRIVE_IMMINENT_FAILURE;
						break;
					}
					if (operationalStatusEnum == OperationalStatusEnum.Error || operationalStatusEnum == OperationalStatusEnum.NonRecoverableError || operationalStatusEnum == OperationalStatusEnum.LostCommunication || operationalStatusEnum == OperationalStatusEnum.NoContact)
					{
						driveStatus = DriveStatus.DRIVE_FAILED;
						break;
					}
				}
			}
			return driveStatus;
		}

		public static VolumeStatus ToStorApiVolumeStatus(VirtualDisk vd)
		{
			VolumeStatus volumeStatus = VolumeStatus.VOLUME_UNKNOWN;
			foreach (OperationalStatusEnum operationalStatusEnum in vd.OperationalStatus)
			{
				if (operationalStatusEnum == OperationalStatusEnum.OK)
				{
					volumeStatus = VolumeStatus.VOLUME_NORMAL;
				}
				else if (operationalStatusEnum == OperationalStatusEnum.Degraded)
				{
					volumeStatus = VolumeStatus.VOLUME_DEGRADED;
				}
				else if (operationalStatusEnum == OperationalStatusEnum.InService)
				{
					volumeStatus = VolumeStatus.VOLUME_REBUILDING;
				}
				else if (operationalStatusEnum == OperationalStatusEnum.Incomplete)
				{
					volumeStatus = VolumeStatus.VOLUME_FAILED;
				}
				if (volumeStatus != VolumeStatus.VOLUME_UNKNOWN)
				{
					break;
				}
			}
			if (volumeStatus == VolumeStatus.VOLUME_UNKNOWN)
			{
				switch (vd.HealthStatus)
				{
				case HealthStatusEnum.Healthy:
					volumeStatus = VolumeStatus.VOLUME_NORMAL;
					break;
				case HealthStatusEnum.Warning:
					volumeStatus = VolumeStatus.VOLUME_DEGRADED;
					break;
				case HealthStatusEnum.Unhealthy:
					volumeStatus = VolumeStatus.VOLUME_FAILED;
					break;
				default:
					volumeStatus = VolumeStatus.VOLUME_UNKNOWN;
					break;
				}
			}
			return volumeStatus;
		}

		public static RaidLevel DetermineVirtualDiskRaidLevel(VirtualDisk vd)
		{
			if (vd.ParityLayout != ParityLayoutEnum.None)
			{
				return RaidLevel.RAID_5;
			}
			if (vd.NumberOfColumns == 1)
			{
				return RaidLevel.RAID_NONE;
			}
			return RaidLevel.RAID_1;
		}

		public static bool CompareOperationalStatus(OperationalStatusEnum os1, OperationalStatusEnum os2)
		{
			return os1 == os2;
		}

		public static bool apiInitialized;
	}
}
