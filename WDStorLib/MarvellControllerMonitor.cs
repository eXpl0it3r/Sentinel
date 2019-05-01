using System;
using System.Threading;

namespace Stor
{
	public class MarvellControllerMonitor : ControllerMonitor
	{
		public MarvellControllerMonitor(MarvellControllerMonitorConfig cfg) : base(cfg)
		{
			this.updateLock = new object();
			this.updateTimer = new Timer(new TimerCallback(this.Run), this.updateLock, -1, -1);
			this.updateTimerRunning = false;
		}

		public override StorApiStatus Start()
		{
			try
			{
				lock (this.updateLock)
				{
					if (!this.updateTimerRunning)
					{
						this.updateTimerRunning = true;
						this.updateTimer.Change(0, ((MarvellControllerMonitorConfig)this.config).monitorInterval * 1000);
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
				if (this.updateTimerRunning)
				{
					this.updateTimer.Change(-1, -1);
					this.updateTimerRunning = false;
				}
			}
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public override bool IsStarted()
		{
			return this.updateTimerRunning;
		}

		public void Run(object o)
		{
			lock (this.syncObject)
			{
				lock (o)
				{
					if (this.updateTimerRunning)
					{
						this.Update();
					}
				}
			}
		}

		protected override void UpdateController(Controller controller)
		{
			UpdateCompletedEvent updateCompletedEvent = new UpdateCompletedEvent();
			MarvellController marvellController = new MarvellController(((MarvellController)controller).AdapterId);
			StorApiStatus storApiStatus = marvellController.Update();
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				updateCompletedEvent.updateStatus = storApiStatus;
				updateCompletedEvent.controller = controller;
				this.OnUpdateCompleted(updateCompletedEvent);
				return;
			}
			ListComparer<Drive> listComparer = new ListComparer<Drive>();
			listComparer.Compare(controller.Drives, marvellController.Drives, new Func<Drive, Drive, bool>(Drive.CompareAndCopyStatus));
			ListComparer<Volume> listComparer2 = new ListComparer<Volume>();
			listComparer2.Compare(controller.Volumes, marvellController.Volumes, new Func<Volume, Volume, bool>(Volume.CompareInfo));
			controller.Drives = marvellController.Drives;
			controller.Volumes = marvellController.Volumes;
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
			updateCompletedEvent.updateStatus = StorApiStatusEnum.STOR_NO_ERROR;
			updateCompletedEvent.controller = controller;
			this.OnUpdateCompleted(updateCompletedEvent);
		}

		protected ManualResetEvent stopEvent;

		protected Timer updateTimer;

		protected object updateLock;

		protected bool updateTimerRunning;
	}
}
