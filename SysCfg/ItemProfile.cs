using System.Collections.Generic;

namespace WDSystemConfig
{
	public class ItemProfile
	{
		public string Name;

		public Dictionary<string, Range> RangeProfile;

		public Dictionary<string, FanThrottle> ThrottleProfile;

		public bool DefaultProfile;
	}
}
