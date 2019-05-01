using System;

namespace Stor
{
	public class ModelDriveEvent : EventArgs
	{
		public override string ToString()
		{
			return string.Format("drive={0}, oldDrive={1}, bay={2}, oldBay={3}", new object[]
			{
				(this.drive == null) ? "" : this.drive.Id,
				(this.oldDrive == null) ? "" : this.oldDrive.Id,
				(this.bay == null) ? "" : this.bay.DrivePort.ToString(),
				(this.oldBay == null) ? "" : this.oldBay.DrivePort.ToString()
			});
		}

		public Drive drive;

		public Drive oldDrive;

		public DriveBay bay;

		public DriveBay oldBay;

		public ControllerEventType type;
	}
}
