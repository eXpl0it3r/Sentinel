namespace SpacesApi
{
	public enum PoolUsage
	{
		Unknown,
		Other,
		Unrestricted,
		ReservedForComputerSystem,
		ReservedAsDeltaReplicaContainer,
		ReservedForMigrationServices,
		ReservedForLocalReplicationServices,
		ReservedForRemoteReplicationServices,
		ReservedForSparing
	}
}
