using System.Collections.Generic;

namespace Stor
{
	public class CreateVolumeData
	{
		public static ulong VOLUME_MAX_CAPACITY = ulong.MaxValue;

		public static StripeSize VOLUME_DEFAULT_STRIPE_SIZE = StripeSize.SS_64K;

		public string name;

		public RaidLevel level;

		public List<Drive> drives;

		public StripeSize stripeSize;

		public ulong capacity;

		public bool writeCache;
	}
}
