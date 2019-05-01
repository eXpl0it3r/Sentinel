using System;
using System.Runtime.InteropServices;

namespace Stor
{
	public static class MemoryWrapper
	{
		[DllImport("kernel32.dll")]
		public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

		[DllImport("kernel32.dll")]
		public static extern void MoveMemory(IntPtr destination, IntPtr source, uint length);

		[DllImport("kernel32.dll", EntryPoint = "RtlFillMemory")]
		public static extern void FillMemory(IntPtr destination, uint length, byte fill);
	}
}
