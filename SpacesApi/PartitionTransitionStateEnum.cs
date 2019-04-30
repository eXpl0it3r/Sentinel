namespace SpacesApi
{
	public enum PartitionTransitionStateEnum
	{
		Reserved,
		Stable,
		Extending,
		Shrinking,
		AutoReconfiguring,
		Restriping
	}
}
