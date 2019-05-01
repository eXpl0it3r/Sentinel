namespace Stor
{
	public class SmartAttribute
	{
		public byte id;

		public ushort flags;

		public byte current;

		public byte worst;

		public byte[] raw = new byte[6];
	}
}
