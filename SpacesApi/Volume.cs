using System;
using System.Management;

namespace SpacesApi
{
	public class Volume : StorageObject
	{
		public override void FromManagementObject(ManagementObject m)
		{
			base.FromManagementObject(m);
			this.DriveLetter = SpacesApiUtil.GetManagementObjectValue<char>(m, "DriveLetter");
			this.Path = SpacesApiUtil.GetManagementObjectValue<string>(m, "Path");
			this.HealthStatus = (VolumeHealthEnum)Enum.ToObject(typeof(VolumeHealthEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "HealthStatus"));
			this.FileSystem = SpacesApiUtil.GetManagementObjectValue<string>(m, "FileSystem");
			this.FileSystemLabel = SpacesApiUtil.GetManagementObjectValue<string>(m, "FileSystemLabel");
			this.Size = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "Size");
			this.SizeRemaining = SpacesApiUtil.GetManagementObjectValue<ulong>(m, "SizeRemaining");
			this.DriveType = (DriveTypeEnum)Enum.ToObject(typeof(DriveTypeEnum), SpacesApiUtil.GetManagementObjectValue<ushort>(m, "DriveType"));
		}

		public char DriveLetter;

		public string Path;

		public VolumeHealthEnum HealthStatus;

		public string FileSystem;

		public string FileSystemLabel;

		public ulong Size;

		public ulong SizeRemaining;

		public DriveTypeEnum DriveType;
	}
}
