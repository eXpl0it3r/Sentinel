using System;
using System.Collections.Generic;
using System.Text;

namespace Stor
{
	public class MarvellController : Controller
	{
		public MarvellController(byte id) : base(MarvellController.MakeMarvellId(id))
		{
			this.mvid = id;
		}

		public byte AdapterId
		{
			get
			{
				return this.mvid;
			}
		}

		public override bool SupportsRaid()
		{
			return true;
		}

		public static string MakeMarvellId(byte id)
		{
			return string.Format("mv{0}", id);
		}

		public new static StorApiStatus GetControllers(ref List<Controller> controllers)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			byte b2 = 0;
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b2 = MvApi.MvApi.MV_Adapter_GetCount();
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_Adapter_GetCount exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			Storage.Debug("MV_Adapter_GetCount: {0}", new object[]
			{
				b2
			});
			for (byte b3 = 0; b3 < b2; b3 += 1)
			{
				byte b4 = 0;
				MvApi.MvApi.Adapter_Info adapter_Info = default(MvApi.MvApi.Adapter_Info);
				lock (MarvellUtil.mvApiLock)
				{
					try
					{
						b = MvApi.MvApi.MV_Adapter_GetInfo(b3, ref b4, ref adapter_Info);
					}
					catch (Exception ex2)
					{
						Logger.Warn("MV_Adapter_GetInfo exception: {0}", new object[]
						{
							ex2
						});
						return StorApiStatusEnum.STOR_API_ERROR;
					}
				}
				Storage.Debug("MV_Adapter_GetInfo: mvstatus={0}, count={1}", new object[]
				{
					b,
					b2
				});
				if (b != 0 || b4 != 1)
				{
					result = MarvellUtil.ToStorApiStatus(b);
					break;
				}
				if (controllers == null)
				{
					controllers = new List<Controller>();
				}
				MarvellController marvellController = new MarvellController(b3);
				marvellController.vendorId = (int)adapter_Info.VenID;
				marvellController.maxHDSupported = (int)adapter_Info.MaxHD;
				Storage.Debug("AdvancedFeatures: {0:X}", new object[]
				{
					adapter_Info.AdvancedFeatures
				});
				Storage.Debug("Supports ATA passthru: {0}", new object[]
				{
					((adapter_Info.AdvancedFeatures & 134217728) != 0) ? "yes" : "no"
				});
				controllers.Add(marvellController);
			}
			return result;
		}

		protected DrivePort GetDrivePort(MvApi.MvApi.HD_Info hdInfo)
		{
			DrivePort drivePort = new DrivePort();
			if (hdInfo.Link.Parent.DevType == 255)
			{
				drivePort.ports.Add((int)hdInfo.Link.Parent.PhyID[0]);
			}
			else if (hdInfo.Link.Parent.DevType == 2)
			{
				MvApi.MvApi.PM_Info_Request pm_Info_Request = default(MvApi.MvApi.PM_Info_Request);
				pm_Info_Request.header.Init();
				pm_Info_Request.header.requestType = 2;
				pm_Info_Request.header.startingIndexOrId = hdInfo.Link.Parent.DevID;
				pm_Info_Request.header.numRequested = 1;
				byte b;
				lock (MarvellUtil.mvApiLock)
				{
					try
					{
						b = MvApi.MvApi.MV_PD_GetPMInfo(this.AdapterId, ref pm_Info_Request);
					}
					catch (Exception ex)
					{
						Logger.Warn("MV_PD_GetPMInfo exception: {0}", new object[]
						{
							ex
						});
						return drivePort;
					}
				}
				if (b != 0)
				{
					Storage.Debug("Failed MV_PD_GetPMInfo: mvstatus={0}", new object[]
					{
						b
					});
					return drivePort;
				}
				if (pm_Info_Request.header.numReturned == 1)
				{
					MvApi.MvApi.PM_Info pm_Info = pm_Info_Request.pmInfo[0];
					drivePort.ports.Add((int)pm_Info.Link.Parent.PhyID[0]);
					drivePort.ports.Add((int)hdInfo.Link.Parent.PhyID[0]);
					return drivePort;
				}
				return drivePort;
			}
			return drivePort;
		}

