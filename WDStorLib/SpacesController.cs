using System;
using System.Collections.Generic;
using SpacesApi;

namespace Stor
{
	public class SpacesController : Controller
	{
		public SpacesController(string id) : base(id)
		{
		}

		public override bool SupportsRaid()
		{
			return true;
		}

		public List<SpacesPool> Pools
		{
			get
			{
				return this.pools;
			}
			set
			{
				this.pools = value;
				if (this.pools != null)
				{
					foreach (SpacesPool spacesPool in this.pools)
					{
						spacesPool.Controller = this;
					}
				}
			}
		}

		public static SpacesController GetSpacesController()
		{
			if (SpacesController.spacesController == null)
			{
				SpacesController.spacesController = new SpacesController("spaces0");
				SpacesController.spacesController.vendorId = 99;
				SpacesController.spacesController.maxHDSupported = 32;
			}
			return SpacesController.spacesController;
		}

		public new static StorApiStatus GetControllers(ref List<Controller> controllers)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (controllers == null)
			{
				controllers = new List<Controller>();
			}
			controllers.Add(SpacesController.GetSpacesController());
			return result;
		}

		protected DrivePort GetDrivePort(PhysicalDisk pd)
		{
			return new DrivePort
			{
				ports = 
				{
					(int)pd.EnclosureNumber,
					(int)pd.SlotNumber
				}
			};
		}

