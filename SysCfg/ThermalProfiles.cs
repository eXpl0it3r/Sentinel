using System;
using System.Collections.Generic;
using System.Xml;

namespace WDSystemConfig
{
	public class ThermalProfiles : Dictionary<string, ThermalProfile>
	{
		public static ThermalProfiles GetInstance()
		{
			if (ThermalProfiles.flowcharts == null)
			{
				lock (ThermalProfiles.lockObj)
				{
					if (ThermalProfiles.flowcharts == null)
					{
						ThermalProfiles.flowcharts = new ThermalProfiles();
					}
				}
			}
			return ThermalProfiles.flowcharts;
		}

		public static ThermalProfiles GetRangeProfiles()
		{
			return ThermalProfiles.GetInstance();
		}

		private ThermalProfiles()
		{
			XmlNodeList nodeList = SystemConfig.GetNodeList("ThermalProfiles", "ThermalProfile");
			this.Load(nodeList);
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

		public ThermalProfile GetFlowchart(string id)
		{
			ThermalProfile result;
			try
			{
				result = base[id];
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
					ThermalProfile thermalProfile = new ThermalProfile();
					thermalProfile.Name = ThermalProfiles.GetValue(xmlNode, "Name");
					thermalProfile.Steps = new Dictionary<string, Step>();
					foreach (object obj2 in xmlNode.ChildNodes)
					{
						XmlNode xmlNode2 = (XmlNode)obj2;
						if (xmlNode2.NodeType != XmlNodeType.Comment)
						{
							Step step = new Step();
							step.Id = ThermalProfiles.GetValue(xmlNode2, "Id");
							step.Name = ThermalProfiles.GetValue(xmlNode2, "Name");
							step.IsValue = ThermalProfiles.GetValue(xmlNode2, "Is");
							step.Then = ThermalProfiles.GetValue(xmlNode2, "Then");
							step.Else = ThermalProfiles.GetValue(xmlNode2, "Else");
							step.Set = ThermalProfiles.GetValue(xmlNode2, "Set");
							step.Next = ThermalProfiles.GetValue(xmlNode2, "Next");
							step.Fail = ThermalProfiles.GetValue(xmlNode2, "Fail");
							try
							{
								thermalProfile.Steps.Add(step.Id, step);
							}
							catch (Exception)
							{
								this.SetError(string.Format("{0}: duplicate step Id={0} Name={1}", thermalProfile.Name, step.Id, step.Name));
							}
						}
					}
					try
					{
						base.Add(thermalProfile.Name, thermalProfile);
					}
					catch (Exception)
					{
						this.SetError(string.Format("{0}: duplicate Thermal Profile Name={0}", thermalProfile.Name));
					}
				}
			}
		}

		public void Load(XmlNode node, XmlNamespaceManager nsmgr)
		{
			XmlNodeList nodes = node.SelectNodes("cfg:ThermalProfile", nsmgr);
			this.Load(nodes);
		}

		public string Dump()
		{
			string text = string.Empty;
			foreach (KeyValuePair<string, ThermalProfile> keyValuePair in this)
			{
				ThermalProfile value = keyValuePair.Value;
				if (value != null)
				{
					text += string.Format("Thermal Profile Name={0}", value.Name);
					text += Environment.NewLine;
					foreach (KeyValuePair<string, Step> keyValuePair2 in value.Steps)
					{
						Step value2 = keyValuePair2.Value;
						if (!string.IsNullOrEmpty(value2.IsValue))
						{
							text += string.Format("  Step: Id={0} Name={1} Is={2} Then={3} Else={4} Fail={5}", new object[]
							{
								value2.Id,
								value2.Name,
								value2.IsValue,
								value2.Then,
								value2.Else,
								value2.Fail
							});
						}
						else if (!string.IsNullOrEmpty(value2.Next))
						{
							text += string.Format("  Step: Id={0} Name={1} Set={2} Next={3}", new object[]
							{
								value2.Id,
								value2.Name,
								value2.Set,
								value2.Next
							});
						}
						else
						{
							text += string.Format("  Step: Id={0} Name={1} Set={2}", value2.Id, value2.Name, value2.Set);
						}
						text += Environment.NewLine;
					}
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

		private static ThermalProfiles flowcharts = null;

		protected static object lockObj = new object();

		protected List<string> Errors;
	}
}
