using System;
using System.Collections.Generic;
using SpacesApi;

namespace Stor
{
	public class SpacesVolume : Volume
	{
		public SpacesVolume(string id, SpacesController c) : base(id, c)
		{
		}

		public override int GetNumericId()
		{
			return this.id.GetHashCode();
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

		public new static bool CompareInfo(Volume v1, Volume v2)
		{
			bool flag = Volume.CompareInfo(v1, v2);
			if (flag)
			{
				SpacesVolume spacesVolume = (SpacesVolume)v1;
				SpacesVolume spacesVolume2 = (SpacesVolume)v2;
				ListComparer<OperationalStatusEnum> listComparer = new ListComparer<OperationalStatusEnum>();
				if (spacesVolume.Health != spacesVolume2.Health || listComparer.Compare(spacesVolume.operationalStatuses, spacesVolume2.operationalStatuses, new Func<OperationalStatusEnum, OperationalStatusEnum, bool>(SpacesUtil.CompareOperationalStatus)))
				{
					flag = false;
				}
			}
			return flag;
		}

		protected HealthStatusEnum health = HealthStatusEnum.Unknown;

		protected List<OperationalStatusEnum> operationalStatuses = new List<OperationalStatusEnum>();
	}
}
