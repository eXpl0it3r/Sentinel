using System;
using System.Collections.Generic;
using System.Xml;

namespace WDSystemConfig
{
	public class SystemProfiles : Dictionary<string, SystemProfile>
	{
		public static SystemProfiles GetInstance()
		{
			if (SystemProfiles.profiles == null)
			{
				lock (SystemProfiles.lockObj)
				{
					if (SystemProfiles.profiles == null)
					{
						SystemProfiles.profiles = new SystemProfiles();
					}
				}
			}
			return SystemProfiles.profiles;
		}

		public static SystemProfiles GetItemProfiles()
		{
			return SystemProfiles.GetInstance();
		}

		private SystemProfiles()
		{
			XmlNodeList nodeList = SystemConfig.GetNodeList("SystemProfiles", "SystemProfile");
			this.Load(nodeList);
		}

		public Item GetItem(string systemProfileName, string itemName)
		{
			Item result = null;
			try
			{
				SystemProfile systemProfile = base[systemProfileName];
				if (systemProfile != null)
				{
					result = systemProfile.Items[itemName];
				}
			}
			catch (Exception)
			{
			}
			return result;
		}

		public int GetIntValue(XmlNode n, string attributeName)
		{
			int result = 0;
			string value = this.GetValue(n, attributeName);
			if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
			{
				try
				{
					result = int.Parse(value);
				}
				catch (Exception)
				{
				}
			}
			return result;
		}

		public string GetValue(XmlNode n, string attributeName)
		{
			string result = string.Empty;
			XmlAttribute xmlAttribute = n.Attributes[attributeName];
			if (xmlAttribute != null)
			{
				result = xmlAttribute.Value;
			}
			return result;
		}

		public ItemType GetTypeValue(XmlNode n, string attributeName)
		{
			ItemType result = ItemType.UNKNOWN;
			try
			{
				string value = this.GetValue(n, attributeName);
				if (!string.IsNullOrEmpty(value))
				{
					ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), value.ToUpper());
					if (Enum.IsDefined(typeof(ItemType), itemType))
					{
						result = itemType;
					}
				}
			}
			catch (ArgumentException)
			{
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
					SystemProfile systemProfile = new SystemProfile();
					systemProfile.Name = this.GetValue(xmlNode, "Name");
					systemProfile.Items = new Dictionary<string, Item>();
					XmlNode xmlNode2 = xmlNode["Items"];
					if (xmlNode2 != null)
					{
						XmlNodeList childNodes = xmlNode2.ChildNodes;
						foreach (object obj2 in childNodes)
						{
							XmlNode xmlNode3 = (XmlNode)obj2;
							if (xmlNode3.NodeType != XmlNodeType.Comment)
							{
								Item item = new Item();
								item.Name = this.GetValue(xmlNode3, "Name");
								item.Type = this.GetTypeValue(xmlNode3, "Type");
								item.ItemProfile = this.GetValue(xmlNode3, "ItemProfile");
								try
								{
									systemProfile.Items.Add(item.Name, item);
								}
								catch (Exception)
								{
									this.SetError(string.Format("{0}: duplicate Item Name={1} Type={2} ItemProfile={3}", new object[]
									{
										systemProfile.Name,
										item.Name,
										item.Type,
										item.ItemProfile
									}));
								}
							}
						}
					}
					systemProfile.MonitorProfiles = new List<MonitorProfile>();
					xmlNode2 = xmlNode["Monitors"];
					if (xmlNode2 != null)
					{
						XmlNodeList childNodes2 = xmlNode2.ChildNodes;
						foreach (object obj3 in childNodes2)
						{
							XmlNode xmlNode4 = (XmlNode)obj3;
							if (xmlNode4.NodeType != XmlNodeType.Comment)
							{
								MonitorProfile monitorProfile = new MonitorProfile();
								monitorProfile.Name = this.GetValue(xmlNode4, "Name");
								monitorProfile.ThermalProfile = this.GetValue(xmlNode4, "ThermalProfile");
								monitorProfile.Interval = this.GetIntValue(xmlNode4, "Interval");
								try
								{
									systemProfile.MonitorProfiles.Add(monitorProfile);
								}
								catch (Exception)
								{
									this.SetError(string.Format("{0}: duplicate Monitor Name={1} ThermalProfile={2} Interval{3}", new object[]
									{
										systemProfile.Name,
										monitorProfile.Name,
										monitorProfile.ThermalProfile,
										monitorProfile.Interval
									}));
								}
							}
						}
					}
					try
					{
						base.Add(systemProfile.Name, systemProfile);
					}
					catch (Exception)
					{
						this.SetError(string.Format("{0}: duplicate System Profile Name={0}", systemProfile.Name));
					}
				}
			}
		}

		public void Load(XmlNode node, XmlNamespaceManager nsmgr)
		{
			XmlNodeList nodes = node.SelectNodes("cfg:SystemProfile", nsmgr);
			this.Load(nodes);
		}

		public string Dump()
		{
			string text = string.Empty;
			foreach (KeyValuePair<string, SystemProfile> keyValuePair in this)
			{
				string key = keyValuePair.Key;
				text += string.Format("System Profile: Name={0}", key);
				text += Environment.NewLine;
				Dictionary<string, Item> items = keyValuePair.Value.Items;
				foreach (KeyValuePair<string, Item> keyValuePair2 in items)
				{
					text += string.Format("  Item: Name={0} Type={1} ItemProfile={2}", keyValuePair2.Key, keyValuePair2.Value.Type, keyValuePair2.Value.ItemProfile);
					text += Environment.NewLine;
				}
				List<MonitorProfile> monitorProfiles = keyValuePair.Value.MonitorProfiles;
				for (int i = 0; i < monitorProfiles.Count; i++)
				{
					MonitorProfile monitorProfile = monitorProfiles[i];
					text += string.Format("  Monitor: Name={0} ThermalProfile={1} Interval={2}", monitorProfile.Name, monitorProfile.ThermalProfile, monitorProfile.Interval);
					text += Environment.NewLine;
				}
			}
			text += Environment.NewLine;
			return text;
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

		private static SystemProfiles profiles = null;

		protected static object lockObj = new object();

		protected List<string> Errors;
	}
}
