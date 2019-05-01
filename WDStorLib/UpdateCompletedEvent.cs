using System;

namespace Stor
{
	public class UpdateCompletedEvent : EventArgs
	{
		public override string ToString()
		{
			return string.Format("controller={0}, status={1}", this.controller.Id, this.updateStatus);
		}

		public StorApiStatus updateStatus;

		public Controller controller;
	}
}
