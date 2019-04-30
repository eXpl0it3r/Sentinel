using System.Management;

namespace SpacesApi
{
	public class StorageObject
	{
		public virtual void FromManagementObject(ManagementObject m)
		{
			this.ObjectId = SpacesApiUtil.GetManagementObjectValue<string>(m, "ObjectId");
			this.UniqueId = SpacesApiUtil.GetManagementObjectValue<string>(m, "UniqueId");
			this.PassThroughClass = SpacesApiUtil.GetManagementObjectValue<string>(m, "PassThroughClass");
			this.PassThroughIds = SpacesApiUtil.GetManagementObjectValue<string>(m, "PassThroughIds");
			this.PassThroughNamespace = SpacesApiUtil.GetManagementObjectValue<string>(m, "PassThroughNamespace");
			this.PassThroughServer = SpacesApiUtil.GetManagementObjectValue<string>(m, "PassThroughServer");
		}

		public override string ToString()
		{
			string str = "";
			str += string.Format("ObjectId={0}", this.ObjectId);
			str += string.Format(",UniqueId={0},", this.UniqueId);
			str += string.Format(",PassThroughClass={0}", this.PassThroughClass);
			str += string.Format(",PassThroughIds={0}", this.PassThroughIds);
			str += string.Format(",PassThroughNamespace={0}", this.PassThroughNamespace);
			return str + string.Format(",PassThroughServer={0}", this.PassThroughServer);
		}

		public string ObjectId;

		public string UniqueId;

		public string PassThroughClass;

		public string PassThroughIds;

		public string PassThroughNamespace;

		public string PassThroughServer;
	}
}
