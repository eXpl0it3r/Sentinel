namespace WDSystemConfig
{
	public class Item
	{
		public Item()
		{
			this.Name = string.Empty;
			this.Type = ItemType.UNKNOWN;
			this.Model = string.Empty;
			this.ItemProfile = string.Empty;
		}

		public string Name;

		public ItemType Type;

		public string Model;

		public string ItemProfile;
	}
}
