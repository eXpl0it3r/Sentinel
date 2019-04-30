using System;
using System.Text.RegularExpressions;

namespace WDSystemConfig
{
	public class DrivelistEntry
	{
		public bool match(string str)
		{
			if (this.type == DrivelistEntryType.TYPE_CONSTANT)
			{
				return str == this.model;
			}
			bool result;
			try
			{
				if (this.regEx == null)
				{
					this.regEx = new Regex(this.model);
				}
				result = this.regEx.IsMatch(str);
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		public string Model
		{
			get
			{
				return this.model;
			}
			set
			{
				this.model = value;
			}
		}

		public string Description
		{
			get
			{
				return this.description;
			}
			set
			{
				this.description = value;
			}
		}

		public DrivelistEntryType Type
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
			}
		}

		public bool AnySize
		{
			get
			{
				return this.anySize;
			}
			set
			{
				this.anySize = value;
			}
		}

		public ulong Size
		{
			get
			{
				return this.size;
			}
			set
			{
				this.size = value;
			}
		}

		public string Profile
		{
			get
			{
				return this.profile;
			}
			set
			{
				this.profile = value;
			}
		}

		public DrivelistEntryCategory Category
		{
			get
			{
				return this.category;
			}
			set
			{
				this.category = value;
			}
		}

		private Regex regEx;

		private string description;

		private string model;

		private DrivelistEntryType type;

		private bool anySize;

		private ulong size;

		private string profile;

		private DrivelistEntryCategory category;
	}
}
