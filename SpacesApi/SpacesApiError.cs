namespace SpacesApi
{
	public enum SpacesApiError
	{
		Success,
		NotSupported,
		Unknown,
		Timeout,
		Failed,
		InvalidParameter,
		AccessDenied = 40001,
		NotEnoughResources,
		CannotConnectToStorageProvider = 46000,
		CannotConnectToStorageSubsystem,
		PoolUnhealthy = 48006,
		PoolReadonly
	}
}
