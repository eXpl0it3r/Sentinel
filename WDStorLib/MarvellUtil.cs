using System;
using System.Text;

namespace Stor
{
	public class MarvellUtil
	{
		public static StorApiStatus Initialize()
		{
			byte b = 0;
			lock (MarvellUtil.mvApiLock)
			{
				if (!MarvellUtil.apiInitialized)
				{
					try
					{
						b = MvApi.MvApi.MV_API_Initialize();
					}
					catch (Exception ex)
					{
						Logger.Warn("MV_API_Initialize exception: {0}", new object[]
						{
							ex
						});
						return StorApiStatusEnum.STOR_API_ERROR;
					}
					if (b == 0)
					{
						MarvellUtil.apiInitialized = true;
					}
				}
			}
			return MarvellUtil.ToStorApiStatus(b);
		}

		public static StorApiStatus Finalize()
		{
			StorApiStatus result;
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					result = MarvellUtil.ToStorApiStatus(MvApi.MvApi.MV_API_Finalize());
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_API_Finalize exception: {0}", new object[]
					{
						ex
					});
					result = StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			return result;
		}

		public static string GetApiString(byte[] buffer, int size)
		{
			char[] array = new char[3];
			array[0] = ' ';
			array[1] = '\t';
			char[] trimChars = array;
			string @string = Encoding.ASCII.GetString(buffer, 0, size);
			return @string.Trim(trimChars);
		}

		public static StorApiStatus ToStorApiStatus(byte mvstatus)
		{
			StorApiStatus storApiStatus = new StorApiStatus(StorApiStatusEnum.STOR_NO_ERROR);
			storApiStatus.internalIntData = (int)mvstatus;
			if (mvstatus <= 19)
			{
				switch (mvstatus)
				{
				case 0:
					storApiStatus.status = StorApiStatusEnum.STOR_NO_ERROR;
					return storApiStatus;
				case 1:
				case 2:
				case 4:
				case 5:
				case 10:
				case 11:
				case 12:
					goto IL_9A;
				case 3:
					storApiStatus.status = StorApiStatusEnum.STOR_UNKNOWN_ERROR;
					return storApiStatus;
				case 6:
				case 7:
				case 8:
				case 9:
				case 13:
				case 14:
					break;
				default:
					if (mvstatus != 19)
					{
						goto IL_9A;
					}
					storApiStatus.status = StorApiStatusEnum.STOR_NOT_SUPPORTED;
					return storApiStatus;
				}
			}
			else if (mvstatus != 57 && mvstatus != 153 && mvstatus != 243)
			{
				goto IL_9A;
			}
			storApiStatus.status = StorApiStatusEnum.STOR_INVALID_PARAM;
			return storApiStatus;
			IL_9A:
			storApiStatus.status = StorApiStatusEnum.STOR_API_ERROR;
			return storApiStatus;
		}

		public static RaidLevel ToStorApiRaidLevel(byte raidMode)
		{
			switch (raidMode)
			{
			case 0:
				return RaidLevel.RAID_0;
			case 1:
				return RaidLevel.RAID_1;
			case 2:
			case 3:
			case 4:
				break;
			case 5:
				return RaidLevel.RAID_5;
			case 6:
				return RaidLevel.RAID_6;
			default:
				if (raidMode == 16)
				{
					return RaidLevel.RAID_10;
				}
				break;
			}
			return RaidLevel.RAID_NONE;
		}

		public static byte ToMarvellRaidLevel(RaidLevel level)
		{
			switch (level)
			{
			case RaidLevel.RAID_0:
				return 0;
			case RaidLevel.RAID_1:
				return 1;
			case RaidLevel.RAID_5:
				return 5;
			case RaidLevel.RAID_6:
				return 6;
			case RaidLevel.RAID_10:
				return 16;
			default:
				return byte.MaxValue;
			}
		}

		public static VolumeStatus ToStorApiVolumeStatus(byte volumeStatus)
		{
			switch (volumeStatus)
			{
			case 0:
				return VolumeStatus.VOLUME_NORMAL;
			case 1:
				return VolumeStatus.VOLUME_DEGRADED;
			case 3:
			case 4:
				return VolumeStatus.VOLUME_FAILED;
			case 9:
				return VolumeStatus.VOLUME_MIGRATING;
			case 10:
				return VolumeStatus.VOLUME_REBUILDING;
			}
			return VolumeStatus.VOLUME_UNKNOWN;
		}

		public static VolumeStatus ToStorApiVolumeStatusBGA(byte bgaStatus)
		{
			if (bgaStatus <= 8)
			{
				switch (bgaStatus)
				{
				case 1:
					return VolumeStatus.VOLUME_REBUILDING;
				case 2:
				case 4:
					return VolumeStatus.VOLUME_VERIFYING;
				case 3:
					return VolumeStatus.VOLUME_UNKNOWN;
				default:
					if (bgaStatus != 8)
					{
						return VolumeStatus.VOLUME_UNKNOWN;
					}
					break;
				}
			}
			else if (bgaStatus != 16)
			{
				if (bgaStatus == 32)
				{
					return VolumeStatus.VOLUME_MIGRATING;
				}
				if (bgaStatus != 64)
				{
					return VolumeStatus.VOLUME_UNKNOWN;
				}
			}
			return VolumeStatus.VOLUME_INITIALIZING;
		}

		public static object mvApiLock = new object();

		public static bool apiInitialized = false;
	}
}
