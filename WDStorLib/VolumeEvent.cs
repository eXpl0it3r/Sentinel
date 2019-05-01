using System;

namespace Stor
{
	public class VolumeEvent : EventArgs
	{
		public override string ToString()
		{
			return string.Format("{0}: controller={1}, volume={2} {3}, oldVolume={4} {5}", new object[]
			{
				this.type,
				this.controller.Id,
				(this.volume != null) ? this.volume.Id : "-",
				(this.volume != null) ? this.volume.Name : "-",
				(this.oldVolume != null) ? this.oldVolume.Id : "-",
				(this.oldVolume != null) ? this.oldVolume.Name : "-"
			});
		}

		public ControllerEventType type;

		public Controller controller;

		public Volume volume;

		public Volume oldVolume;
	}
}
