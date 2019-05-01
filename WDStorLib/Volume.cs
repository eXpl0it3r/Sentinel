using System;
using System.Collections.Generic;

namespace Stor
{
	public class Volume : IEquatable<Volume>
	{
		public Volume(string id, Controller c)
		{
			this.id = id;
			this.controller = c;
			this.drives = new List<Drive>();
		}

		public virtual string Id
		{
			get
			{
				return this.id;
			}
		}

		public virtual int GetNumericId()
		{
			int result = -1;
			try
			{
				result = int.Parse(this.id);
			}
			catch (Exception)
			{
				result = -1;
			}
			return result;
		}

		public virtual Controller Controller
		{
			get
			{
				return this.controller;
			}
			set
			{
				this.controller = value;
			}
		}

		public virtual string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		public virtual RaidLevel RaidLevel
		{
			get
			{
				return this.raidLevel;
			}
			set
			{
				this.raidLevel = value;
			}
		}

		public virtual ulong StripeSize
		{
			get
			{
				return this.stripeSize;
			}
			set
			{
				this.stripeSize = value;
			}
		}

		public virtual ulong Capacity
		{
			get
			{
				return this.capacity;
			}
			set
			{
				this.capacity = value;
			}
		}

		public virtual VolumeStatus Status
		{
			get
			{
				return this.status;
			}
			set
			{
				this.status = value;
			}
		}

		public virtual float Progress
		{
			get
			{
				return this.progress;
			}
			set
			{
				this.progress = value;
			}
		}

		public virtual bool IsSystem
		{
			get
			{
				return this.system;
			}
			set
			{
				this.system = value;
			}
		}

		public virtual List<Drive> Drives
		{
			get
			{
				return this.drives;
			}
			set
			{
				this.drives = value;
			}
		}

		public virtual ulong GetMinDiskSize()
		{
			ulong num = 0UL;
			if (this.drives != null)
			{
				num = ulong.MaxValue;
				foreach (Drive drive in this.drives)
				{
					if (drive.Capacity < num)
					{
						num = drive.Capacity;
					}
				}
			}
			return num;
		}

		public virtual StorApiStatus Verify(bool fixError)
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public virtual StorApiStatus Rebuild(List<Drive> newDrives)
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public virtual StorApiStatus Migrate(RaidLevel newLevel, List<Drive> newDrives)
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public virtual StorApiStatus Initialize(VolumeInitializeType type)
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Volume volume = obj as Volume;
			return volume != null && volume.id == this.id;
		}

		public bool Equals(Volume d)
		{
			return d != null && d.id == this.id;
		}

		public override int GetHashCode()
		{
			return this.name.GetHashCode();
		}

		public static bool CompareInfo(Volume v1, Volume v2)
		{
			bool flag = v1.name == v2.name && v1.raidLevel == v2.raidLevel && v1.stripeSize == v2.stripeSize && v1.status == v2.status && v1.progress == v2.progress && v1.capacity == v2.capacity && v1.system == v2.system;
			if (flag)
			{
				ListComparer<Drive> listComparer = new ListComparer<Drive>();
				flag = !listComparer.Compare(v1.drives, v2.drives, new Func<Drive, Drive, bool>(Drive.CompareId));
			}
			return flag;
		}

		protected string id;

		protected Controller controller;

		protected string name;

		protected RaidLevel raidLevel;

		protected ulong stripeSize;

		protected ulong capacity;

		protected VolumeStatus status;

		protected float progress;

		protected bool system;

		protected List<Drive> drives;
	}
}
