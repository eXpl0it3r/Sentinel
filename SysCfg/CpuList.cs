using System;
using System.Collections.Generic;
using System.Xml;

namespace WDSystemConfig
{
	public class CpuList : Dictionary<string, CpuItem>
	{
		public static CpuList GetInstance()
		{
			if (CpuList.profiles == null)
			{
				lock (CpuList.lockObj)
				{
					if (CpuList.profiles == null)
					{
						CpuList.profiles = new CpuList();
					}
				}
			}
			return CpuList.profiles;
		}

		public static CpuList GetCpuList()
		{
			return CpuList.GetInstance();
		}

		private CpuList()
		{
			XmlNodeList nodeList = SystemConfig.GetNodeList("CPUList", "CPU");
			this.Load(nodeList);
		}

		public CpuItem GetItem(string name)
		{
			CpuItem result;
			try
			{
				result = base[name];
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		public static string GetValue(XmlNode n, string attributeName)
		{
			string result = string.Empty;
			XmlAttribute xmlAttribute = n.Attributes[attributeName];
			if (xmlAttribute != null)
			{
				result = xmlAttribute.Value;
			}
			return result;
		}

		public void Load(XmlNodeList nodes)
		{
			foreach (object obj in nodes)
			{
				XmlNode xmlNode = (XmlNode)obj;
				if (xmlNode.NodeType != XmlNodeType.Comment)
				{
					CpuItem cpuItem = new CpuItem();
					cpuItem.Name = CpuList.GetValue(xmlNode, "Name");
					cpuItem.SystemProfile = CpuList.GetValue(xmlNode, "SystemProfile");
					cpuItem.ItemProfile = CpuList.GetValue(xmlNode, "ItemProfile");
					try
					{
						base.Add(cpuItem.Name, cpuItem);
					}
					catch (Exception)
					{
						this.SetError(string.Format("{0}: duplicate Name={0} SystemProfile={1} ItemProfile={2}", cpuItem.Name, cpuItem.SystemProfile, cpuItem.ItemProfile));
					}
				}
			}
		}

		public void Load(XmlNode node, XmlNamespaceManager nsmgr)
		{
			XmlNodeList nodes = node.SelectNodes("cfg:CPU", nsmgr);
			this.Load(nodes);
		}

		public string Dump()
		{
			string str = string.Empty;
			foreach (KeyValuePair<string, CpuItem> keyValuePair in this)
			{
				string key = keyValuePair.Key;
				CpuItem value = keyValuePair.Value;
				str += string.Format("CPU: Name={0} SystemProfile={1} ItemProfile={2}", value.Name, value.SystemProfile, value.ItemProfile);
				str += Environment.NewLine;
			}
			return str + Environment.NewLine;
		}

		public List<string> GetErrors()
		{
			return this.Errors;
		}

		public void SetError(string str)
		{
			if (this.Errors == null)
			{
				this.Errors = new List<string>();
			}
			this.Errors.Add(str);
		}

		private static CpuList profiles = null;

		protected static object lockObj = new object();

		protected List<string> Errors;
	}
}
