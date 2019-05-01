using System;

namespace Stor
{
	public class PersistentDriveInfo
	{
		public PersistentDriveInfo()
		{
			this.serial = "";
			this.creationTime = DateTime.Now;
			this.lastInserted = this.creationTime;
			this.unsupportedConsent = false;
		}

		public override string ToString()
		{
			string text = "serial=" + this.serial;
			text = text + ",created=" + this.creationTime.ToBinary();
			text = text + ",lastInserted=" + this.lastInserted.ToBinary();
			return text + ",unsupportedConsent=" + this.unsupportedConsent.ToString();
		}

		public bool IsRecent
		{
			get
			{
				DateTime now = DateTime.Now;
				TimeSpan t = now - this.creationTime;
				return PersistentDriveInfoList.PersistentDriveExpiration > t;
			}
		}

		public bool IsJustInserted
		{
			get
			{
				DateTime now = DateTime.Now;
				TimeSpan t = now - this.lastInserted;
				return PersistentDriveInfoList.PersistentDriveJustInsertedExpiration > t;
			}
		}

		public string serial;

		public DateTime creationTime;

		public DateTime lastInserted;

		public bool unsupportedConsent;
	}
}
