using System;

namespace Stor
{
	public class DriveBay : ICloneable, IEquatable<DriveBay>
	{
		public int Number
		{
			get
			{
				return this.number;
			}
			set
			{
				this.number = value;
			}
		}

		public int ControlPort
		{
			get
			{
				return this.controlport;
			}
			set
			{
				this.controlport = value;
			}
		}

		public int BootIndex
		{
			get
			{
				return this.bootIndex;
			}
			set
			{
				this.bootIndex = value;
			}
		}

		public DriveBayStatusEnum Status
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

		public bool Present
		{
			get
			{
				return this.present;
			}
			set
			{
				this.present = value;
			}
		}

		public bool Power
		{
			get
			{
				return this.power;
			}
			set
			{
				this.power = value;
			}
		}

		public bool IsHidden
		{
			get
			{
				return this.number == -1;
			}
		}

		public DrivePort DrivePort
		{
			get
			{
				return this.drivePort;
			}
			set
			{
				this.drivePort = value;
			}
		}

		public Drive OldDrive
		{
			get
			{
				return this.oldDrive;
			}
			set
			{
				this.oldDrive = value;
			}
		}

		public bool IsBoot
		{
			get
			{
				return this.isBoot;
			}
			set
			{
				this.isBoot = value;
			}
		}

		public Volume RaidVolume
		{
			get
			{
				return this.raidVolume;
			}
			set
			{
				this.raidVolume = value;
			}
		}

		public SpacesPool SpacesPool
		{
			get
			{
				return this.spacesPool;
			}
			set
			{
				this.spacesPool = value;
			}
		}

		public void SetPower(bool on)
		{
			DriveBayHW instance = DriveBayHW.GetInstance();
			if (instance != null && instance.SetBayPower(this, on))
			{
				this.power = on;
			}
		}

		public object Clone()
		{
			return new DriveBay
			{
				number = this.number,
				controlport = this.controlport,
				bootIndex = this.bootIndex,
				status = this.status,
				power = this.power,
				present = this.present,
				drivePort = (DrivePort)this.drivePort.Clone(),
				oldDrive = this.oldDrive,
				isBoot = this.isBoot,
				raidVolume = this.raidVolume,
				spacesPool = this.spacesPool
			};
		}

		public override string ToString()
		{
			string text = "";
			if (this.drivePort != null)
			{
				text = this.drivePort.ToString();
			}
			string text2 = "";
			if (this.oldDrive != null)
			{
				text2 = string.Format("Drive {0},{1},{2}", this.oldDrive.Id, this.oldDrive.Serial, this.oldDrive.Model);
			}
			return string.Format("Number={0}, Status={1}, Power={2}, Present={3}, DrivePort={4}, oldDrive={5}, BootIndex={6}, RaidVolume={7}, SpacesPool={8}", new object[]
			{
				this.number,
				this.status.ToString(),
				this.power,
				this.present,
				text,
				text2,
				this.bootIndex,
				(this.raidVolume != null) ? this.raidVolume.Name : "-",
				(this.spacesPool != null) ? this.spacesPool.Name : "-"
			});
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			DriveBay driveBay = obj as DriveBay;
			return driveBay != null && driveBay.drivePort.IsSame(this.drivePort);
		}

		public bool Equals(DriveBay d)
		{
			return d != null && d.drivePort.IsSame(this.drivePort);
		}

		public override int GetHashCode()
		{
			return this.drivePort.GetHashCode();
		}

		public static bool CompareStatus(DriveBay b1, DriveBay b2)
		{
			return b1.power == b2.power && b1.present == b2.present;
		}

		protected int number = -1;

		protected int controlport = -1;

		protected DriveBayStatusEnum status;

		protected bool present;

		protected bool power;

		protected DrivePort drivePort;

		protected bool isBoot;

		protected int bootIndex = -1;

		protected Drive oldDrive;

		protected Volume raidVolume;

		protected SpacesPool spacesPool;
	}
}
