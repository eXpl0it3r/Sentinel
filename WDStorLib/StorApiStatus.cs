using System.ComponentModel;
using System.Globalization;

namespace Stor
{
	[TypeConverter(typeof(StorApiStatus.StorApiStatusConverter))]
	public class StorApiStatus
	{
		public StorApiStatus(StorApiStatusEnum s)
		{
			this.status = s;
			this.internalIntData = 0;
			this.internalStringData = null;
		}

		public static implicit operator StorApiStatus(StorApiStatusEnum b)
		{
			return new StorApiStatus(b);
		}

		public static bool operator ==(StorApiStatus a, StorApiStatusEnum b)
		{
			return a.status == b;
		}

		public static bool operator ==(StorApiStatusEnum a, StorApiStatus b)
		{
			return a == b.status;
		}

		public static bool operator ==(StorApiStatus a, StorApiStatus b)
		{
			return a.status == b.status;
		}

		public static bool operator !=(StorApiStatus a, StorApiStatusEnum b)
		{
			return a.status != b;
		}

		public static bool operator !=(StorApiStatusEnum a, StorApiStatus b)
		{
			return a != b.status;
		}

		public static bool operator !=(StorApiStatus a, StorApiStatus b)
		{
			return a.status != b.status;
		}

		public override int GetHashCode()
		{
			return this.status.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(obj, null))
			{
				return false;
			}
			StorApiStatus storApiStatus = obj as StorApiStatus;
			return !object.ReferenceEquals(storApiStatus, null) && storApiStatus.status == this.status;
		}

		public override string ToString()
		{
			string text = this.status.ToString();
			if (this.internalIntData != 0)
			{
				text = text + ": int: " + this.internalIntData.ToString();
			}
			if (this.internalStringData != null)
			{
				text = text + ": str: " + this.internalStringData;
			}
			return text;
		}

		public StorApiStatusEnum status;

		public int internalIntData;

		public string internalStringData;

		public class StorApiStatusConverter : TypeConverter
		{
			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				if (value is StorApiStatusEnum)
				{
					return new StorApiStatus((StorApiStatusEnum)value);
				}
				return base.ConvertFrom(context, culture, value);
			}
		}
	}
}
