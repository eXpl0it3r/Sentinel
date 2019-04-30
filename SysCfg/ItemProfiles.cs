using System;
using System.Collections.Generic;
using System.Xml;

namespace WDSystemConfig
{
	public class ItemProfiles : Dictionary<string, ItemProfile>
	{
		public static ItemProfiles GetInstance()
		{
			if (ItemProfiles.profiles == null)
			{
				lock (ItemProfiles.lockObj)
				{
					if (ItemProfiles.profiles == null)
					{
						ItemProfiles.profiles = new ItemProfiles();
					}
				}
			}
			return ItemProfiles.profiles;
		}

		public static ItemProfiles GetItemProfiles()
		{
			return ItemProfiles.GetInstance();
		}

		private ItemProfiles()
		{
			XmlNodeList nodeList = SystemConfig.GetNodeList("ItemProfiles", "ItemProfile");
			this.Load(nodeList);
			try
			{
				nodeList = SystemConfig.GetNodeList(SystemConfig.GetDriveListFilePath(), "ItemProfiles", "ItemProfile");
				this.Load(nodeList);
			}
			catch (Exception)
			{
			}
		}

		public string GetDefaultItemProfile()
		{
			return ItemProfiles.defaultItemProfile;
		}

		public Range GetRange(string itemName, string rangeName)
		{
			Range result = null;
			try
			{
				ItemProfile itemProfile = base[itemName];
				if (itemProfile != null)
				{
					result = itemProfile.RangeProfile[rangeName];
				}
			}
			catch (Exception)
			{
			}
			return result;
		}

		public FanThrottle GetFanThrottle(string itemName, string actionName)
		{
			FanThrottle result = null;
			try
			{
				ItemProfile itemProfile = base[itemName];
				if (itemProfile != null)
				{
					result = itemProfile.ThrottleProfile[actionName];
				}
			}
			catch (Exception)
			{
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
					ItemProfile itemProfile = new ItemProfile();
					itemProfile.Name = ItemProfiles.GetValue(xmlNode, "Name");
					string value = ItemProfiles.GetValue(xmlNode, "Default");
					if (string.IsNullOrEmpty(value))
					{
						itemProfile.DefaultProfile = false;
					}
					else
					{
						itemProfile.DefaultProfile = true;
						ItemProfiles.defaultItemProfile = itemProfile.Name;
					}
					itemProfile.RangeProfile = new Dictionary<string, Range>();
					XmlNode xmlNode2 = xmlNode["RangeProfile"];
					if (xmlNode2 != null)
					{
						XmlNodeList childNodes = xmlNode2.ChildNodes;
						foreach (object obj2 in childNodes)
						{
							XmlNode xmlNode3 = (XmlNode)obj2;
							if (xmlNode3.NodeType != XmlNodeType.Comment)
							{
								Range range = new Range();
								range.Name = xmlNode3.Attributes["Name"].Value;
								range.Min = int.Parse(xmlNode3.Attributes["Min"].Value);
								range.Max = int.Parse(xmlNode3.Attributes["Max"].Value);
								try
								{
									itemProfile.RangeProfile.Add(range.Name, range);
								}
								catch (Exception)
								{
									this.SetError(string.Format("{0}: duplicate Range Name={1} Value={2} Delta={3}", new object[]
									{
										itemProfile.Name,
										range.Name,
										range.Min,
										range.Max
									}));
								}
							}
						}
					}
					itemProfile.ThrottleProfile = new Dictionary<string, FanThrottle>();
					xmlNode2 = xmlNode["FanThrottleProfile"];
					if (xmlNode2 != null)
					{
						XmlNodeList childNodes2 = xmlNode2.ChildNodes;
						foreach (object obj3 in childNodes2)
						{
							XmlNode xmlNode4 = (XmlNode)obj3;
							if (xmlNode4.NodeType != XmlNodeType.Comment)
							{
								FanThrottle fanThrottle = new FanThrottle();
								fanThrottle.Name = ItemProfiles.GetValue(xmlNode4, "Name");
								fanThrottle.Value = ItemProfiles.GetValue(xmlNode4, "Value");
								fanThrottle.Delta = ItemProfiles.GetValue(xmlNode4, "Delta");
								try
								{
									itemProfile.ThrottleProfile.Add(fanThrottle.Name, fanThrottle);
								}
								catch (Exception)
								{
									this.SetError(string.Format("{0}: duplicate FanThrottle action Name={1} Value={2} Delta={3}", new object[]
									{
										itemProfile.Name,
										fanThrottle.Name,
										fanThrottle.Value,
										fanThrottle.Delta
									}));
								}
							}
						}
					}
					try
					{
						base.Add(itemProfile.Name, itemProfile);
					}
					catch (Exception)
					{
						this.SetError(string.Format("{0}: duplicate Item Profile Name={0}", itemProfile.Name));
					}
				}
			}
		}

		public void Load(XmlNode node, XmlNamespaceManager nsmgr)
		{
			XmlNodeList nodes = node.SelectNodes("cfg:Range", nsmgr);
			this.Load(nodes);
		}

		public string Dump()
		{
			string text = string.Empty;
			foreach (KeyValuePair<string, ItemProfile> keyValuePair in this)
			{
				string key = keyValuePair.Key;
				bool defaultProfile = keyValuePair.Value.DefaultProfile;
				text += string.Format("Item Profile: Name={0} Default={1}", key, defaultProfile.ToString());
				text += Environment.NewLine;
				Dictionary<string, Range> rangeProfile = keyValuePair.Value.RangeProfile;
				foreach (KeyValuePair<string, Range> keyValuePair2 in rangeProfile)
				{
					text += string.Format("  Range: Name={0} Min={1} Max={2}", keyValuePair2.Key, keyValuePair2.Value.Min, keyValuePair2.Value.Max);
					text += Environment.NewLine;
				}
				Dictionary<string, FanThrottle> throttleProfile = keyValuePair.Value.ThrottleProfile;
				foreach (KeyValuePair<string, FanThrottle> keyValuePair3 in throttleProfile)
				{
					text += string.Format("  FanThrottle: Name={0} Value={1} Delta={2}", keyValuePair3.Key, keyValuePair3.Value.Value, keyValuePair3.Value.Delta);
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

		private static ItemProfiles profiles = null;

		protected static object lockObj = new object();

		private static string defaultItemProfile = string.Empty;

		protected List<string> Errors;
	}
}
