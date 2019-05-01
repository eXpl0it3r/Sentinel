using System.Runtime.InteropServices;

namespace Stor
{
	public struct FEAT_CFG_FMT_HEADER
	{
		public ushort CacheHi;

		public ushort CacheLo;

		public ushort DashHi;

		public ushort DashLo;

		public byte CfgState;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
		public byte[] Pad;

		public ushort ParmBytes;
	}
}
