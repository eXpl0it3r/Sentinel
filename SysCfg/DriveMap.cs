using System;
using System.Collections.Generic;
using System.Xml;

namespace WDSystemConfig
{
	public class DriveMap
	{
		public static DriveMap GetInstance()
		{
			if (DriveMap._instance == null)
			{
				lock (DriveMap.lockObj)
				{
					if (DriveMap._instance == null)
					{
						DriveMap._instance = new DriveMap();
					}
				}
			}
			return DriveMap._instance;
		}

		public static DriveMap GetDriveMap()
		{
			return DriveMap.GetInstance();
		}

		private DriveMap()
		{
			XmlNodeList nodeList = SystemConfig.GetNodeList("DriveMap", "DriveMapEntry");
			this.Load(nodeList);
		}

		public int GetBayCount()
		{
			return this.entries.Count;
		}

		public List<DriveMapEntry> Entries
		{
			get
			{
				return this.entries;
			}
		}

		public string GetIndex(string fromIndex, MapIndexType fromType, MapIndexType toType)
		{
			foreach (DriveMapEntry driveMapEntry in this.entries)
			{
				try
				{
					if (driveMapEntry.indexData[fromType].index == fromIndex)
					{
						return driveMapEntry.indexData[toType].index;
					}
				}
				catch (Exception)
				{
				}
			}
			return null;
		}

		private void Load(XmlNodeList nodes)
		{
			this.entries = new List<DriveMapEntry>();
			foreach (object obj in nodes)
			{
				XmlNode xmlNode = (XmlNode)obj;
				if (xmlNode.NodeType != XmlNodeType.Comment)
				{
					DriveMapEntry driveMapEntry = new DriveMapEntry();
					if (xmlNode.Attributes["Bay"] != null)
					{
						driveMapEntry.Add(MapIndexType.INDEX_TYPE_BAY, xmlNode.Attributes["Bay"].Value);
					}
					else
					{
						driveMapEntry.Add(MapIndexType.INDEX_TYPE_BAY, null);
					}
					driveMapEntry.Add(MapIndexType.INDEX_TYPE_DRIVEPORT, xmlNode.Attributes["DrivePort"].Value);
					if (xmlNode.Attributes["LED"] != null)
					{
						driveMapEntry.Add(MapIndexType.INDEX_TYPE_LED, xmlNode.Attributes["LED"].Value);
					}
					else
					{
						driveMapEntry.Add(MapIndexType.INDEX_TYPE_LED, null);
					}
					if (xmlNode.Attributes["ControlPort"] != null)
					{
						driveMapEntry.Add(MapIndexType.INDEX_TYPE_CONTROLPORT, xmlNode.Attributes["ControlPort"].Value);
					}
					else
					{
						driveMapEntry.Add(MapIndexType.INDEX_TYPE_CONTROLPORT, null);
					}
					if (xmlNode.Attributes["Boot"] != null)
					{
						driveMapEntry.isBoot = true;
						driveMapEntry.Add(MapIndexType.INDEX_TYPE_BOOT, xmlNode.Attributes["Boot"].Value);
					}
					else
					{
						driveMapEntry.isBoot = false;
						driveMapEntry.Add(MapIndexType.INDEX_TYPE_BOOT, null);
					}
					this.entries.Add(driveMapEntry);
				}
			}
		}

		public void Load(XmlNode node, XmlNamespaceManager nsmgr)
		{
			XmlNodeList nodes = node.SelectNodes("cfg:DriveMapEntry", nsmgr);
			this.Load(nodes);
		}

		public string Dump()
		{
			string text = "DriveMap" + Environment.NewLine;
			int num = 0;
			foreach (DriveMapEntry driveMapEntry in this.entries)
			{
				text += string.Format("  Entry {0}: ", num);
				foreach (MapIndexType mapIndexType in driveMapEntry.indexData.Keys)
				{
					text += string.Format("{0}={1},", mapIndexType.ToString(), driveMapEntry.indexData[mapIndexType].index);
				}
				text += (driveMapEntry.IsBoot ? "Boot=1" : "Boot=0");
				text += Environment.NewLine;
				num++;
			}
			text += Environment.NewLine;
			return text;
		}

		private static DriveMap _instance = null;

		protected static object lockObj = new object();

		protected static DriveMap driveMap = null;

		protected List<DriveMapEntry> entries;
	}
}
