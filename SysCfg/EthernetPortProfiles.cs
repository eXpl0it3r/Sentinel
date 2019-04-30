using System;
using System.Collections.Generic;
using System.Xml;

namespace WDSystemConfig
{
	public class EthernetPortProfiles : Dictionary<int, EthernetPort>
	{
		public static EthernetPortProfiles GetInstance()
		{
			if (EthernetPortProfiles._instance == null)
			{
				lock (EthernetPortProfiles.lockObj)
				{
					if (EthernetPortProfiles._instance == null)
					{
						EthernetPortProfiles._instance = new EthernetPortProfiles();
					}
				}
			}
			return EthernetPortProfiles._instance;
		}

		public static EthernetPortProfiles GetEthernetProfiles()
		{
			return EthernetPortProfiles.GetInstance();
		}

		private EthernetPortProfiles()
		{
			XmlNodeList nodeList = SystemConfig.GetNodeList("EthernetPorts", "Port");
			this.Load(nodeList);
		}

		public EthernetPort GetPort(int number)
		{
			EthernetPort result;
			try
			{
				result = base[number];
			}
			catch (Exception)
			{
				result = null;
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
					EthernetPort ethernetPort = new EthernetPort();
					ethernetPort.Number = int.Parse(xmlNode.Attributes["Number"].Value);
					ethernetPort.Bus = int.Parse(xmlNode.Attributes["Bus"].Value);
					ethernetPort.Device = int.Parse(xmlNode.Attributes["Device"].Value);
					try
					{
						base.Add(ethernetPort.Number, ethernetPort);
					}
					catch (Exception)
					{
						this.SetError(string.Format("Duplicate Ethernet Port Number={0} Bus={1} Device={2}", ethernetPort.Number, ethernetPort.Bus, ethernetPort.Device));
					}
				}
			}
		}

		public void Load(XmlNode node, XmlNamespaceManager nsmgr)
		{
			XmlNodeList nodes = node.SelectNodes("cfg:Port", nsmgr);
			this.Load(nodes);
		}

		public string Dump()
		{
			string str = "EthernetPorts" + Environment.NewLine;
			foreach (KeyValuePair<int, EthernetPort> keyValuePair in this)
			{
				int key = keyValuePair.Key;
				EthernetPort value = keyValuePair.Value;
				str += string.Format("  Number={0}, Bus={1}, Device={2}", value.Number, value.Bus, value.Device);
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

		private static EthernetPortProfiles _instance = null;

		protected static object lockObj = new object();

		protected List<string> Errors;
	}
}
