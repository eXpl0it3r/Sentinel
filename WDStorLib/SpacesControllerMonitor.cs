using System;
using System.Threading;

namespace Stor
{
	public class SpacesControllerMonitor : ControllerMonitor
	{
		public event EventHandler<PoolEvent> PoolChanged;

		public SpacesControllerMonitor(SpacesControllerMonitorConfig cfg) : base(cfg)
		{
			this.updateLock = new object();
			this.updateTimer = new Timer(new TimerCallback(this.Run), this.updateLock, -1, -1);
			this.isrunning = false;
		}

		public SpacesController GetController()
		{
			if (this.controllers.Count > 0 && this.controllers[0] is SpacesController)
			{
				return (SpacesController)this.controllers[0];
			}
			return null;
		}

		public override StorApiStatus Start()
		{
			try
			{
				lock (this.updateLock)
				{
					if (!this.isrunning)
					{
						this.isrunning = true;
						this.updateTimer.Change(0, ((SpacesControllerMonitorConfig)this.config).monitorInterval * 1000);
					}
				}
			}
			catch (Exception)
			{
				return StorApiStatusEnum.STOR_UNKNOWN_ERROR;
			}
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public override StorApiStatus Stop()
		{
			lock (this.updateLock)
			{
				if (this.isrunning)
				{
					this.updateTimer.Change(-1, -1);
					this.isrunning = false;
				}
			}
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public override bool IsStarted()
		{
			return this.isrunning;
		}

		protected virtual void OnPoolChanged(PoolEvent evt)
		{
			if (this.PoolChanged != null)
			{
				this.PoolChanged(this, evt);
			}
		}

		public void Run(object o)
		{
			lock (this.syncObject)
			{
				lock (o)
				{
					if (this.isrunning)
					{
						this.Update();
					}
				}
			}
		}

		protected override void UpdateController(Controller controller)
		{
			UpdateCompletedEvent updateCompletedEvent = new UpdateCompletedEvent();
			SpacesController spacesController = new SpacesController(((SpacesController)controller).Id);
			StorApiStatus storApiStatus = spacesController.Update();
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				updateCompletedEvent.updateStatus = storApiStatus;
				updateCompletedEvent.controller = controller;
				this.OnUpdateCompleted(updateCompletedEvent);
				return;
			}
			ListComparer<Drive> listComparer = new ListComparer<Drive>();
			listComparer.Compare(controller.Drives, spacesController.Drives, new Func<Drive, Drive, bool>(SpacesDrive.CompareAndPreserve));
			ListComparer<Volume> listComparer2 = new ListComparer<Volume>();
			listComparer2.Compare(controller.Volumes, spacesController.Volumes, new Func<Volume, Volume, bool>(SpacesVolume.CompareInfo));
			ListComparer<SpacesPool> listComparer3 = new ListComparer<SpacesPool>();
			listComparer3.Compare(((SpacesController)controller).Pools, spacesController.Pools, new Func<SpacesPool, SpacesPool, bool>(SpacesPool.CompareInfo));
			controller.Drives = spacesController.Drives;
			controller.Volumes = spacesController.Volumes;
			((SpacesController)controller).Pools = spacesController.Pools;
			if (listComparer.AnythingChanged)
			{
				listComparer.removedItems.ForEach(delegate(ListComparer<Drive>.ItemData d)
				{
					DriveEvent driveEvent = new DriveEvent();
					driveEvent.controller = controller;
					driveEvent.drive = d.newItem;
					driveEvent.oldDrive = d.oldItem;
					driveEvent.type = ControllerEventType.EVENT_TYPE_REMOVED;
					this.OnDriveChanged(driveEvent);
				});
				listComparer.changedItems.ForEach(delegate(ListComparer<Drive>.ItemData d)
				{
					DriveEvent driveEvent = new DriveEvent();
					driveEvent.controller = controller;
					driveEvent.drive = d.newItem;
					driveEvent.oldDrive = d.oldItem;
					driveEvent.type = ControllerEventType.EVENT_TYPE_CHANGED;
					this.OnDriveChanged(driveEvent);
				});
				listComparer.addedItems.ForEach(delegate(ListComparer<Drive>.ItemData d)
				{
					DriveEvent driveEvent = new DriveEvent();
					driveEvent.controller = controller;
					driveEvent.drive = d.newItem;
					driveEvent.oldDrive = d.oldItem;
					driveEvent.type = ControllerEventType.EVENT_TYPE_ADDED;
					this.OnDriveChanged(driveEvent);
				});
			}
			if (listComparer2.AnythingChanged)
			{
				listComparer2.removedItems.ForEach(delegate(ListComparer<Volume>.ItemData d)
				{
					VolumeEvent volumeEvent = new VolumeEvent();
					volumeEvent.controller = controller;
					volumeEvent.volume = d.newItem;
					volumeEvent.oldVolume = d.oldItem;
					volumeEvent.type = ControllerEventType.EVENT_TYPE_REMOVED;
					this.OnVolumeChanged(volumeEvent);
				});
				listComparer2.changedItems.ForEach(delegate(ListComparer<Volume>.ItemData d)
				{
					VolumeEvent volumeEvent = new VolumeEvent();
					volumeEvent.controller = controller;
					volumeEvent.volume = d.newItem;
					volumeEvent.oldVolume = d.oldItem;
					volumeEvent.type = ControllerEventType.EVENT_TYPE_CHANGED;
					this.OnVolumeChanged(volumeEvent);
				});
				listComparer2.addedItems.ForEach(delegate(ListComparer<Volume>.ItemData d)
				{
					VolumeEvent volumeEvent = new VolumeEvent();
					volumeEvent.controller = controller;
					volumeEvent.volume = d.newItem;
					volumeEvent.oldVolume = d.oldItem;
					volumeEvent.type = ControllerEventType.EVENT_TYPE_ADDED;
					this.OnVolumeChanged(volumeEvent);
				});
			}
			if (listComparer3.AnythingChanged)
			{
				listComparer3.removedItems.ForEach(delegate(ListComparer<SpacesPool>.ItemData d)
				{
					PoolEvent poolEvent = new PoolEvent();
					poolEvent.controller = controller;
					poolEvent.pool = d.newItem;
					poolEvent.oldPool = d.oldItem;
					poolEvent.type = ControllerEventType.EVENT_TYPE_REMOVED;
					this.OnPoolChanged(poolEvent);
				});
				listComparer3.changedItems.ForEach(delegate(ListComparer<SpacesPool>.ItemData d)
				{
					PoolEvent poolEvent = new PoolEvent();
					poolEvent.controller = controller;
					poolEvent.pool = d.newItem;
					poolEvent.oldPool = d.oldItem;
					poolEvent.type = ControllerEventType.EVENT_TYPE_CHANGED;
					this.OnPoolChanged(poolEvent);
				});
				listComparer3.addedItems.ForEach(delegate(ListComparer<SpacesPool>.ItemData d)
				{
					PoolEvent poolEvent = new PoolEvent();
					poolEvent.controller = controller;
					poolEvent.pool = d.newItem;
					poolEvent.oldPool = d.oldItem;
					poolEvent.type = ControllerEventType.EVENT_TYPE_ADDED;
					this.OnPoolChanged(poolEvent);
				});
			}
			updateCompletedEvent.updateStatus = StorApiStatusEnum.STOR_NO_ERROR;
			updateCompletedEvent.controller = controller;
			this.OnUpdateCompleted(updateCompletedEvent);
		}

		protected Timer updateTimer;

		protected object updateLock;

		protected bool isrunning;
	}
}
