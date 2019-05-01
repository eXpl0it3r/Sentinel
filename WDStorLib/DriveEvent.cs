using System;

namespace Stor
{
	public class DriveEvent : EventArgs
	{
		public override string ToString()
		{
			return string.Format("{0}: controller={1}, drive={2} {3}, oldDrive={4} {5}", new object[]
			{
				this.type,
				this.controller.Id,
				(this.drive != null) ? this.drive.Id : "-",
				(this.drive != null) ? this.drive.Serial : "-",
				(this.oldDrive != null) ? this.oldDrive.Id : "-",
				(this.oldDrive != null) ? this.oldDrive.Serial : "-"
			});
		}

		public ControllerEventType type;

		public Controller controller;

		public Drive drive;

		public Drive oldDrive;
	}
}
