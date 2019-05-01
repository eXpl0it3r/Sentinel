using System;
using System.Collections.Generic;
using SpacesApi;

namespace Stor
{
	public class SpacesDrive : Drive
	{
		public SpacesDrive(string id, SpacesController c) : base(id, c)
		{
		}

		public HealthStatusEnum Health
		{
			get
			{
				return this.health;
			}
			set
			{
				this.health = value;
			}
		}

		public List<OperationalStatusEnum> OperationalStatuses
		{
			get
			{
				return this.operationalStatuses;
			}
			set
			{
				this.operationalStatuses = value;
			}
		}

		public override int GetNumericId()
		{
			return this.serial.GetHashCode();
		}

		public bool IsConfiguredForSpaces()
		{
			StorApiStatus a = StorApiStatusEnum.STOR_NO_ERROR;
			bool result = false;
			if (this.controller != null)
			{
				a = ((SpacesController)this.controller).IsDriveConfiguredForSpaces(this, ref result);
				if (a != StorApiStatusEnum.STOR_NO_ERROR)
				{
					result = false;
				}
			}
			return result;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Drive drive = obj as Drive;
			return drive != null && string.Equals(drive.Id, this.id, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(Drive d)
		{
			return d != null && string.Equals(d.Id, this.id, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			return this.id.GetHashCode();
		}

		public new static bool CompareInfo(Drive v1, Drive v2)
		{
			bool flag = Drive.CompareInfo(v1, v2);
			if (flag)
			{
				SpacesDrive spacesDrive = (SpacesDrive)v1;
				SpacesDrive spacesDrive2 = (SpacesDrive)v2;
				ListComparer<OperationalStatusEnum> listComparer = new ListComparer<OperationalStatusEnum>();
				if (spacesDrive.Health != spacesDrive2.Health || listComparer.Compare(spacesDrive.operationalStatuses, spacesDrive2.operationalStatuses, new Func<OperationalStatusEnum, OperationalStatusEnum, bool>(SpacesUtil.CompareOperationalStatus)))
				{
					flag = false;
				}
			}
			return flag;
		}

		public static bool CompareAndPreserve(Drive d1, Drive d2)
		{
			if (string.IsNullOrEmpty(d2.Model))
			{
				d2.Model = d1.Model;
			}
			if (string.IsNullOrEmpty(d2.Serial))
			{
				d2.Serial = d1.Serial;
			}
			if (string.IsNullOrEmpty(d2.Revision))
			{
				d2.Revision = d1.Revision;
			}
			if (d2.SectorCount == 0UL)
			{
				d2.SectorCount = d1.SectorCount;
			}
			if (d2.SectorSize == 0UL)
			{
				d2.SectorSize = d1.SectorSize;
			}
			return SpacesDrive.CompareInfo(d1, d2);
		}

		protected HealthStatusEnum health = HealthStatusEnum.Unknown;

		protected List<OperationalStatusEnum> operationalStatuses = new List<OperationalStatusEnum>();
	}
}
