using System.Collections.Generic;

namespace WDSystemConfig
{
	public class SystemProfile
	{
		public string Name;

		public Dictionary<string, Item> Items;

		public List<MonitorProfile> MonitorProfiles;
	}
}
