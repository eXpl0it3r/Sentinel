using System;
using System.Collections.Generic;
using System.Threading;
using WDHWLibDn;

namespace Stor
{
	public class DriveBayMonitor : IDisposable
	{
		public event EventHandler<DriveBayChangedEvent> BayChanged;

		public DriveBayMonitor(List<DriveBay> bays)
		{
			this.bays = bays;
			this.updateThread = new Thread(new ThreadStart(this.Run));
		}

		public virtual StorApiStatus Initialize()
		{
			if (this.hHwLib == IntPtr.Zero)
			{
				this.hHwLib = WDHWLib.HWOpenHandle();
				if (this.hHwLib == IntPtr.Zero)
				{
					Logger.Error("Failed to open HW lib handle!", new object[0]);
					return StorApiStatusEnum.STOR_INIT_FAILED;
				}
			}
			this.Update();
			foreach (DriveBay driveBay in this.bays)
			{
				if (!driveBay.Power)
				{
					driveBay.SetPower(true);
				}
			}
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public void Dispose()
		{
			this.Stop();
			if (this.hHwLib != IntPtr.Zero)
			{
				WDHWLib.HWCloseHandle(this.hHwLib);
				this.hHwLib = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}

		public object SyncObject
		{
			get
			{
				return this.syncObject;
			}
			set
			{
				this.syncObject = value;
			}
		}

		protected List<DriveBay> GetBayStatuses()
		{
			List<DriveBay> list = new List<DriveBay>();
			DriveBayHW instance = DriveBayHW.GetInstance();
			if (instance != null)
			{
				foreach (DriveBay driveBay in this.bays)
				{
					DriveBay driveBay2 = new DriveBay();
					driveBay2.Number = driveBay.Number;
					driveBay2.ControlPort = driveBay.ControlPort;
					driveBay2.Power = true;
					driveBay2.Present = true;
					driveBay2.DrivePort = (DrivePort)driveBay.DrivePort.Clone();
					driveBay2.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
					driveBay2.OldDrive = null;
					driveBay2.IsBoot = driveBay.IsBoot;
					driveBay2.BootIndex = driveBay.BootIndex;
					driveBay2.RaidVolume = driveBay.RaidVolume;
					driveBay2.SpacesPool = driveBay2.SpacesPool;
					instance.GetBayStatus(ref driveBay2);
					list.Add(driveBay2);
				}
			}
			return list;
		}

		public virtual StorApiStatus Start()
		{
			try
			{
				this.stopEvent.Reset();
				this.updateThread.Start();
			}
			catch (Exception)
			{
				return StorApiStatusEnum.STOR_UNKNOWN_ERROR;
			}
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public virtual StorApiStatus Stop()
		{
			if (this.updateThread.IsAlive)
			{
				this.stopEvent.Set();
				if (this.hHwLib != IntPtr.Zero)
				{
					WDHWLib.HWCancelWaitForDriveBayChange(this.hHwLib);
				}
				this.updateThread.Join();
			}
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public void Run()
		{
			int num = 0;
			while (!this.stopEvent.WaitOne(num))
			{
				if (this.hHwLib == IntPtr.Zero)
				{
					return;
				}
				WDHWLib.DRIVE_BAY_PRESENCE_DATA drive_BAY_PRESENCE_DATA = default(WDHWLib.DRIVE_BAY_PRESENCE_DATA);
				drive_BAY_PRESENCE_DATA.presenceChanged = 0u;
				WDHWLib.HWStatus hwstatus = WDHWLib.HWWaitForDriveBayChange(this.hHwLib, ref drive_BAY_PRESENCE_DATA);
				if (hwstatus != WDHWLib.HWStatus.HW_STATUS_OK)
				{
					if (num == 0)
					{
						Logger.Warn("Wait for bay change failed: {0}", new object[]
						{
							hwstatus.ToString()
						});
						num = 10000;
					}
				}
				else
				{
					if (drive_BAY_PRESENCE_DATA.presenceChanged != 0u)
					{
						this.Update();
					}
					num = 0;
				}
			}
		}

		protected virtual void Update()
		{
			List<DriveBay> bayStatuses = this.GetBayStatuses();
			List<DriveBayChangedEvent> events = new List<DriveBayChangedEvent>();
			lock (this.syncObject)
			{
				ListComparer<DriveBay> listComparer = new ListComparer<DriveBay>();
				listComparer.Compare(this.bays, bayStatuses, new Func<DriveBay, DriveBay, bool>(DriveBay.CompareStatus));
				listComparer.changedItems.ForEach(delegate(ListComparer<DriveBay>.ItemData d)
				{
					DriveBayChangedEvent driveBayChangedEvent = new DriveBayChangedEvent();
					driveBayChangedEvent.oldBay = (DriveBay)d.oldItem.Clone();
					d.oldItem.Power = d.newItem.Power;
					d.oldItem.Present = d.newItem.Present;
					driveBayChangedEvent.bay = d.oldItem;
					events.Add(driveBayChangedEvent);
				});
				foreach (DriveBayChangedEvent evt in events)
				{
					this.OnBayChanged(evt);
				}
			}
		}

		protected virtual void OnBayChanged(DriveBayChangedEvent evt)
		{
			if (this.BayChanged != null)
			{
				this.BayChanged(this, evt);
			}
		}

		protected object syncObject = new object();

		protected Thread updateThread;

		protected IntPtr hHwLib = IntPtr.Zero;

		public List<DriveBay> bays;

		public ManualResetEvent stopEvent = new ManualResetEvent(false);
	}
}
