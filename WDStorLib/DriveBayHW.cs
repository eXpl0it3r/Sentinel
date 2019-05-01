using System;
using WDHWLibDn;

namespace Stor
{
	public class DriveBayHW : IDisposable
	{
		public static DriveBayHW GetInstance()
		{
			if (DriveBayHW.hw == null)
			{
				DriveBayHW.hw = new DriveBayHW();
			}
			return DriveBayHW.hw;
		}

		public DriveBayHW()
		{
			this.hHwLib = WDHWLib.HWOpenHandle();
		}

		public void Dispose()
		{
			if (this.hHwLib != IntPtr.Zero)
			{
				lock (this.hwSyncObj)
				{
					WDHWLib.HWCloseHandle(this.hHwLib);
					this.hHwLib = IntPtr.Zero;
				}
			}
			GC.SuppressFinalize(this);
		}

		public bool GetBayStatus(ref DriveBay bay)
		{
			if (bay.ControlPort == -1)
			{
				return false;
			}
			lock (this.hwSyncObj)
			{
				WDHWLib.DRIVE_BAY_STATUS_DATA drive_BAY_STATUS_DATA = default(WDHWLib.DRIVE_BAY_STATUS_DATA);
				drive_BAY_STATUS_DATA.bay = (uint)bay.ControlPort;
				drive_BAY_STATUS_DATA.power = 0;
				drive_BAY_STATUS_DATA.present = 0;
				WDHWLib.HWStatus hwstatus;
				if (bay.IsBoot)
				{
					hwstatus = WDHWLib.HWGetBootDriveBayStatus(this.hHwLib, (uint)bay.ControlPort, ref drive_BAY_STATUS_DATA);
				}
				else
				{
					hwstatus = WDHWLib.HWGetDriveBayStatus(this.hHwLib, (uint)bay.ControlPort, ref drive_BAY_STATUS_DATA);
				}
				if (hwstatus != WDHWLib.HWStatus.HW_STATUS_OK)
				{
					return false;
				}
				bay.Power = (drive_BAY_STATUS_DATA.power == 1);
				bay.Present = (drive_BAY_STATUS_DATA.present == 1);
			}
			return true;
		}

		public bool SetBayPower(DriveBay bay, bool power)
		{
			if (bay.ControlPort == -1 || bay.IsBoot)
			{
				return false;
			}
			lock (this.hwSyncObj)
			{
				Logger.Info("Setting bay {0} power = {1}", new object[]
				{
					bay.Number,
					power
				});
				WDHWLib.HWStatus hwstatus = WDHWLib.HWSetDriveBayPower(this.hHwLib, (uint)bay.ControlPort, power);
				if (hwstatus != WDHWLib.HWStatus.HW_STATUS_OK)
				{
					Logger.Warn("Failed to set drive bay power {0}={1}: {2}", new object[]
					{
						bay.ControlPort,
						power,
						hwstatus.ToString()
					});
					return false;
				}
			}
			return true;
		}

		protected static DriveBayHW hw;

		protected IntPtr hHwLib = IntPtr.Zero;

		protected object hwSyncObj = new object();
	}
}
