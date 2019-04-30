namespace SpacesApi
{
	public enum VirtualDiskUsage
	{
		Unknown,
		Other,
		Unrestricted,
		ReservedForComputerSystem,
		ReservedByReplicationServices,
		ReservedByMigrationServices,
		LocalReplicaSource,
		RemoteReplicaSource,
		LocalReplicaTarget,
		RemoteReplicaTarget,
		LocalReplicaSourceorTarget,
		RemoteReplicaSourceorTarget,
		DeltaReplicaTarget,
		ElementComponent,
		ReservedasPoolContributor,
		CompositeVolumeMember,
		CompositeVirtualDiskMember,
		ReservedforSparing
	}
}
