using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Win32;

namespace WDSystemConfig
{
	public class SystemConfig
	{
		public static XmlNodeList GetNodeList(string filePath, string pathName, string nodeName)
		{
			XmlNodeList result = null;
			if (string.IsNullOrEmpty(filePath))
			{
				throw new Exception("Empty configuration XML file path");
			}
			if (!SystemConfig.xmlFileTable.ContainsKey(filePath))
			{
				lock (SystemConfig.lockObj)
				{
					if (!SystemConfig.xmlFileTable.ContainsKey(filePath))
					{
						XmlFileNs xmlFileNs = new XmlFileNs();
						xmlFileNs.doc = new XmlDocument();
						xmlFileNs.doc.Load(filePath);
						xmlFileNs.nsmgr = new XmlNamespaceManager(xmlFileNs.doc.NameTable);
						xmlFileNs.nsmgr.AddNamespace("cfg", "http://www.wdc.com/bpg/nas/win/sysconfig");
						SystemConfig.xmlFileTable[filePath] = xmlFileNs;
					}
				}
			}
			if (SystemConfig.xmlFileTable.ContainsKey(filePath))
			{
				XmlFileNs xmlFileNs2 = SystemConfig.xmlFileTable[filePath];
				string xpath;
				if (!string.IsNullOrEmpty(pathName))
				{
					xpath = string.Format("/cfg:Config/cfg:{0}", pathName);
				}
				else
				{
					xpath = "/cfg:Config";
				}
				XmlNode xmlNode = xmlFileNs2.doc.SelectSingleNode(xpath, xmlFileNs2.nsmgr);
				if (xmlNode != null)
				{
					string xpath2 = string.Format("cfg:{0}", nodeName);
					result = xmlNode.SelectNodes(xpath2, xmlFileNs2.nsmgr);
				}
			}
			return result;
		}

		public static XmlNodeList GetNodeList(string pathName, string nodeName)
		{
			return SystemConfig.GetNodeList(SystemConfig.GetDefaultSystemConfigFilePath(), pathName, nodeName);
		}

		public static string GetDefaultSystemConfigFilePath()
		{
			if (!string.IsNullOrEmpty(SystemConfig.systemConfigFilePath))
			{
				return SystemConfig.systemConfigFilePath;
			}
			try
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(SystemConfig.SENTINEL_SYSTEMCONFIG_SUBKEY);
				if (registryKey != null)
				{
					return (string)registryKey.GetValue(SystemConfig.SYSTEMCONFIGFILEPATH_KEYVALUE);
				}
			}
			catch (Exception)
			{
			}
			return string.Empty;
		}

		public static string GetDriveListFilePath()
		{
			if (!string.IsNullOrEmpty(SystemConfig.driveListFilePath))
			{
				return SystemConfig.driveListFilePath;
			}
			try
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(SystemConfig.SENTINEL_SYSTEMCONFIG_SUBKEY);
				if (registryKey != null)
				{
					return (string)registryKey.GetValue(SystemConfig.DRIVELISTFILEPATH_KEYVALUE);
				}
			}
			catch (Exception)
			{
			}
			return string.Empty;
		}

		public static void SetSystemConfigFilePath(string p)
		{
			SystemConfig.systemConfigFilePath = p;
		}

		public static void SetDriveListFilePath(string p)
		{
			SystemConfig.driveListFilePath = p;
		}

		public static void Load()
		{
			SystemConfig.Load(SystemConfig.GetDefaultSystemConfigFilePath());
		}

		public static void Load(string filePath)
		{
			SystemConfig.systemConfigFilePath = filePath;
		}

		public static string Dump()
		{
			string str = "";
			string text = "WDC WD2009FYPX-09AAMB0";
			Range driveTemperatureRange = SystemConfig.GetDriveTemperatureRange(text, "Warning");
			if (driveTemperatureRange != null)
			{
				str += string.Format("Model={0} Warning Min={1} Max={2}", text, driveTemperatureRange.Min, driveTemperatureRange.Max);
			}
			else
			{
				str += string.Format("Cannot find drive model: {0}", text);
			}
			str += Environment.NewLine;
			str += Drivelist.GetInstance().Dump();
			str += DriveMap.GetInstance().Dump();
			str += EthernetPortProfiles.GetInstance().Dump();
			str += CpuList.GetInstance().Dump();
			str += SystemProfiles.GetInstance().Dump();
			str += ItemProfiles.GetInstance().Dump();
			return str + ThermalProfiles.GetInstance().Dump();
		}

		public static Range GetDriveTemperatureRange(string model, string rangeName)
		{
			DrivelistEntry drivelistEntry = Drivelist.GetInstance().Match(model);
			if (drivelistEntry != null && ItemProfiles.GetInstance() != null)
			{
				return ItemProfiles.GetInstance().GetRange(drivelistEntry.Profile, rangeName);
			}
			return null;
		}

		private static string SENTINEL_SYSTEMCONFIG_SUBKEY = "SOFTWARE\\Western Digital\\WD System Config";

		private static string SYSTEMCONFIGFILEPATH_KEYVALUE = "SystemConfigFilePath";

		private static string DRIVELISTFILEPATH_KEYVALUE = "DriveListFilePath";

		private static Dictionary<string, XmlFileNs> xmlFileTable = new Dictionary<string, XmlFileNs>();

		protected static object lockObj = new object();

		protected static string systemConfigFilePath = null;

		protected static string driveListFilePath = null;
	}
}
