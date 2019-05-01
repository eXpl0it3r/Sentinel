using System;

namespace Stor
{
	public class DriveBayChangedEvent : EventArgs
	{
		public override string ToString()
		{
			return string.Format("Bay:\n{0}\nOldBay:\n{1}", (this.bay == null) ? "" : this.bay.ToString(), (this.oldBay == null) ? "" : this.oldBay.ToString());
		}

		public DriveBay bay;

		public DriveBay oldBay;
	}
}
