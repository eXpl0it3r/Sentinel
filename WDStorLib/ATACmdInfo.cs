namespace Stor
{
	public class ATACmdInfo
	{
		public int timeout;

		public ATACmdFlags flags;

		public byte[] registers = new byte[7];

		public uint datalen;

		public byte[] data;
	}
}
