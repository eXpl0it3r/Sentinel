namespace WDSystemConfig
{
	public class Step
	{
		public Step()
		{
			this.Id = string.Empty;
			this.Name = string.Empty;
			this.IsValue = string.Empty;
			this.Then = string.Empty;
			this.Else = string.Empty;
			this.Set = string.Empty;
			this.Next = string.Empty;
			this.Fail = string.Empty;
		}

		public string Id;

		public string Name;

		public string IsValue;

		public string Then;

		public string Else;

		public string Set;

		public string Next;

		public string Fail;
	}
}