		protected short GetDriveIdFromBlockId(short blkid)
		{
			byte b = 0;
			short result = -1;
			if (blkid != -1)
			{
				lock (MarvellUtil.mvApiLock)
				{
					MvApi.MvApi.Block_Info_Request block_Info_Request = default(MvApi.MvApi.Block_Info_Request);
					block_Info_Request.header.Init();
					block_Info_Request.header.requestType = 2;
					block_Info_Request.header.startingIndexOrId = blkid;
					block_Info_Request.header.numRequested = 1;
					try
					{
						b = MvApi.MvApi.MV_BLK_GetInfo(this.AdapterId, ref block_Info_Request);
					}
					catch (Exception ex)
					{
						Logger.Warn("MV_BLK_GetInfo exception: {0}", new object[]
						{
							ex
						});
						return result;
					}
					if (b == 0)
					{
						result = block_Info_Request.blockInfos[0].HDID;
					}
				}
				return result;
			}
			return result;
		}

		public override StorApiStatus Update()
		{
			StorApiStatus storApiStatus = this.UpdateVolumes();
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				return storApiStatus;
			}
			storApiStatus = this.UpdateDrives();
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				return storApiStatus;
			}
			this.UpdateVolumeDrives();
			return storApiStatus;
		}

		protected MarvellDrive MakeDrive(MvApi.MvApi.HD_Info hdInfo)
		{
			byte b = 0;
			StorApiStatus a = StorApiStatusEnum.STOR_NO_ERROR;
			MarvellDrive marvellDrive = new MarvellDrive(hdInfo.Link.Self.DevID, this);
			marvellDrive.Port = this.GetDrivePort(hdInfo);
			marvellDrive.Model = MarvellUtil.GetApiString(hdInfo.Model, 40);
			marvellDrive.Serial = MarvellUtil.GetApiString(hdInfo.SerialNo, 20);
			marvellDrive.Revision = MarvellUtil.GetApiString(hdInfo.FWVersion, 8);
			marvellDrive.SectorSize = (ulong)((hdInfo.BlockSize == 0u) ? 512u : hdInfo.BlockSize);
			marvellDrive.SectorCount = hdInfo.Size.ToUlong() * 1024UL / marvellDrive.SectorSize;
			marvellDrive.IsSmartEnabled = false;
			marvellDrive.IsSystem = false;
			marvellDrive.Status = DriveStatus.DRIVE_UNKNOWN;
			marvellDrive.Domain = DriveDomain.DRIVE_DOMAIN_UNKNOWN;
			short[] id = new short[]
			{
				hdInfo.Link.Self.DevID
			};
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_DiskHasOS(this.AdapterId, 1, 1, id);
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_DiskHasOS exception: {0}", new object[]
					{
						ex
					});
					throw ex;
				}
			}
			if (b == 159)
			{
				marvellDrive.IsSystem = true;
			}
			MvApi.MvApi.HD_Config_Request hd_Config_Request = default(MvApi.MvApi.HD_Config_Request);
			hd_Config_Request.header.Init();
			hd_Config_Request.header.requestType = 2;
			hd_Config_Request.header.startingIndexOrId = hdInfo.Link.Self.DevID;
			hd_Config_Request.header.numRequested = 1;
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_PD_GetConfig(this.AdapterId, ref hd_Config_Request);
				}
				catch (Exception ex2)
				{
					Logger.Warn("MV_PD_GetConfig exception: {0}", new object[]
					{
						ex2
					});
					throw ex2;
				}
			}
			marvellDrive.IsSmartEnabled = (hd_Config_Request.hdConfig[0].SMARTOn == 1);
			if (!marvellDrive.IsSmartEnabled)
			{
				a = marvellDrive.EnableSmart();
				if (a == StorApiStatusEnum.STOR_NO_ERROR)
				{
					marvellDrive.IsSmartEnabled = true;
				}
			}
			return marvellDrive;
		}

		public override StorApiStatus UpdateDrives()
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			if (this.drives != null)
			{
				this.drives.Clear();
			}
			for (byte b2 = 0; b2 < 128; b2 += 1)
			{
				MvApi.MvApi.HD_Info_Request hd_Info_Request = default(MvApi.MvApi.HD_Info_Request);
				hd_Info_Request.header.Init();
				hd_Info_Request.header.requestType = 2;
				hd_Info_Request.header.startingIndexOrId = (short)b2;
				hd_Info_Request.header.numRequested = 1;
				lock (MarvellUtil.mvApiLock)
				{
					try
					{
						b = MvApi.MvApi.MV_PD_GetHDInfo_Ext(this.AdapterId, ref hd_Info_Request);
					}
					catch (Exception ex)
					{
						Logger.Warn("MV_PD_GetHDInfo_Ext exception: {0}", new object[]
						{
							ex
						});
						return StorApiStatusEnum.STOR_API_ERROR;
					}
				}
				Storage.Debug("MV_PD_GetHDInfo_Ext: mvstatus={0}", new object[]
				{
					b
				});
				if (b == 0)
				{
					Storage.Debug("numReturned: {0}", new object[]
					{
						hd_Info_Request.header.numReturned
					});
					if (hd_Info_Request.header.numReturned == 1)
					{
						if (this.drives == null)
						{
							this.drives = new List<Drive>();
						}
						MvApi.MvApi.HD_Info hdInfo = hd_Info_Request.hdInfo[0];
						this.drives.Add(this.MakeDrive(hdInfo));
					}
				}
			}
			return result;
		}

		protected void UpdateVolumeDrives()
		{
			foreach (Volume volume in this.volumes)
			{
				MarvellVolume marvellVolume = (MarvellVolume)volume;
				foreach (short blkid in marvellVolume.BlockIds)
				{
					short driveIdFromBlockId = this.GetDriveIdFromBlockId(blkid);
					if (driveIdFromBlockId != -1)
					{
						Drive drive = this.FindDrive(driveIdFromBlockId.ToString());
						if (drive != null)
						{
							drive.Domain = DriveDomain.DRIVE_DOMAIN_RAID;
							marvellVolume.Drives.Add(drive);
						}
					}
				}
			}
			foreach (Drive drive2 in this.drives)
			{
				MarvellDrive marvellDrive = (MarvellDrive)drive2;
				if (marvellDrive.Domain == DriveDomain.DRIVE_DOMAIN_UNKNOWN)
				{
					StorApiStatus a = StorApiStatusEnum.STOR_NO_ERROR;
					SpacesController spacesController = SpacesController.GetSpacesController();
					SpacesDrive spacesDrive = null;
					a = spacesController.GetSpacesDrive(marvellDrive.Serial, ref spacesDrive);
					if (a == StorApiStatusEnum.STOR_NO_ERROR && spacesDrive != null)
					{
						bool flag = false;
						a = spacesController.IsDriveConfiguredForSpaces(spacesDrive, ref flag);
						if (a == StorApiStatusEnum.STOR_NO_ERROR && flag)
						{
							marvellDrive.Domain = DriveDomain.DRIVE_DOMAIN_SPACES;
						}
					}
				}
			}
		}

		protected MarvellVolume MakeVolume(MvApi.MvApi.LD_Info ldInfo)
		{
			byte b = 0;
			MarvellVolume marvellVolume = new MarvellVolume(ldInfo.ID, this);
			marvellVolume.Name = MarvellUtil.GetApiString(ldInfo.Name, 16);
			marvellVolume.RaidLevel = MarvellUtil.ToStorApiRaidLevel(ldInfo.RaidMode);
			marvellVolume.StripeSize = (ulong)((long)(ldInfo.StripeBlockSize * 1024));
			marvellVolume.Status = MarvellUtil.ToStorApiVolumeStatus(ldInfo.Status);
			marvellVolume.Capacity = ldInfo.Size.ToUlong() * 1024UL;
			for (byte b2 = 0; b2 < ldInfo.HDCount; b2 += 1)
			{
				marvellVolume.BlockIds.Add(ldInfo.BlockIDs[(int)b2]);
			}
			short[] id = new short[]
			{
				ldInfo.ID
			};
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_DiskHasOS(this.AdapterId, 0, 1, id);
				}
				catch (Exception ex)
				{
					Logger.Warn("MakeVolume MV_DiskHasOS exception: {0}", new object[]
					{
						ex
					});
					throw ex;
				}
			}
			if (b == 158)
			{
				marvellVolume.IsSystem = true;
			}
			if (ldInfo.BGAStatus != 0)
			{
				MvApi.MvApi.LD_Status_Request ld_Status_Request = default(MvApi.MvApi.LD_Status_Request);
				ld_Status_Request.header.Init();
				ld_Status_Request.header.requestType = 2;
				ld_Status_Request.header.startingIndexOrId = ldInfo.ID;
				ld_Status_Request.header.numRequested = 1;
				lock (MarvellUtil.mvApiLock)
				{
					try
					{
						b = MvApi.MvApi.MV_LD_GetStatus(this.AdapterId, ref ld_Status_Request);
					}
					catch (Exception ex2)
					{
						Logger.Warn("MV_LD_GetStatus exception: {0}", new object[]
						{
							ex2
						});
						throw ex2;
					}
				}
				if (b == 0)
				{
					marvellVolume.Progress = (float)ld_Status_Request.ldStatus[0].BgaPercentage;
					if (ld_Status_Request.ldStatus[0].Bga != 0 && marvellVolume.Status != VolumeStatus.VOLUME_FAILED)
					{
						VolumeStatus volumeStatus = MarvellUtil.ToStorApiVolumeStatusBGA(ld_Status_Request.ldStatus[0].Bga);
						if (volumeStatus != VolumeStatus.VOLUME_UNKNOWN)
						{
							marvellVolume.Status = volumeStatus;
						}
					}
					if (ld_Status_Request.ldStatus[0].Bga == 32)
					{
						MvApi.MvApi.LD_Info ld_Info = default(MvApi.MvApi.LD_Info);
						lock (MarvellUtil.mvApiLock)
						{
							try
							{
								b = MvApi.MvApi.MV_LD_GetTargetLDInfo(this.AdapterId, ldInfo.ID, ref ld_Info);
							}
							catch (Exception ex3)
							{
								Logger.Warn("MV_LD_GetTargetLDInfo exception: {0}", new object[]
								{
									ex3
								});
								throw ex3;
							}
						}
						if (b == 0)
						{
							marvellVolume.RaidLevel = MarvellUtil.ToStorApiRaidLevel(ld_Info.RaidMode);
							marvellVolume.StripeSize = (ulong)ld_Info.StripeBlockSize;
							marvellVolume.BlockIds.Clear();
							for (byte b3 = 0; b3 < ld_Info.HDCount; b3 += 1)
							{
								marvellVolume.BlockIds.Add(ld_Info.BlockIDs[(int)b3]);
							}
						}
					}
				}
			}
			return marvellVolume;
		}

		public override StorApiStatus UpdateVolumes()
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			if (this.volumes != null)
			{
				this.volumes.Clear();
			}
			for (byte b2 = 0; b2 < 32; b2 += 1)
			{
				MvApi.MvApi.LD_Info_Request ld_Info_Request = default(MvApi.MvApi.LD_Info_Request);
				ld_Info_Request.header.Init();
				ld_Info_Request.header.requestType = 2;
				ld_Info_Request.header.startingIndexOrId = (short)b2;
				ld_Info_Request.header.numRequested = 1;
				lock (MarvellUtil.mvApiLock)
				{
					try
					{
						b = MvApi.MvApi.MV_LD_GetInfo(this.AdapterId, ref ld_Info_Request);
					}
					catch (Exception ex)
					{
						Logger.Warn("MV_LD_GetInfo exception: {0}", new object[]
						{
							ex
						});
						return StorApiStatusEnum.STOR_API_ERROR;
					}
				}
				if (b == 0 && ld_Info_Request.header.numReturned == 1)
				{
					if (this.volumes == null)
					{
						this.volumes = new List<Volume>();
					}
					MvApi.MvApi.LD_Info ldInfo = ld_Info_Request.ldInfo[0];
					this.volumes.Add(this.MakeVolume(ldInfo));
				}
			}
			return result;
		}

		public override StorApiStatus DeleteVolume(string id)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte mvstatus = 0;
			Volume volume = this.FindVolume(id);
			if (volume == null || !(volume is MarvellVolume))
			{
				return StorApiStatusEnum.STOR_INVALID_PARAM;
			}
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					mvstatus = MvApi.MvApi.MV_LD_Delete(this.AdapterId, ((MarvellVolume)volume).MvId, 1);
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_LD_Delete exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			return MarvellUtil.ToStorApiStatus(mvstatus);
		}

		public override StorApiStatus CreateVolume(CreateVolumeData data)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte mvstatus = 0;
			MvApi.MvApi.Create_LD_Param create_LD_Param = default(MvApi.MvApi.Create_LD_Param);
			int num = 0;
			create_LD_Param.RaidMode = MarvellUtil.ToMarvellRaidLevel(data.level);
			create_LD_Param.RoundingScheme = 0;
			create_LD_Param.InitializationOption = 0;
			create_LD_Param.CachePolicy = (byte)(data.writeCache ? 0 : 1);
			create_LD_Param.StripeBlockSize = (short)data.stripeSize;
			create_LD_Param.HDIDs = new short[128];
			foreach (Drive drive in data.drives)
			{
				if (num >= 128)
				{
					return StorApiStatusEnum.STOR_INVALID_PARAM;
				}
				create_LD_Param.HDIDs[num] = ((MarvellDrive)drive).MvId;
				num++;
			}
			create_LD_Param.HDCount = (byte)num;
			create_LD_Param.Name = new byte[16];
			byte[] bytes = Encoding.ASCII.GetBytes(data.name);
			if (bytes.Length >= 16)
			{
				return StorApiStatusEnum.STOR_INVALID_PARAM;
			}
			bytes.CopyTo(create_LD_Param.Name, 0);
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					mvstatus = MvApi.MvApi.MV_LD_Create(this.AdapterId, ref create_LD_Param);
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_LD_Create exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			return MarvellUtil.ToStorApiStatus(mvstatus);
		}

		protected static MarvellControllerMonitor monitor;

		protected byte mvid;
	}
}
