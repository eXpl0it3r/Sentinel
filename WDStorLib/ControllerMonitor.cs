using System;
using System.Collections.Generic;

namespace Stor
{
	public class ControllerMonitor
	{
		public event EventHandler<VolumeEvent> VolumeChanged;

		public event EventHandler<DriveEvent> DriveChanged;

		public event EventHandler<UpdateCompletedEvent> UpdateCompleted;

		public ControllerMonitor(ControllerMonitorConfig cfg)
		{
			this.config = cfg;
		}

		public List<Controller> Controllers
		{
			get
			{
				return this.controllers;
			}
			set
			{
				this.controllers = value;
			}
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

		public virtual StorApiStatus Start()
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public virtual StorApiStatus Stop()
		{
			return StorApiStatusEnum.STOR_NOT_IMPLEMENTED;
		}

		public virtual bool IsStarted()
		{
			return false;
		}

		public virtual void Update()
		{
			if (this.controllers != null)
			{
				foreach (Controller c in this.controllers)
				{
					this.UpdateController(c);
				}
			}
		}

		protected virtual void UpdateController(Controller c)
		{
		}

		protected virtual void OnVolumeChanged(VolumeEvent evt)
		{
			if (this.VolumeChanged != null)
			{
				this.VolumeChanged(this, evt);
			}
		}

		protected virtual void OnDriveChanged(DriveEvent evt)
		{
			if (this.DriveChanged != null)
			{
				this.DriveChanged(this, evt);
			}
		}

		protected virtual void OnUpdateCompleted(UpdateCompletedEvent evt)
		{
			if (this.UpdateCompleted != null)
			{
				this.UpdateCompleted(this, evt);
			}
		}

		protected object syncObject;

		protected ControllerMonitorConfig config;

		protected List<Controller> controllers;
	}
}
