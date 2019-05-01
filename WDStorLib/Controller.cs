using System.Collections.Generic;

namespace Stor
{
	public class Controller
	{
		public Controller(string id)
		{
			this.id = id;
			this.drives = new List<Drive>();
			this.volumes = new List<Volume>();
		}

		public string Id
		{
			get
			{
				return this.id;
			}
		}

		public int VendorId
		{
			get
			{
				return this.vendorId;
			}
			set
			{
				this.vendorId = value;
			}
		}

		public int MaxHDSupported
		{
			get
			{
				return this.maxHDSupported;
			}
		}

		public List<Drive> Drives
		{
			get
			{
				return this.drives;
			}
			set
			{
				this.drives = value;
				if (this.drives != null)
				{
					foreach (Drive drive in this.drives)
					{
						drive.Controller = this;
					}
				}
			}
		}

		public List<Volume> Volumes
		{
			get
			{
				return this.volumes;
			}
			set
			{
				this.volumes = value;
				if (this.volumes != null)
				{
					foreach (Volume volume in this.volumes)
					{
						volume.Controller = this;
					}
				}
			}
		}

		public static StorApiStatus GetControllers(ref List<Controller> controllers)
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public virtual bool SupportsRaid()
		{
			return false;
		}

		public virtual StorApiStatus Update()
		{
			StorApiStatus storApiStatus = this.UpdateDrives();
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				return storApiStatus;
			}
			return this.UpdateVolumes();
		}

		public virtual StorApiStatus UpdateDrives()
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public virtual StorApiStatus UpdateVolumes()
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public virtual StorApiStatus GetDriveVolumes(Drive drive, ref List<Volume> driveVolumes)
		{
			StorApiStatus result = StorApiStatusEnum.STOR_NO_ERROR;
			foreach (Volume volume in this.volumes)
			{
				foreach (Drive d in volume.Drives)
				{
					if (drive.Equals(d))
					{
						if (driveVolumes == null)
						{
							driveVolumes = new List<Volume>();
						}
						driveVolumes.Add(volume);
						break;
					}
				}
			}
			return result;
		}

		public virtual StorApiStatus GetFreeDrives(ref List<Drive> freeDrives)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			foreach (Drive drive in this.drives)
			{
				List<Volume> list = new List<Volume>();
				storApiStatus = this.GetDriveVolumes(drive, ref list);
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					return storApiStatus;
				}
				if (list.Count > 0)
				{
					if (freeDrives == null)
					{
						freeDrives = new List<Drive>();
					}
					freeDrives.Add(drive);
				}
			}
			return storApiStatus;
		}

		public virtual Drive FindDrive(string id)
		{
			foreach (Drive drive in this.drives)
			{
				if (drive.Id == id)
				{
					return drive;
				}
			}
			return null;
		}

		public virtual Volume FindVolume(string id)
		{
			foreach (Volume volume in this.volumes)
			{
				if (volume.Id == id)
				{
					return volume;
				}
			}
			return null;
		}

		public virtual StorApiStatus CreateVolume(CreateVolumeData data)
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public virtual StorApiStatus DeleteVolume(string id)
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		protected string id;

		protected int vendorId;

		protected int maxHDSupported;

		protected List<Drive> drives;

		protected List<Volume> volumes;
	}
}
