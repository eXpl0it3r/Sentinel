using System;
using System.Collections.Generic;
using System.Xml;

namespace WDSystemConfig
{
	public class Drivelist
	{
		public static Drivelist GetInstance()
		{
			if (Drivelist._instance == null)
			{
				lock (Drivelist.lockObj)
				{
					if (Drivelist._instance == null)
					{
						Drivelist._instance = new Drivelist();
					}
				}
			}
			return Drivelist._instance;
		}

		public static Drivelist GetDrivelist()
		{
			return Drivelist.GetInstance();
		}

		private Drivelist()
		{
			try
			{
				XmlNodeList nodeList = SystemConfig.GetNodeList(SystemConfig.GetDriveListFilePath(), null, "DrivelistVersion");
				this.LoadVersion(nodeList);
				nodeList = SystemConfig.GetNodeList(SystemConfig.GetDriveListFilePath(), "Drivelist", "Drive");
				this.Load(nodeList);
			}
			catch (Exception)
			{
			}
		}

		public List<DrivelistEntry> Entries
		{
			get
			{
				return this.entries;
			}
			set
			{
				this.entries = value;
			}
		}

		public string Version
		{
			get
			{
				return this.version;
			}
		}

		public List<DrivelistEntry> GetCategory(DrivelistEntryCategory category)
		{
			List<DrivelistEntry> list = new List<DrivelistEntry>();
			if (this.entries != null)
			{
				foreach (DrivelistEntry drivelistEntry in this.entries)
				{
					if (drivelistEntry.Category == category)
					{
						list.Add(drivelistEntry);
					}
				}
			}
			return list;
		}

		public DrivelistEntry Match(string model)
		{
			if (this.entries != null)
			{
				foreach (DrivelistEntry drivelistEntry in this.entries)
				{
					if (drivelistEntry.match(model))
					{
						return drivelistEntry;
					}
				}
			}
			return null;
		}

		public DrivelistEntry Match(string model, DrivelistEntryCategory category)
		{
			if (this.entries != null)
			{
				foreach (DrivelistEntry drivelistEntry in this.entries)
				{
					if (drivelistEntry.Category == category && drivelistEntry.match(model))
					{
						return drivelistEntry;
					}
				}
			}
			return null;
		}

		public void LoadVersion(XmlNodeList nodes)
		{
			this.version = nodes[0].InnerText;
		}

		public void Load(XmlNodeList nodes)
		{
			this.entries = new List<DrivelistEntry>();
			foreach (object obj in nodes)
			{
				XmlNode xmlNode = (XmlNode)obj;
				if (xmlNode.NodeType != XmlNodeType.Comment)
				{
					DrivelistEntry drivelistEntry = new DrivelistEntry();
					drivelistEntry.Description = xmlNode.Attributes["Description"].Value;
					drivelistEntry.Model = xmlNode.Attributes["Model"].Value;
					string value = xmlNode.Attributes["Size"].Value;
					if (value == "*")
					{
						drivelistEntry.Size = 0UL;
						drivelistEntry.AnySize = true;
					}
					else
					{
						drivelistEntry.Size = ulong.Parse(value);
						drivelistEntry.AnySize = false;
					}
					if (xmlNode.Attributes["Type"] != null)
					{
						value = xmlNode.Attributes["Type"].Value;
						if (value == null)
						{
							drivelistEntry.Type = DrivelistEntryType.TYPE_CONSTANT;
						}
						else if (value == "re")
						{
							drivelistEntry.Type = DrivelistEntryType.TYPE_RE;
						}
						else if (value == "constant")
						{
							drivelistEntry.Type = DrivelistEntryType.TYPE_CONSTANT;
						}
					}
					drivelistEntry.Profile = xmlNode.Attributes["ItemProfile"].Value;
					value = xmlNode.Attributes["Category"].Value;
					if (value == "Preferred")
					{
						drivelistEntry.Category = DrivelistEntryCategory.CATEGORY_PREFERRED;
					}
					else if (value == "Supported")
					{
						drivelistEntry.Category = DrivelistEntryCategory.CATEGORY_SUPPORTED;
					}
					else
					{
						if (!(value == "Blacklisted"))
						{
							string message = string.Format("Unknown drive list entry category {0}", value);
							throw new Exception(message);
						}
						drivelistEntry.Category = DrivelistEntryCategory.CATEGORY_BLACKLISTED;
					}
					this.entries.Add(drivelistEntry);
				}
			}
		}

		public void Load(XmlNode node, XmlNamespaceManager nsmgr)
		{
			XmlNodeList nodes = node.SelectNodes("cfg:Drive", nsmgr);
			this.Load(nodes);
		}

		public string Dump()
		{
			string text = "Drivelist" + Environment.NewLine;
			if (!string.IsNullOrEmpty(this.version))
			{
				text = text + "  Version=" + this.version + Environment.NewLine;
			}
			if (this.entries != null)
			{
				using (List<DrivelistEntry>.Enumerator enumerator = this.entries.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DrivelistEntry drivelistEntry = enumerator.Current;
						text += string.Format("  Model={0}, Description={1}, Size={2}, Type={3}, ItemProfile={4}, Category={5}", new object[]
						{
							drivelistEntry.Model,
							drivelistEntry.Description,
							drivelistEntry.Size,
							drivelistEntry.Type.ToString(),
							drivelistEntry.Profile,
							drivelistEntry.Category
						});
						text += Environment.NewLine;
					}
					goto IL_E9;
				}
			}
			text += "Empty list";
			IL_E9:
			text += Environment.NewLine;
			return text;
		}

		private static Drivelist _instance = null;

		protected static object lockObj = new object();

		private List<DrivelistEntry> entries;

		private string version = "";
	}
}
