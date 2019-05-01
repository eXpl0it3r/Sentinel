using System;
using System.Runtime.InteropServices;

namespace Stor
{
	public class StorHelper
	{
		public static IntPtr AllocateIntPtr<T>(T t)
		{
			int num = Marshal.SizeOf(t);
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			if (intPtr != IntPtr.Zero)
			{
				MemoryWrapper.FillMemory(intPtr, (uint)num, 0);
				Marshal.StructureToPtr(t, intPtr, false);
			}
			return intPtr;
		}

		public static T GetIntPtrData<T>(IntPtr p)
		{
			if (p != IntPtr.Zero)
			{
				return (T)((object)Marshal.PtrToStructure(p, typeof(T)));
			}
			return default(T);
		}

		public static void FreeIntPtr(IntPtr p)
		{
			if (p != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(p);
			}
		}
	}
}
