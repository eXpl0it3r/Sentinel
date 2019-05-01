using System;
using System.Collections.Generic;

namespace Stor
{
	public class DrivePort : ICloneable
	{
		public DrivePort()
		{
			this.ports = new List<int>();
		}

		public int Root
		{
			get
			{
				if (this.ports == null || this.ports.Count <= 0)
				{
					return -1;
				}
				return this.ports[0];
			}
		}

		public override string ToString()
		{
			return string.Join<int>(".", this.ports);
		}

		public static DrivePort FromString(string s)
		{
			DrivePort drivePort = new DrivePort();
			string[] array = s.Split(new char[]
			{
				'.'
			});
			for (int i = 0; i < array.Length; i++)
			{
				drivePort.ports.Add(int.Parse(array[i]));
			}
			return drivePort;
		}

		public static implicit operator string(DrivePort d)
		{
			return d.ToString();
		}

		public static implicit operator int(DrivePort d)
		{
			return d.Root;
		}

		public bool IsSame(DrivePort d)
		{
			if (d.ports.Count != this.ports.Count)
			{
				return false;
			}
			for (int i = 0; i < this.ports.Count; i++)
			{
				if (this.ports[i] != d.ports[i])
				{
					return false;
				}
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			bool flag = base.Equals(obj);
			DrivePort d = obj as DrivePort;
			if (flag)
			{
				flag = this.IsSame(d);
			}
			return flag;
		}

		public override int GetHashCode()
		{
			return this.ports.GetHashCode();
		}

		public object Clone()
		{
			return new DrivePort
			{
				ports = new List<int>(this.ports)
			};
		}

		public List<int> ports;
	}
}
