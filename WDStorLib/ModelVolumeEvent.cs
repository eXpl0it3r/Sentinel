using System;

namespace Stor
{
	public class ModelVolumeEvent : EventArgs
	{
		public override string ToString()
		{
			return string.Format("volume={0}, oldVolume={1}", (this.volume == null) ? "" : this.volume.Id, (this.oldVolume == null) ? "" : this.oldVolume.Id);
		}

		public Volume volume;

		public Volume oldVolume;

		public ControllerEventType type;
	}
}