		public StorApiStatus GetSpacesDrive(string serial, ref SpacesDrive drive)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			PhysicalDisk physicalDisk = null;
			SpacesApiError physicalDisk2 = SpacesApi.SpacesApi.GetPhysicalDisk(serial, ref physicalDisk);
			if (physicalDisk2 != SpacesApiError.Success)
			{
				return SpacesUtil.ToStorApiStatus(physicalDisk2);
			}
			if (physicalDisk != null)
			{
				drive = this.MakeDrive(physicalDisk);
			}
			return result;
		}

		public override StorApiStatus Update()
		{
			StorApiStatus storApiStatus = base.Update();
			if (storApiStatus == StorApiStatusEnum.STOR_NO_ERROR)
			{
				storApiStatus = this.UpdatePools();
			}
			return storApiStatus;
		}

		public SpacesDrive MakeDrive(PhysicalDisk pd)
		{
			SpacesDrive spacesDrive = new SpacesDrive(pd.ObjectId, this);
			spacesDrive.Port = this.GetDrivePort(pd);
			spacesDrive.Model = pd.Model;
			spacesDrive.Serial = pd.SerialNumber;
			spacesDrive.Revision = pd.FirmwareVersion;
			spacesDrive.SectorSize = pd.LogicalSectorSize;
			spacesDrive.SectorCount = 0UL;
			if (pd.LogicalSectorSize != 0UL)
			{
				spacesDrive.SectorCount = pd.Size / pd.LogicalSectorSize;
			}
			spacesDrive.IsSmartEnabled = false;
			spacesDrive.IsSystem = SpacesApi.SpacesApi.IsPhysicalDiskSystem(pd);
			spacesDrive.Status = SpacesUtil.ToStorApiDriveStatus(pd);
			spacesDrive.Temperature = 0;
			spacesDrive.Health = pd.HealthStatus;
			spacesDrive.OperationalStatuses = pd.OperationalStatus;
			return spacesDrive;
		}

		public override StorApiStatus UpdateDrives()
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.drives != null)
			{
				this.drives.Clear();
			}
			List<PhysicalDisk> list = new List<PhysicalDisk>();
			SpacesApiError physicalDisks = SpacesApi.SpacesApi.GetPhysicalDisks(ref list);
			if (physicalDisks != SpacesApiError.Success)
			{
				Logger.Error("GetPhysicalDiskDisks returned error: {0}", new object[]
				{
					physicalDisks
				});
				return SpacesUtil.ToStorApiStatus(physicalDisks);
			}
			foreach (PhysicalDisk pd in list)
			{
				SpacesDrive item = this.MakeDrive(pd);
				this.drives.Add(item);
			}
			return result;
		}

		public StorApiStatus AddDrive(SpacesDrive drive)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.drives == null)
			{
				this.drives = new List<Drive>();
			}
			int i;
			for (i = 0; i < this.drives.Count; i++)
			{
				Drive drive2 = this.drives[i];
				if (drive2.Serial == drive.Serial)
				{
					this.drives[i] = drive;
					break;
				}
			}
			if (i == this.drives.Count)
			{
				this.drives.Add(drive);
			}
			return result;
		}

		public StorApiStatus RemoveDrive(string serial, ref Drive removedDrive)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.drives == null)
			{
				return result;
			}
			for (int i = 0; i < this.drives.Count; i++)
			{
				Drive drive = this.drives[i];
				if (drive.Serial == serial)
				{
					removedDrive = drive;
					this.drives.RemoveAt(i);
					break;
				}
			}
			return result;
		}

		public SpacesVolume MakeVolume(VirtualDisk d)
		{
			SpacesVolume spacesVolume = new SpacesVolume(d.ObjectId, this);
			spacesVolume.Name = d.FriendlyName;
			spacesVolume.StripeSize = d.Interleave;
			spacesVolume.RaidLevel = SpacesUtil.DetermineVirtualDiskRaidLevel(d);
			spacesVolume.Progress = 0f;
			spacesVolume.Status = SpacesUtil.ToStorApiVolumeStatus(d);
			spacesVolume.IsSystem = false;
			spacesVolume.Health = d.HealthStatus;
			spacesVolume.OperationalStatuses = d.OperationalStatus;
			List<Drive> drives = new List<Drive>();
			StorApiStatus volumeDrives = this.GetVolumeDrives(spacesVolume, ref drives);
			if (volumeDrives == StorApiStatusEnum.STOR_NO_ERROR)
			{
				spacesVolume.Drives = drives;
			}
			return spacesVolume;
		}

		public override StorApiStatus UpdateVolumes()
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.volumes != null)
			{
				this.volumes.Clear();
			}
			List<VirtualDisk> list = new List<VirtualDisk>();
			SpacesApiError virtualDisks = SpacesApi.SpacesApi.GetVirtualDisks(ref list);
			if (virtualDisks != SpacesApiError.Success)
			{
				Logger.Error("GetVirtualDisks returned error: {0}", new object[]
				{
					virtualDisks
				});
				return SpacesUtil.ToStorApiStatus(virtualDisks);
			}
			foreach (VirtualDisk d in list)
			{
				this.volumes.Add(this.MakeVolume(d));
			}
			return result;
		}

		public StorApiStatus AddVolume(SpacesVolume volume)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.volumes == null)
			{
				this.volumes = new List<Volume>();
			}
			int i;
			for (i = 0; i < this.volumes.Count; i++)
			{
				Volume volume2 = this.volumes[i];
				if (string.Equals(volume2.Id, volume.Id, StringComparison.OrdinalIgnoreCase))
				{
					this.volumes[i] = volume2;
					break;
				}
			}
			if (i == this.volumes.Count)
			{
				this.volumes.Add(volume);
			}
			return result;
		}

		public StorApiStatus RemoveVolume(string id, ref Volume removedVolume)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.volumes == null)
			{
				return result;
			}
			for (int i = 0; i < this.volumes.Count; i++)
			{
				Volume volume = this.volumes[i];
				if (string.Equals(volume.Id, id, StringComparison.OrdinalIgnoreCase))
				{
					removedVolume = volume;
					this.volumes.RemoveAt(i);
					break;
				}
			}
			return result;
		}

		public SpacesPool MakePool(StoragePool d)
		{
			return new SpacesPool(d.ObjectId, this)
			{
				Name = d.FriendlyName,
				IsPrimordial = d.IsPrimordial,
				Health = d.HealthStatus,
				OperationalStatuses = d.OperationalStatus,
				Size = d.Size,
				AllocatedSize = d.AllocatedSize
			};
		}

		public virtual StorApiStatus UpdatePools()
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.pools != null)
			{
				this.pools.Clear();
			}
			List<StoragePool> list = new List<StoragePool>();
			SpacesApiError storagePools = SpacesApi.SpacesApi.GetStoragePools(ref list);
			if (storagePools != SpacesApiError.Success)
			{
				return SpacesUtil.ToStorApiStatus(storagePools);
			}
			foreach (StoragePool d in list)
			{
				this.pools.Add(this.MakePool(d));
			}
			return result;
		}

		public StorApiStatus AddPool(SpacesPool pool)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.pools == null)
			{
				this.pools = new List<SpacesPool>();
			}
			int i;
			for (i = 0; i < this.pools.Count; i++)
			{
				SpacesPool spacesPool = this.pools[i];
				if (string.Equals(spacesPool.Id, pool.Id, StringComparison.OrdinalIgnoreCase))
				{
					this.pools[i] = spacesPool;
					break;
				}
			}
			if (i == this.pools.Count)
			{
				this.pools.Add(pool);
			}
			return result;
		}

		public StorApiStatus RemovePool(string id, ref SpacesPool removedPool)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.pools == null)
			{
				return result;
			}
			for (int i = 0; i < this.pools.Count; i++)
			{
				SpacesPool spacesPool = this.pools[i];
				if (string.Equals(spacesPool.Id, id, StringComparison.OrdinalIgnoreCase))
				{
					removedPool = spacesPool;
					this.pools.RemoveAt(i);
					break;
				}
			}
			return result;
		}

		public virtual StorApiStatus GetPool(string id, ref SpacesPool pool)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			if (this.pools == null)
			{
				return result;
			}
			for (int i = 0; i < this.pools.Count; i++)
			{
				SpacesPool spacesPool = this.pools[i];
				if (string.Equals(spacesPool.Id, id, StringComparison.OrdinalIgnoreCase))
				{
					pool = spacesPool;
					break;
				}
			}
			return result;
		}

		public virtual StorApiStatus GetVolumeDrives(Volume volume, ref List<Drive> voldrives)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			SpacesApiError spacesApiError = SpacesApiError.Success;
			if (volume == null)
			{
				return StorApiStatusEnum.STOR_INVALID_PARAM;
			}
			List<PhysicalDisk> list = new List<PhysicalDisk>();
			spacesApiError = SpacesApi.SpacesApi.GetPhysicalDisksForVirtualDisk(volume.Id, ref list);
			if (spacesApiError == SpacesApiError.Success)
			{
				foreach (PhysicalDisk pd in list)
				{
					SpacesDrive item = this.MakeDrive(pd);
					voldrives.Add(item);
				}
			}
			return SpacesUtil.ToStorApiStatus(spacesApiError);
		}

		public virtual StorApiStatus IsDriveConfiguredForSpaces(Drive drive, ref bool configured)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			if (drive == null)
			{
				return StorApiStatusEnum.STOR_INVALID_PARAM;
			}
			StoragePool storagePool = null;
			SpacesApiError physicalDiskPool = SpacesApi.SpacesApi.GetPhysicalDiskPool(drive.Id, ref storagePool);
			if (physicalDiskPool == SpacesApiError.Success)
			{
				if (storagePool == null)
				{
					configured = false;
				}
				else
				{
					configured = !storagePool.IsPrimordial;
				}
			}
			return SpacesUtil.ToStorApiStatus(physicalDiskPool);
		}

		public virtual StorApiStatus GetDrivePool(Drive drive, ref SpacesPool pool)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			SpacesApiError spacesApiError = SpacesApiError.Success;
			if (drive == null)
			{
				return StorApiStatusEnum.STOR_INVALID_PARAM;
			}
			StoragePool storagePool = null;
			spacesApiError = SpacesApi.SpacesApi.GetPhysicalDiskPool(drive.Id, ref storagePool);
			if (spacesApiError == SpacesApiError.Success && storagePool != null)
			{
				foreach (SpacesPool spacesPool in this.pools)
				{
					if (string.Equals(spacesPool.Id, storagePool.ObjectId, StringComparison.OrdinalIgnoreCase))
					{
						pool = spacesPool;
						break;
					}
				}
			}
			return SpacesUtil.ToStorApiStatus(spacesApiError);
		}

		protected List<SpacesPool> pools = new List<SpacesPool>();

		protected static SpacesController spacesController;
	}
}
