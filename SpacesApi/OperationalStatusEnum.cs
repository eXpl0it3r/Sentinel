namespace SpacesApi
{
	public enum OperationalStatusEnum
	{
		Unknown,
		Other,
		OK,
		Degraded,
		Stressed,
		PredictiveFailure,
		Error,
		NonRecoverableError,
		Starting,
		Stopping,
		Stopped,
		InService,
		NoContact,
		LostCommunication,
		Aborted,
		Dormant,
		SupportingEntityInError,
		Completed,
		PowerMode,
		Relocating,
		MajorityDisksUnhealthy = 32768,
		MinorityDisksUnhealthy,
		Detached,
		Incomplete,
		FailedMedia,
		Split,
		StaleMetadata,
		IOError,
		CorruptMetadata
	}
}
