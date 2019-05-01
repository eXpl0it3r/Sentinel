namespace Stor
{
	public struct FEAT_ID
	{
		public FEAT_CFG_FMT_HEADER Header;

		public uint Sig;

		public ushort FeatID;

		public ushort ParmID;

		public ushort ParmFieldType;

		public ushort ParmFieldLen;

		public uint Data;
	}
}
