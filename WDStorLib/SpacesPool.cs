using System;
using System.Collections.Generic;
using SpacesApi;

namespace Stor
{
	public class SpacesPool : IEquatable<SpacesPool>
	{
		public SpacesPool(string id, SpacesController c)
		{
			this.id = id;
			this.controller = c;
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

		public virtual SpacesController Controller
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

		public virtual bool IsPrimordial
		{
			get
			{
				return this.isprimordial;
			}
			set
			{
				this.isprimordial = value;
			}
		}

		public virtual HealthStatusEnum Health
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

		public virtual List<OperationalStatusEnum> OperationalStatuses
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

		public virtual ulong Size
		{
			get
			{
				return this.size;
			}
			set
			{
				this.size = value;
			}
		}

		public virtual ulong AllocatedSize
		{
			get
			{
				return this.allocatedSize;
			}
			set
			{
				this.allocatedSize = value;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			SpacesPool spacesPool = obj as SpacesPool;
			return spacesPool != null && string.Equals(spacesPool.id, this.id, StringComparison.OrdinalIgnoreCase);
		}

		public bool Equals(SpacesPool d)
		{
			return d != null && string.Equals(d.id, this.id, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			return this.id.GetHashCode();
		}

		public static bool CompareInfo(SpacesPool v1, SpacesPool v2)
		{
			bool flag = v1.name == v2.name && v1.size == v2.size && v1.allocatedSize == v2.allocatedSize;
			if (flag)
			{
				ListComparer<OperationalStatusEnum> listComparer = new ListComparer<OperationalStatusEnum>();
				if (v1.Health != v2.Health || listComparer.Compare(v1.operationalStatuses, v2.operationalStatuses, new Func<OperationalStatusEnum, OperationalStatusEnum, bool>(SpacesUtil.CompareOperationalStatus)))
				{
					flag = false;
				}
			}
			return flag;
		}

		protected string id;

		protected SpacesController controller;

		protected string name;

		protected bool isprimordial;

		protected HealthStatusEnum health = HealthStatusEnum.Unknown;

		protected List<OperationalStatusEnum> operationalStatuses = new List<OperationalStatusEnum>();

		protected ulong size;

		protected ulong allocatedSize;
	}
}
