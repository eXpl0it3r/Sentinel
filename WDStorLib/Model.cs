using System;
using System.Collections.Generic;
using System.Threading;
using SpacesApi;
using WDSystemConfig;

namespace Stor
{
	public class Model : IDisposable
	{
		public event EventHandler<ModelVolumeEvent> OnVolumeEvent;

		public event EventHandler<ModelDriveEvent> OnDriveEvent;

		public event EventHandler<ModelUpdateCompleteEvent> OnUpdateCompleteEvent;

		public Model()
		{
			this.modelLock = new object();
		}

		public virtual void InitializePersistentDriveInfos()
		{
			try
			{
				lock (this.modelLock)
				{
					this.persistentDriveInfoList.Load();
				}
			}
			catch (Exception ex)
			{
				Logger.Warn("Failed to load persistent drive info list: {0}", new object[]
				{
					ex
				});
			}
		}

		protected virtual StorApiStatus InitializeBays()
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			string text = "";
			this.driveBays = new List<DriveBay>();
			foreach (DriveMapEntry driveMapEntry in DriveMap.GetDriveMap().Entries)
			{
				DriveBay driveBay = new DriveBay();
				string text2 = null;
				text2 = driveMapEntry.GetData(MapIndexType.INDEX_TYPE_BAY);
				if (text2 != null)
				{
					try
					{
						driveBay.Number = int.Parse(text2);
						goto IL_8A;
					}
					catch (Exception)
					{
						Logger.Error("Invalid bay number {0}", new object[]
						{
							text2
						});
						return StorApiStatusEnum.STOR_INIT_FAILED;
					}
					goto IL_83;
				}
				goto IL_83;
				IL_8A:
				text2 = driveMapEntry.GetData(MapIndexType.INDEX_TYPE_DRIVEPORT);
				if (text2 == null)
				{
					Logger.Error("Null drive port", new object[0]);
					return StorApiStatusEnum.STOR_INIT_FAILED;
				}
				try
				{
					driveBay.DrivePort = DrivePort.FromString(text2);
				}
				catch (Exception)
				{
					Logger.Error("Invalid drive port {0}", new object[]
					{
						text2
					});
					return StorApiStatusEnum.STOR_INIT_FAILED;
				}
				text2 = driveMapEntry.GetData(MapIndexType.INDEX_TYPE_CONTROLPORT);
				if (text2 == null)
				{
					driveBay.ControlPort = -1;
				}
				else
				{
					try
					{
						driveBay.ControlPort = int.Parse(text2);
					}
					catch (Exception)
					{
						Logger.Error("Invalid control port: {0}", new object[]
						{
							text2
						});
						return StorApiStatusEnum.STOR_INIT_FAILED;
					}
				}
				driveBay.Present = false;
				driveBay.Power = true;
				driveBay.IsBoot = driveMapEntry.IsBoot;
				if (driveMapEntry.IsBoot)
				{
					text2 = driveMapEntry.GetData(MapIndexType.INDEX_TYPE_BOOT);
					if (text2 != null)
					{
						try
						{
							driveBay.BootIndex = int.Parse(text2);
						}
						catch (Exception)
						{
							Logger.Error("Invalid boot index: {0}", new object[]
							{
								text2
							});
							return StorApiStatusEnum.STOR_INIT_FAILED;
						}
					}
				}
				text += string.Format("Bay={0} DrivePort={1} ControlPort={2} Boot={3}", new object[]
				{
					driveBay.Number,
					driveBay.DrivePort.ToString(),
					driveBay.ControlPort,
					driveBay.BootIndex
				});
				text += Environment.NewLine;
				this.driveBays.Add(driveBay);
				continue;
				IL_83:
				driveBay.Number = -1;
				goto IL_8A;
			}
			Logger.Debug(text, new object[0]);
			this.driveBayMonitor = new DriveBayMonitor(this.driveBays);
			return this.driveBayMonitor.Initialize();
		}

		public virtual StorApiStatus Initialize()
		{
			return this.Initialize(null, null);
		}

		public virtual StorApiStatus Initialize(string sysConfigFilePath, string driveListFilePath)
		{
			StorApiStatus result;
			lock (this.modelLock)
			{
				StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
				try
				{
					if (sysConfigFilePath != null)
					{
						SystemConfig.SetSystemConfigFilePath(sysConfigFilePath);
					}
					if (driveListFilePath != null)
					{
						SystemConfig.SetDriveListFilePath(driveListFilePath);
					}
					Logger.Debug(SystemConfig.Dump(), new object[0]);
				}
				catch (Exception ex)
				{
					Logger.Error("Failed to load system configuration: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_INIT_FAILED;
				}
				storApiStatus = this.InitializeBays();
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					result = storApiStatus;
				}
				else
				{
					storApiStatus = MarvellUtil.Initialize();
					if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
					{
						Logger.Warn("Marvell initialization failed: {0}", new object[]
						{
							storApiStatus
						});
					}
					storApiStatus = SpacesUtil.Initialize();
					if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
					{
						Logger.Warn("Storage Spaces initialization failed: {0}", new object[]
						{
							storApiStatus
						});
					}
					this.controllers = new List<Controller>();
					storApiStatus = MarvellController.GetControllers(ref this.controllers);
					if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
					{
						Logger.Warn("Failed to get Marvell controllers: {0}", new object[]
						{
							storApiStatus
						});
					}
					else
					{
						Logger.Info("Marvell controllers found: {0}", new object[]
						{
							this.controllers.Count
						});
					}
					this.spacesControllers = new List<Controller>();
					storApiStatus = SpacesController.GetControllers(ref this.spacesControllers);
					if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
					{
						Logger.Warn("Failed to get Storage Spaces controllers: {0}", new object[]
						{
							storApiStatus
						});
					}
					this.monitor = new MarvellControllerMonitor(new MarvellControllerMonitorConfig
					{
						monitorInterval = 5
					});
					this.monitor.SyncObject = this.modelLock;
					this.monitor.Controllers = this.controllers;
					this.spacesMonitor = new SpacesControllerMonitor(new SpacesControllerMonitorConfig
					{
						monitorInterval = 5
					});
					this.spacesMonitor.SyncObject = this.modelLock;
					this.spacesMonitor.Controllers = this.spacesControllers;
					result = StorApiStatusEnum.STOR_NO_ERROR;
				}
			}
			return result;
		}

		public virtual void Dispose()
		{
			this.Stop();
			MarvellUtil.Finalize();
			SpacesUtil.Finalize();
		}

		public virtual StorApiStatus Start()
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			lock (this.modelLock)
			{
				this.monitor.DriveChanged += this.OnDriveChanged;
				this.monitor.VolumeChanged += this.OnVolumeChanged;
				this.monitor.UpdateCompleted += this.OnUpdateCompleted;
				storApiStatus = this.monitor.Start();
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					Logger.Error("Failed to start Marvell controller monitor", new object[0]);
				}
				this.driveBayMonitor.BayChanged += this.OnBayChanged;
				storApiStatus = this.driveBayMonitor.Start();
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					Logger.Error("Failed to start monitor for drive bays", new object[0]);
				}
				this.driveStatusMonitorEvent = new AutoResetEvent(false);
				this.driveStatusMonitor = new Thread(new ThreadStart(this.DriveStatusMonitor));
				this.driveStatusMonitor.Start();
				this.spacesMonitor.DriveChanged += this.OnSpacesDriveChanged;
				this.spacesMonitor.VolumeChanged += this.OnSpacesVolumeChanged;
				this.spacesMonitor.PoolChanged += this.OnSpacesPoolChanged;
				this.spacesMonitor.Start();
			}
			return storApiStatus;
		}

		public virtual StorApiStatus Stop()
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			lock (this.modelLock)
			{
				if (this.spacesMonitor != null && this.spacesMonitor.IsStarted())
				{
					Logger.Info("Stopping Spaces controller monitor", new object[0]);
					this.spacesMonitor.Stop();
					this.spacesMonitor.DriveChanged -= this.OnSpacesDriveChanged;
					this.spacesMonitor.VolumeChanged -= this.OnSpacesVolumeChanged;
					this.spacesMonitor.PoolChanged -= this.OnSpacesPoolChanged;
				}
				this.driveBayMonitor.Stop();
				this.driveBayMonitor.BayChanged -= this.OnBayChanged;
				if (this.monitor != null && this.monitor.IsStarted())
				{
					Logger.Info("Stopping Marvell controller monitor", new object[0]);
					storApiStatus = this.monitor.Stop();
					if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
					{
						Logger.Error("Failed to stop Marvell controller monitor", new object[0]);
					}
					this.monitor.DriveChanged -= this.OnDriveChanged;
					this.monitor.VolumeChanged -= this.OnVolumeChanged;
					this.monitor.UpdateCompleted -= this.OnUpdateCompleted;
				}
				if (this.driveStatusMonitor != null && this.driveStatusMonitor.IsAlive)
				{
					this.driveStatusMonitorEvent.Set();
					this.driveStatusMonitor.Join();
				}
				if (this.persistentDriveInfoList != null)
				{
					this.persistentDriveInfoList.Scrub();
				}
			}
			return storApiStatus;
		}

		public object SyncObject
		{
			get
			{
				return this.modelLock;
			}
		}

		public int ControllerCount
		{
			get
			{
				int count;
				lock (this.modelLock)
				{
					count = this.controllers.Count;
				}
				return count;
			}
		}

		public bool NeedsVerify
		{
			get
			{
				return this.needsVerify;
			}
			set
			{
				this.needsVerify = value;
			}
		}

		public List<Controller> Controllers
		{
			get
			{
				return this.controllers;
			}
		}

		public int BayCount
		{
			get
			{
				int count;
				lock (this.modelLock)
				{
					count = this.driveBays.Count;
				}
				return count;
			}
		}

		public List<Drive> Drives
		{
			get
			{
				List<Drive> result;
				lock (this.modelLock)
				{
					List<Drive> list = new List<Drive>();
					foreach (Controller controller in this.controllers)
					{
						foreach (Drive item in controller.Drives)
						{
							list.Add(item);
						}
					}
					result = list;
				}
				return result;
			}
		}

		public List<Controller> SpacesControllers
		{
			get
			{
				List<Controller> result;
				lock (this.modelLock)
				{
					result = this.spacesControllers;
				}
				return result;
			}
		}

		public virtual Drive GetSpacesDrive(string serial)
		{
			Drive result;
			lock (this.modelLock)
			{
				foreach (Controller controller in this.spacesControllers)
				{
					foreach (Drive drive in controller.Drives)
					{
						if (drive.Serial == serial)
						{
							return drive;
						}
					}
				}
				result = null;
			}
			return result;
		}

		public List<DriveBay> DriveBays
		{
			get
			{
				List<DriveBay> result;
				lock (this.modelLock)
				{
					List<DriveBay> list = new List<DriveBay>();
					foreach (DriveBay item in this.driveBays)
					{
						list.Add(item);
					}
					result = list;
				}
				return result;
			}
		}

		public virtual DriveBay GetDriveBay(Drive d)
		{
			DriveBay result;
			lock (this.modelLock)
			{
				result = this.FindDriveBayFromDrive(d);
			}
			return result;
		}

		public virtual Drive GetDriveFromBay(DriveBay b)
		{
			Drive result;
			lock (this.modelLock)
			{
				Drive drive = this.FindDriveFromDriveBay(b);
				if (drive == null && (b.Status == DriveBayStatusEnum.DRIVE_BAY_INVALID_WHITELIST || b.Status == DriveBayStatusEnum.DRIVE_BAY_INVALID_DRIVE_SIZE || b.Status == DriveBayStatusEnum.DRIVE_BAY_MISSING_DRIVE))
				{
					drive = b.OldDrive;
				}
				result = drive;
			}
			return result;
		}

		public Drivelist Drivelist
		{
			get
			{
				return Drivelist.GetDrivelist();
			}
		}

		public List<Volume> Volumes
		{
			get
			{
				List<Volume> result;
				lock (this.modelLock)
				{
					List<Volume> list = new List<Volume>();
					foreach (Controller controller in this.controllers)
					{
						foreach (Volume item in controller.Volumes)
						{
							list.Add(item);
						}
					}
					result = list;
				}
				return result;
			}
		}

		public Volume SystemVolume
		{
			get
			{
				Volume systemVolume;
				lock (this.modelLock)
				{
					systemVolume = this.GetSystemVolume();
				}
				return systemVolume;
			}
		}

		public void OnDriveChanged(object sender, DriveEvent evt)
		{
			ModelDriveEvent modelDriveEvent = new ModelDriveEvent();
			if (evt.controller is SpacesController)
			{
				return;
			}
			if (evt.type == ControllerEventType.EVENT_TYPE_ADDED)
			{
				DriveBay driveBay = this.FindDriveBayFromDrive(evt.drive);
				if (driveBay == null)
				{
					Logger.Warn("Cannot find bay associated with drive {0}", new object[]
					{
						evt.drive.GetNumericId()
					});
					return;
				}
				DriveBay oldBay = (DriveBay)driveBay.Clone();
				if (driveBay.IsBoot)
				{
					evt.drive.IsSystem = true;
				}
				this.UpdateDriveStatus(evt.drive);
				if (!driveBay.IsBoot && driveBay.Status == DriveBayStatusEnum.DRIVE_BAY_MISSING_DRIVE)
				{
					Drive oldDrive = driveBay.OldDrive;
					driveBay.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
					driveBay.OldDrive = null;
					driveBay.RaidVolume = null;
					driveBay.SpacesPool = null;
					modelDriveEvent.drive = oldDrive;
					modelDriveEvent.oldDrive = oldDrive;
					modelDriveEvent.bay = driveBay;
					modelDriveEvent.oldBay = oldBay;
					modelDriveEvent.type = ControllerEventType.EVENT_TYPE_REMOVED;
					this.GenerateDriveStatusEvent(modelDriveEvent);
					Logger.Info("Clear bay {0} missing drive status due to drive inserted", new object[]
					{
						driveBay.Number
					});
				}
				if (this.persistentDriveInfoList != null)
				{
					PersistentDriveInfo persistentDriveInfo = this.persistentDriveInfoList.Find(evt.drive.Serial);
					if (persistentDriveInfo == null)
					{
						persistentDriveInfo = new PersistentDriveInfo();
						persistentDriveInfo.serial = evt.drive.Serial;
						this.persistentDriveInfoList.Add(persistentDriveInfo);
					}
					else
					{
						this.persistentDriveInfoList.Update(persistentDriveInfo);
					}
				}
				if (!this.CheckDriveList(evt.drive))
				{
					Logger.Warn("Drive id={0} model={1} serial={2} failed whitelist", new object[]
					{
						evt.drive.GetNumericId(),
						evt.drive.Model,
						evt.drive.Serial
					});
					driveBay.Status = DriveBayStatusEnum.DRIVE_BAY_INVALID_WHITELIST;
					if (!evt.drive.IsSystem)
					{
						driveBay.SetPower(false);
					}
					modelDriveEvent.drive = evt.drive;
					modelDriveEvent.oldDrive = evt.oldDrive;
					modelDriveEvent.bay = driveBay;
					modelDriveEvent.oldBay = oldBay;
					modelDriveEvent.type = ControllerEventType.EVENT_TYPE_ADDED;
					this.GenerateDriveStatusEvent(modelDriveEvent);
					return;
				}
				if (!this.IsDriveSizeOk(evt.drive))
				{
					Logger.Warn("Drive id={0} model={1} serial={2} failed size check", new object[]
					{
						evt.drive.GetNumericId(),
						evt.drive.Model,
						evt.drive.Serial
					});
					driveBay.Status = DriveBayStatusEnum.DRIVE_BAY_INVALID_DRIVE_SIZE;
					if (!evt.drive.IsSystem)
					{
						driveBay.SetPower(false);
					}
					modelDriveEvent.drive = evt.drive;
					modelDriveEvent.oldDrive = evt.oldDrive;
					modelDriveEvent.bay = driveBay;
					modelDriveEvent.oldBay = oldBay;
					modelDriveEvent.type = ControllerEventType.EVENT_TYPE_ADDED;
					this.GenerateDriveStatusEvent(modelDriveEvent);
					return;
				}
				driveBay.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
				driveBay.OldDrive = null;
				this.HandleSpecialDrive(evt.drive);
				this.HandleNewDrive(evt.drive);
				modelDriveEvent.drive = evt.drive;
				modelDriveEvent.oldDrive = evt.oldDrive;
				modelDriveEvent.bay = driveBay;
				modelDriveEvent.oldBay = oldBay;
				modelDriveEvent.type = ControllerEventType.EVENT_TYPE_ADDED;
				this.GenerateDriveStatusEvent(modelDriveEvent);
				return;
			}
			else
			{
				if (evt.type != ControllerEventType.EVENT_TYPE_REMOVED)
				{
					if (evt.type == ControllerEventType.EVENT_TYPE_CHANGED)
					{
						DriveBay driveBay2 = this.FindDriveBayFromDrive(evt.drive);
						if (driveBay2 == null)
						{
							return;
						}
						DriveBay oldBay2 = (DriveBay)driveBay2.Clone();
						if (driveBay2.IsBoot)
						{
							evt.drive.IsSystem = true;
						}
						if (evt.drive != null && evt.oldDrive != null)
						{
							modelDriveEvent.drive = evt.drive;
							modelDriveEvent.oldDrive = evt.oldDrive;
							modelDriveEvent.bay = driveBay2;
							modelDriveEvent.oldBay = oldBay2;
							modelDriveEvent.type = ControllerEventType.EVENT_TYPE_CHANGED;
							this.GenerateDriveStatusEvent(modelDriveEvent);
						}
					}
					return;
				}
				DriveBay driveBay3 = this.FindDriveBayFromDrive(evt.oldDrive);
				if (driveBay3 == null)
				{
					return;
				}
				DriveBay oldBay3 = (DriveBay)driveBay3.Clone();
				if (!driveBay3.Power && (driveBay3.Status == DriveBayStatusEnum.DRIVE_BAY_INVALID_WHITELIST || driveBay3.Status == DriveBayStatusEnum.DRIVE_BAY_INVALID_DRIVE_SIZE))
				{
					driveBay3.OldDrive = evt.oldDrive;
				}
				else if (driveBay3.Power && !driveBay3.IsBoot && (evt.oldDrive.Domain == DriveDomain.DRIVE_DOMAIN_RAID || evt.oldDrive.Domain == DriveDomain.DRIVE_DOMAIN_SPACES))
				{
					bool flag = false;
					if (evt.oldDrive.Domain == DriveDomain.DRIVE_DOMAIN_SPACES && driveBay3.SpacesPool != null)
					{
						SpacesController spacesController = SpacesController.GetSpacesController();
						if (spacesController != null)
						{
							SpacesPool spacesPool = null;
							if (spacesController.GetPool(driveBay3.SpacesPool.Id, ref spacesPool) == StorApiStatusEnum.STOR_NO_ERROR && spacesPool != null)
							{
								flag = true;
							}
							else
							{
								Logger.Warn("Drive {0} is not configured for Spaces: pool is not found", new object[]
								{
									evt.oldDrive.Port,
									driveBay3.SpacesPool.Name
								});
							}
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						driveBay3.Status = DriveBayStatusEnum.DRIVE_BAY_MISSING_DRIVE;
						driveBay3.OldDrive = evt.oldDrive;
					}
					else
					{
						driveBay3.OldDrive = null;
						driveBay3.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
						driveBay3.RaidVolume = null;
						driveBay3.SpacesPool = null;
					}
				}
				else
				{
					driveBay3.OldDrive = null;
					driveBay3.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
					driveBay3.RaidVolume = null;
					driveBay3.SpacesPool = null;
				}
				modelDriveEvent.drive = evt.drive;
				modelDriveEvent.oldDrive = evt.oldDrive;
				modelDriveEvent.bay = driveBay3;
				modelDriveEvent.oldBay = oldBay3;
				modelDriveEvent.type = ControllerEventType.EVENT_TYPE_REMOVED;
				this.GenerateDriveStatusEvent(modelDriveEvent);
				return;
			}
		}

		public void OnVolumeChanged(object sender, VolumeEvent evt)
		{
			ModelVolumeEvent modelVolumeEvent = new ModelVolumeEvent();
			if (evt.controller is SpacesController)
			{
				return;
			}
			if (evt.type == ControllerEventType.EVENT_TYPE_ADDED || evt.type == ControllerEventType.EVENT_TYPE_CHANGED)
			{
				VolumeStatus volumeStatus = VolumeStatus.VOLUME_UNKNOWN;
				VolumeStatus volumeStatus2 = VolumeStatus.VOLUME_UNKNOWN;
				float num = 0f;
				float num2 = 0f;
				if (evt.volume != null)
				{
					volumeStatus2 = evt.volume.Status;
					num2 = evt.volume.Progress;
				}
				if (evt.oldVolume != null)
				{
					volumeStatus = evt.oldVolume.Status;
					num = evt.oldVolume.Progress;
				}
				bool flag = false;
				if (volumeStatus2 != volumeStatus)
				{
					flag = true;
				}
				else if ((volumeStatus2 == VolumeStatus.VOLUME_INITIALIZING || volumeStatus2 == VolumeStatus.VOLUME_MIGRATING || volumeStatus2 == VolumeStatus.VOLUME_REBUILDING || volumeStatus2 == VolumeStatus.VOLUME_VERIFYING) && num != num2)
				{
					flag = true;
				}
				if (flag)
				{
					modelVolumeEvent.type = evt.type;
					modelVolumeEvent.volume = evt.volume;
					modelVolumeEvent.oldVolume = evt.oldVolume;
					this.GenerateVolumeStatusEvent(modelVolumeEvent);
				}
				if (volumeStatus2 != VolumeStatus.VOLUME_NORMAL && volumeStatus2 != VolumeStatus.VOLUME_REBUILDING)
				{
					return;
				}
				using (List<DriveBay>.Enumerator enumerator = this.driveBays.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DriveBay driveBay = enumerator.Current;
						if (!driveBay.IsBoot && driveBay.Status == DriveBayStatusEnum.DRIVE_BAY_MISSING_DRIVE && driveBay.RaidVolume != null && driveBay.RaidVolume.Equals(evt.volume))
						{
							Drive oldDrive = driveBay.OldDrive;
							driveBay.RaidVolume = null;
							driveBay.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
							driveBay.OldDrive = null;
							ModelDriveEvent modelDriveEvent = new ModelDriveEvent();
							modelDriveEvent.drive = oldDrive;
							modelDriveEvent.oldDrive = oldDrive;
							modelDriveEvent.bay = driveBay;
							modelDriveEvent.oldBay = driveBay;
							modelDriveEvent.type = ControllerEventType.EVENT_TYPE_REMOVED;
							Logger.Info("Clear bay {0} missing drive status due to Volume {1} status normal", new object[]
							{
								driveBay.Number,
								evt.volume.Name
							});
							this.GenerateDriveStatusEvent(modelDriveEvent);
						}
					}
					return;
				}
			}
			if (evt.type == ControllerEventType.EVENT_TYPE_REMOVED)
			{
				Volume oldVolume = evt.oldVolume;
				if (oldVolume == null)
				{
					return;
				}
				foreach (DriveBay driveBay2 in this.driveBays)
				{
					if (!driveBay2.IsBoot && driveBay2.Status == DriveBayStatusEnum.DRIVE_BAY_MISSING_DRIVE && driveBay2.RaidVolume != null && driveBay2.RaidVolume.Equals(oldVolume))
					{
						Drive oldDrive2 = driveBay2.OldDrive;
						driveBay2.RaidVolume = null;
						driveBay2.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
						driveBay2.OldDrive = null;
						ModelDriveEvent modelDriveEvent2 = new ModelDriveEvent();
						modelDriveEvent2.drive = oldDrive2;
						modelDriveEvent2.oldDrive = oldDrive2;
						modelDriveEvent2.bay = driveBay2;
						modelDriveEvent2.oldBay = driveBay2;
						modelDriveEvent2.type = ControllerEventType.EVENT_TYPE_REMOVED;
						Logger.Info("Clear bay {0} missing drive status due to Volume {1} removed", new object[]
						{
							driveBay2.Number,
							oldVolume.Name
						});
						this.GenerateDriveStatusEvent(modelDriveEvent2);
					}
				}
				modelVolumeEvent.type = evt.type;
				modelVolumeEvent.volume = null;
				modelVolumeEvent.oldVolume = evt.oldVolume;
				this.GenerateVolumeStatusEvent(modelVolumeEvent);
			}
		}

		public void OnUpdateCompleted(object sender, UpdateCompletedEvent evt)
		{
			this.UpdateBayStorageInfo();
			EventHandler<ModelUpdateCompleteEvent> onUpdateCompleteEvent = this.OnUpdateCompleteEvent;
			if (onUpdateCompleteEvent != null)
			{
				ModelUpdateCompleteEvent e = new ModelUpdateCompleteEvent();
				onUpdateCompleteEvent(this, e);
			}
			this.CheckVolumeOperations();
			if (this.persistentDriveInfoList.wasEmptyOnLoad)
			{
				this.persistentDriveInfoList.wasEmptyOnLoad = false;
			}
		}

		protected void OnBayChanged(object sender, DriveBayChangedEvent evt)
		{
			Logger.Debug("OnBayChanged: {0}", new object[]
			{
				evt.ToString()
			});
			ModelDriveEvent modelDriveEvent = new ModelDriveEvent();
			if (evt.oldBay.Present && !evt.bay.Present)
			{
				if (!evt.bay.IsBoot && (evt.oldBay.Status == DriveBayStatusEnum.DRIVE_BAY_INVALID_WHITELIST || evt.oldBay.Status == DriveBayStatusEnum.DRIVE_BAY_INVALID_DRIVE_SIZE))
				{
					evt.bay.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
					evt.bay.OldDrive = null;
					modelDriveEvent.drive = evt.oldBay.OldDrive;
					modelDriveEvent.oldDrive = evt.oldBay.OldDrive;
					modelDriveEvent.bay = evt.bay;
					modelDriveEvent.oldBay = evt.oldBay;
					modelDriveEvent.type = ControllerEventType.EVENT_TYPE_REMOVED;
					this.GenerateDriveStatusEvent(modelDriveEvent);
					return;
				}
			}
			else if (!evt.oldBay.Present && evt.bay.Present)
			{
				evt.bay.SetPower(true);
			}
		}

		protected virtual void UpdateBayStorageInfo()
		{
			foreach (DriveBay driveBay in this.driveBays)
			{
				Drive drive = this.FindDriveFromDriveBay(driveBay);
				if (drive != null)
				{
					StorApiStatus a = StorApiStatusEnum.STOR_NO_ERROR;
					if (drive.Domain == DriveDomain.DRIVE_DOMAIN_RAID)
					{
						List<Volume> list = null;
						a = drive.Controller.GetDriveVolumes(drive, ref list);
						if (a == StorApiStatusEnum.STOR_NO_ERROR && list != null)
						{
							driveBay.RaidVolume = list[0];
						}
					}
					else if (drive.Domain == DriveDomain.DRIVE_DOMAIN_SPACES)
					{
						SpacesController spacesController = SpacesController.GetSpacesController();
						SpacesDrive spacesDrive = null;
						a = spacesController.GetSpacesDrive(drive.Serial, ref spacesDrive);
						if (a == StorApiStatusEnum.STOR_NO_ERROR && spacesDrive != null)
						{
							SpacesPool spacesPool = null;
							a = spacesController.GetDrivePool(spacesDrive, ref spacesPool);
							if (a == StorApiStatusEnum.STOR_NO_ERROR && spacesPool != null)
							{
								driveBay.SpacesPool = spacesPool;
							}
						}
					}
					else
					{
						driveBay.RaidVolume = null;
						driveBay.SpacesPool = null;
					}
				}
			}
		}

		protected virtual void OnSpacesDriveChanged(object sender, DriveEvent evt)
		{
			Logger.Debug("OnSpacesDriveChanged: {0}", new object[]
			{
				evt.ToString()
			});
		}

		protected virtual void OnSpacesVolumeChanged(object sender, VolumeEvent evt)
		{
			Logger.Debug("OnSpacesVolumeChanged: {0}", new object[]
			{
				evt.ToString()
			});
		}

		protected virtual void OnSpacesPoolChanged(object sender, PoolEvent evt)
		{
			Logger.Debug("OnSpacesPoolChanged: {0}", new object[]
			{
				evt.ToString()
			});
			ModelDriveEvent modelDriveEvent = new ModelDriveEvent();
			if (evt.type == ControllerEventType.EVENT_TYPE_REMOVED)
			{
				if (evt.oldPool == null)
				{
					return;
				}
				using (List<DriveBay>.Enumerator enumerator = this.driveBays.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						DriveBay driveBay = enumerator.Current;
						if (driveBay.Status == DriveBayStatusEnum.DRIVE_BAY_MISSING_DRIVE && driveBay.SpacesPool != null && driveBay.SpacesPool.Equals(evt.oldPool))
						{
							Drive oldDrive = driveBay.OldDrive;
							driveBay.SpacesPool = null;
							driveBay.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
							driveBay.OldDrive = null;
							modelDriveEvent.drive = oldDrive;
							modelDriveEvent.oldDrive = oldDrive;
							modelDriveEvent.bay = driveBay;
							modelDriveEvent.oldBay = driveBay;
							modelDriveEvent.type = ControllerEventType.EVENT_TYPE_REMOVED;
							Logger.Info("Clear bay {0} missing drive status due to Spaces Pool {1} removed", new object[]
							{
								driveBay.Number,
								evt.oldPool.Name
							});
							this.GenerateDriveStatusEvent(modelDriveEvent);
						}
					}
					return;
				}
			}
			if (evt.type == ControllerEventType.EVENT_TYPE_CHANGED)
			{
				if (evt.pool == null)
				{
					return;
				}
				if (evt.pool.Health == HealthStatusEnum.Healthy)
				{
					foreach (DriveBay driveBay2 in this.driveBays)
					{
						if (driveBay2.Status == DriveBayStatusEnum.DRIVE_BAY_MISSING_DRIVE && driveBay2.SpacesPool != null && driveBay2.SpacesPool.Equals(evt.pool))
						{
							Drive oldDrive2 = driveBay2.OldDrive;
							driveBay2.SpacesPool = null;
							driveBay2.Status = DriveBayStatusEnum.DRIVE_BAY_NONE;
							driveBay2.OldDrive = null;
							modelDriveEvent.drive = oldDrive2;
							modelDriveEvent.oldDrive = oldDrive2;
							modelDriveEvent.bay = driveBay2;
							modelDriveEvent.oldBay = driveBay2;
							modelDriveEvent.type = ControllerEventType.EVENT_TYPE_REMOVED;
							Logger.Info("Clear bay {0} missing drive status due to Spaces Pool {1} healthy", new object[]
							{
								driveBay2.Number,
								evt.pool.Name
							});
							this.GenerateDriveStatusEvent(modelDriveEvent);
						}
					}
				}
			}
		}

		protected virtual bool CheckDriveList(Drive d)
		{
			bool flag = true;
			Drivelist drivelist = Drivelist.GetDrivelist();
			if (drivelist.Match(d.Model, DrivelistEntryCategory.CATEGORY_BLACKLISTED) != null)
			{
				d.Category = DriveCategory.DRIVE_CATEGORY_BLACKLISTED;
				flag = false;
			}
			else if (drivelist.Match(d.Model, DrivelistEntryCategory.CATEGORY_PREFERRED) != null)
			{
				d.Category = DriveCategory.DRIVE_CATEGORY_PREFERRED;
			}
			else if (drivelist.Match(d.Model, DrivelistEntryCategory.CATEGORY_SUPPORTED) != null)
			{
				d.Category = DriveCategory.DRIVE_CATEGORY_SUPPORTED;
			}
			else
			{
				d.Category = DriveCategory.DRIVE_CATEGORY_UNKNOWN;
				flag = false;
			}
			if (!flag && d.Category == DriveCategory.DRIVE_CATEGORY_UNKNOWN && this.persistentDriveInfoList != null)
			{
				PersistentDriveInfo persistentDriveInfo = this.persistentDriveInfoList.Find(d.Serial);
				if (persistentDriveInfo.unsupportedConsent)
				{
					flag = true;
				}
			}
			return flag;
		}

		protected virtual bool IsDriveSizeOk(Drive d)
		{
			bool result = true;
			DriveBay driveBay = this.FindDriveBayFromDrive(d);
			if (driveBay == null)
			{
				return result;
			}
			if (driveBay.IsBoot)
			{
				Volume systemVolume = this.GetSystemVolume();
				if (systemVolume != null && d.Capacity < systemVolume.GetMinDiskSize())
				{
					result = false;
				}
			}
			return result;
		}

		protected virtual DriveBay FindDriveBayFromDrive(Drive d)
		{
			foreach (DriveBay driveBay in this.driveBays)
			{
				if (driveBay.DrivePort.IsSame(d.Port))
				{
					return driveBay;
				}
			}
			return null;
		}

		protected virtual DriveBay FindDriveBayByNumber(int baynum)
		{
			foreach (DriveBay driveBay in this.driveBays)
			{
				if (driveBay.Number == baynum && driveBay.Number != -1)
				{
					return driveBay;
				}
			}
			return null;
		}

		protected virtual Drive FindDriveFromDriveBay(DriveBay b)
		{
			foreach (Controller controller in this.controllers)
			{
				if (controller is MarvellController)
				{
					foreach (Drive drive in controller.Drives)
					{
						if (b.DrivePort.IsSame(drive.Port))
						{
							return drive;
						}
					}
				}
			}
			return null;
		}

		protected virtual void HandleSpecialDrive(Drive d)
		{
			if (d.Model.StartsWith("Hitachi") || d.Model.StartsWith("HGST"))
			{
				this.HandleHitachiDrive(d);
				return;
			}
			if (d.Model == "WDC WD5000LUCT-63C26Y0" || d.Model == "WDC WD5000LUCT-63Y8HY0" || d.Model == "WDC WD3200BUCT-63TWBY0")
			{
				this.EnableWDTLER(d);
			}
		}

		protected virtual void EnableWDTLER(Drive d)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			uint num = 0u;
			storApiStatus = d.GetAC55(5, 3, ref num);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Logger.Warn("Failed to get drive {1} reader TLER: {0}", new object[]
				{
					storApiStatus,
					d.GetNumericId()
				});
			}
			else if (num != 7000u)
			{
				Logger.Info("Setting reader TLER on drive {0}", new object[]
				{
					d.GetNumericId()
				});
				storApiStatus = d.SetAC55(5, 3, 3, 2, 7000u);
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					Logger.Warn("Failed to set drive {1} reader TLER: {0}", new object[]
					{
						storApiStatus,
						d.GetNumericId()
					});
				}
			}
			storApiStatus = d.GetAC55(5, 2, ref num);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Logger.Warn("Failed to get drive {1} writer TLER: {0}", new object[]
				{
					storApiStatus,
					d.GetNumericId()
				});
			}
			else if (num != 7000u)
			{
				Logger.Info("Setting writer TLER on drive {0}", new object[]
				{
					d.GetNumericId()
				});
				storApiStatus = d.SetAC55(5, 2, 3, 2, 7000u);
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					Logger.Warn("Failed to set drive {1} writer TLER: {0}", new object[]
					{
						storApiStatus,
						d.GetNumericId()
					});
				}
			}
			storApiStatus = d.GetAC55(5, 1, ref num);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Logger.Warn("Failed to get drive {1} TLER enabled: {0}", new object[]
				{
					storApiStatus,
					d.GetNumericId()
				});
				return;
			}
			if (num != 1u)
			{
				Logger.Info("Enabling TLER on drive {0}", new object[]
				{
					d.GetNumericId()
				});
				storApiStatus = d.SetAC55(5, 1, 1, 1, 1u);
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					Logger.Warn("Failed to enable drive {1} TLER: {0}", new object[]
					{
						storApiStatus,
						d.GetNumericId()
					});
				}
			}
		}

		protected virtual void HandleHitachiDrive(Drive d)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			bool flag = false;
			storApiStatus = d.GetSSC(ref flag);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Logger.Warn("Failed to get drive {1} SSC: {0}", new object[]
				{
					storApiStatus,
					d.GetNumericId()
				});
			}
			else if (!flag)
			{
				Logger.Info("Setting SSC on drive {0}", new object[]
				{
					d.GetNumericId()
				});
				storApiStatus = d.SetSSC(true);
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					Logger.Warn("Failed to set drive {1} SSC: {0}", new object[]
					{
						storApiStatus,
						d.GetNumericId()
					});
				}
			}
			ushort num = 0;
			storApiStatus = d.GetErrorRecoveryTime(true, ref num);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Logger.Warn("Failed to get drive {1} reader error timeout: {0}", new object[]
				{
					storApiStatus,
					d.GetNumericId()
				});
			}
			else if (num != Model.DEFAULT_DRIVE_ERROR_RECOVERY_TIMEOUT)
			{
				Logger.Info("Setting reader error recovery timeout on drive {0}", new object[]
				{
					d.GetNumericId()
				});
				storApiStatus = d.SetErrorRecoveryTime(true, Model.DEFAULT_DRIVE_ERROR_RECOVERY_TIMEOUT);
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					Logger.Warn("Failed to set drive {1} reader error timeout: {0}", new object[]
					{
						storApiStatus,
						d.GetNumericId()
					});
				}
			}
			num = 0;
			storApiStatus = d.GetErrorRecoveryTime(false, ref num);
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Logger.Warn("Failed to get drive {1} writer error timeout: {0}", new object[]
				{
					storApiStatus,
					d.GetNumericId()
				});
				return;
			}
			if (num != Model.DEFAULT_DRIVE_ERROR_RECOVERY_TIMEOUT)
			{
				Logger.Info("Setting writer error recovery timeout on drive {0}", new object[]
				{
					d.GetNumericId()
				});
				storApiStatus = d.SetErrorRecoveryTime(false, Model.DEFAULT_DRIVE_ERROR_RECOVERY_TIMEOUT);
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					Logger.Warn("Failed to set drive {1} writer error timeout: {0}", new object[]
					{
						storApiStatus,
						d.GetNumericId()
					});
				}
			}
		}

		protected virtual void HandleNewDrive(Drive drive)
		{
			StorApiStatus a = StorApiStatusEnum.STOR_NO_ERROR;
			bool flag = false;
			a = drive.IsWriteCacheOn(ref flag);
			if (a == StorApiStatusEnum.STOR_NO_ERROR && !flag)
			{
				Logger.Info("Setting drive {0} {1} cache", new object[]
				{
					drive.Serial,
					drive.Id
				});
				drive.SetWriteCache(true);
			}
		}

		public bool ConsentDriveInBay(int baynum)
		{
			bool result;
			lock (this.modelLock)
			{
				DriveBay driveBay = this.FindDriveBayByNumber(baynum);
				if (driveBay != null)
				{
					Drive driveFromBay = this.GetDriveFromBay(driveBay);
					if (driveFromBay != null)
					{
						Logger.Info("ConsentDrive category={0} bay={1} status={2}", new object[]
						{
							driveFromBay.Category.ToString(),
							driveBay.Number,
							driveBay.Status.ToString()
						});
						if (driveFromBay.Category == DriveCategory.DRIVE_CATEGORY_UNKNOWN && driveBay.Status == DriveBayStatusEnum.DRIVE_BAY_INVALID_WHITELIST)
						{
							if (this.persistentDriveInfoList != null)
							{
								PersistentDriveInfo persistentDriveInfo = this.persistentDriveInfoList.Find(driveFromBay.Serial);
								if (persistentDriveInfo != null)
								{
									persistentDriveInfo.unsupportedConsent = true;
									this.persistentDriveInfoList.Save();
								}
							}
							if (!driveBay.IsBoot)
							{
								Logger.Info("Turn on drive {0}", new object[]
								{
									driveFromBay.Id
								});
								driveBay.SetPower(true);
							}
							return true;
						}
					}
					else
					{
						Logger.Warn("Cannot find drive for bay {0}", new object[]
						{
							baynum
						});
					}
				}
				else
				{
					Logger.Warn("Cannot find bay number {0}", new object[]
					{
						baynum
					});
				}
				result = false;
			}
			return result;
		}

		public bool IsDriveConsented(Drive drive)
		{
			bool result;
			lock (this.modelLock)
			{
				if (drive != null && this.persistentDriveInfoList != null)
				{
					PersistentDriveInfo persistentDriveInfo = this.persistentDriveInfoList.Find(drive.Serial);
					if (persistentDriveInfo != null)
					{
						return persistentDriveInfo.unsupportedConsent;
					}
				}
				result = false;
			}
			return result;
		}

		public bool IsDriveJustInserted(Drive d)
		{
			lock (this.modelLock)
			{
				if (this.persistentDriveInfoList != null)
				{
					PersistentDriveInfo persistentDriveInfo = this.persistentDriveInfoList.Find(d.Serial);
					if (persistentDriveInfo != null)
					{
						return persistentDriveInfo.IsJustInserted;
					}
				}
			}
			return true;
		}

		public bool IsDriveNew(Drive d)
		{
			lock (this.modelLock)
			{
				if (this.persistentDriveInfoList != null)
				{
					PersistentDriveInfo persistentDriveInfo = this.persistentDriveInfoList.Find(d.Serial);
					if (persistentDriveInfo != null)
					{
						return persistentDriveInfo.IsRecent;
					}
				}
			}
			return true;
		}

		protected virtual Drive FindDriveForVolumeRebuild(Volume volume)
		{
			if (volume == null)
			{
				return null;
			}
			List<DriveBay> list;
			if (volume.IsSystem)
			{
				list = this.GetBootBays();
			}
			else
			{
				list = this.GetDataBays();
			}
			foreach (DriveBay driveBay in list)
			{
				if (driveBay.Status == DriveBayStatusEnum.DRIVE_BAY_NONE)
				{
					Drive drive = this.FindDriveFromDriveBay(driveBay);
					if (drive != null && drive.Status == DriveStatus.DRIVE_GOOD)
					{
						List<Volume> list2 = new List<Volume>();
						volume.Controller.GetDriveVolumes(drive, ref list2);
						if (list2.Count == 0 && drive.Capacity >= volume.GetMinDiskSize())
						{
							return drive;
						}
					}
				}
			}
			return null;
		}

		protected virtual Drive FindDriveForVolumeMigrate(Volume volume)
		{
			return this.FindDriveForVolumeRebuild(volume);
		}

		protected virtual void CheckVolumeOperations()
		{
			foreach (Volume volume in this.Volumes)
			{
				this.CheckVolumeOperation(volume);
			}
			this.needsVerify = false;
		}

		protected virtual void CheckVolumeOperation(Volume volume)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			if (volume.Status == VolumeStatus.VOLUME_DEGRADED)
			{
				if (volume.IsSystem)
				{
					Drive drive = this.FindDriveForVolumeRebuild(volume);
					if (drive != null)
					{
						List<Drive> list = new List<Drive>();
						list.Add(drive);
						Logger.Info("Start rebuild on volume {0} using drive {1}", new object[]
						{
							volume.Id,
							drive.GetNumericId()
						});
						storApiStatus = volume.Rebuild(list);
						if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
						{
							Logger.Error("Failed to start volume rebuild for {0} on drive {1}: {2}", new object[]
							{
								volume.Id,
								drive.GetNumericId(),
								storApiStatus
							});
						}
					}
					else
					{
						this.ClearInvalidVolumesOnBootDrives(volume);
					}
				}
			}
			else if (volume.Status == VolumeStatus.VOLUME_NORMAL && volume.IsSystem && volume.RaidLevel == RaidLevel.RAID_0 && volume.Drives.Count == 1)
			{
				Drive drive2 = this.FindDriveForVolumeRebuild(volume);
				if (drive2 != null)
				{
					if (drive2.Port.Root != 7)
					{
						if (!this.sendMigrateWarning)
						{
							Logger.Warn("Failed to start migration because new boot drive is on wrong port: {0} port={1}", new object[]
							{
								drive2.Id,
								drive2.Port.ToString()
							});
							this.sendMigrateWarning = true;
						}
					}
					else if (!drive2.FailedMigrate)
					{
						List<Drive> list2 = new List<Drive>();
						list2.Add(drive2);
						Logger.Info("Start migrate on volume {0} using drive {1}", new object[]
						{
							volume.Id,
							drive2.GetNumericId()
						});
						storApiStatus = volume.Migrate(RaidLevel.RAID_1, list2);
						if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
						{
							Logger.Error("Failed to start volume migrate for {0} on drive {1}: {2}", new object[]
							{
								volume.Id,
								drive2.GetNumericId(),
								storApiStatus
							});
							drive2.FailedMigrate = true;
						}
					}
				}
			}
			if (this.needsVerify && volume.IsSystem && volume.Status == VolumeStatus.VOLUME_NORMAL)
			{
				Logger.Info("Start verify on volume {0} due to unclean shutdown", new object[]
				{
					volume.Id
				});
				storApiStatus = volume.Verify(true);
				if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
				{
					Logger.Error("Failed to start volume verify for {0}: {1}", new object[]
					{
						volume.Id,
						storApiStatus
					});
					return;
				}
				this.needsVerify = false;
			}
		}

		protected virtual Volume GetSystemVolume()
		{
			foreach (Volume volume in this.Volumes)
			{
				if (volume.IsSystem)
				{
					return volume;
				}
			}
			return null;
		}

		protected virtual List<DriveBay> GetBootBays()
		{
			List<DriveBay> list = new List<DriveBay>();
			foreach (DriveBay driveBay in this.driveBays)
			{
				if (driveBay.IsBoot)
				{
					list.Add(driveBay);
				}
			}
			return list;
		}

		protected virtual List<DriveBay> GetDataBays()
		{
			List<DriveBay> list = new List<DriveBay>();
			foreach (DriveBay driveBay in this.driveBays)
			{
				if (!driveBay.IsBoot)
				{
					list.Add(driveBay);
				}
			}
			return list;
		}

		protected virtual void ClearInvalidVolumesOnBootDrives(Volume sysvol)
		{
			foreach (Volume volume in this.Volumes)
			{
				if (volume != sysvol && (volume.Status == VolumeStatus.VOLUME_FAILED || volume.Status == VolumeStatus.VOLUME_DEGRADED))
				{
					bool flag = true;
					foreach (Drive d in volume.Drives)
					{
						DriveBay driveBay = this.FindDriveBayFromDrive(d);
						if (!driveBay.IsBoot)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						StorApiStatus storApiStatus = volume.Controller.DeleteVolume(volume.Id);
						if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
						{
							Logger.Warn("Unable to delete volume {0}: {1}", new object[]
							{
								volume.Id,
								storApiStatus
							});
						}
					}
				}
			}
		}

		protected virtual void GenerateDriveStatusEvent(ModelDriveEvent evt)
		{
			EventHandler<ModelDriveEvent> onDriveEvent = this.OnDriveEvent;
			if (onDriveEvent != null)
			{
				onDriveEvent(this, evt);
			}
		}

		protected virtual void GenerateVolumeStatusEvent(ModelVolumeEvent evt)
		{
			EventHandler<ModelVolumeEvent> onVolumeEvent = this.OnVolumeEvent;
			if (onVolumeEvent != null)
			{
				onVolumeEvent(this, evt);
			}
		}

		protected virtual void GenerateUpdateCompleteEvent()
		{
			EventHandler<ModelUpdateCompleteEvent> onUpdateCompleteEvent = this.OnUpdateCompleteEvent;
			if (onUpdateCompleteEvent != null)
			{
				ModelUpdateCompleteEvent e = new ModelUpdateCompleteEvent();
				onUpdateCompleteEvent(this, e);
			}
		}

		public virtual string GetDebug()
		{
			string text = "";
			text += "Model: \n";
			text += string.Format("NeedsVerify={0}\n", this.needsVerify);
			if (this.controllers != null)
			{
				text += string.Format("Controllers={0}\n", this.controllers.Count);
				foreach (Controller controller in this.controllers)
				{
					foreach (Drive drive in controller.Drives)
					{
						text += string.Format("Drive: {0}\n", drive.Id);
						text += string.Format(" Serial: {0}\n", drive.Serial);
						text += string.Format(" Status: {0}\n", drive.Status.ToString());
						text += string.Format(" Model: {0}\n", drive.Model);
						text += string.Format(" IsSystem: {0}\n", drive.IsSystem);
						text += string.Format(" Port: {0}\n", drive.Port.ToString());
						text += string.Format(" Domain: {0}\n", drive.Domain);
					}
					foreach (Volume volume in controller.Volumes)
					{
						text += string.Format("Volume: {0}\n", volume.Id);
						text += string.Format(" Name: {0}\n", volume.Name);
						text += string.Format(" Status: {0}\n", volume.Status.ToString());
						text += string.Format(" Raid: {0}\n", volume.RaidLevel);
						text += string.Format(" IsSystem: {0}\n", volume.IsSystem);
						text += string.Format(" Progress: {0}\n", volume.Progress.ToString());
					}
				}
			}
			if (this.driveBays != null)
			{
				text += string.Format("Bays={0}\n", this.driveBays.Count);
				foreach (DriveBay driveBay in this.driveBays)
				{
					text = text + "Bay: " + driveBay.ToString() + "\n";
				}
			}
			if (this.spacesControllers != null)
			{
				text += "Spaces:\n";
				text += string.Format(" Controllers: {0}\n", this.spacesControllers.Count);
				foreach (Controller controller2 in this.spacesControllers)
				{
					SpacesController spacesController = (SpacesController)controller2;
					foreach (Drive drive2 in spacesController.Drives)
					{
						SpacesDrive spacesDrive = (SpacesDrive)drive2;
						text += string.Format(" Drive: {0}\n", spacesDrive.Id);
						text += string.Format("  Serial: {0}\n", spacesDrive.Serial);
						text += string.Format("  Model: {0}\n", spacesDrive.Model);
						text += string.Format("  Health: {0}\n", spacesDrive.Health.ToString());
						text += string.Format("  OperationalStatus: {0}\n", string.Join<OperationalStatusEnum>(",", spacesDrive.OperationalStatuses));
					}
					foreach (Volume volume2 in spacesController.Volumes)
					{
						SpacesVolume spacesVolume = (SpacesVolume)volume2;
						text += string.Format(" Volume: {0}\n", spacesVolume.Id);
						text += string.Format("  Name: {0}\n", spacesVolume.Name);
						text += string.Format("  Raid: {0}\n", spacesVolume.RaidLevel);
						text += string.Format("  Health: {0}\n", spacesVolume.Health.ToString());
						text += string.Format("  OperationalStatus: {0}\n", string.Join<OperationalStatusEnum>(",", spacesVolume.OperationalStatuses));
					}
					foreach (SpacesPool spacesPool in spacesController.Pools)
					{
						text += string.Format(" Pool: {0}\n", spacesPool.Id);
						text += string.Format("  Name: {0}\n", spacesPool.Name);
						text += string.Format("  Primordial: {0}\n", spacesPool.IsPrimordial);
						text += string.Format("  Health: {0}\n", spacesPool.Health.ToString());
						text += string.Format("  OperationalStatus: {0}\n", string.Join<OperationalStatusEnum>(",", spacesPool.OperationalStatuses));
					}
				}
			}
			return text;
		}

		protected void UpdateDriveStatus(Drive d)
		{
			StorApiStatus a = StorApiStatusEnum.STOR_NO_ERROR;
			if (d is MarvellDrive)
			{
				d.Status = DriveStatus.DRIVE_GOOD;
				if (d.IsSmartEnabled)
				{
					SmartPrediction smartPrediction = new SmartPrediction();
					a = d.GetSmartPrediction(ref smartPrediction);
					if (a == StorApiStatusEnum.STOR_NO_ERROR && smartPrediction.predictFailure)
					{
						d.Status = DriveStatus.DRIVE_IMMINENT_FAILURE;
					}
					SmartInfo smartInfo = new SmartInfo();
					a = d.GetSmart(ref smartInfo);
					if (a == StorApiStatusEnum.STOR_NO_ERROR)
					{
						d.Temperature = d.GetTemperatureFromSMARTData(smartInfo);
						Range driveTemperatureRange = SystemConfig.GetDriveTemperatureRange(d.Model, "Warning");
						if (driveTemperatureRange != null && d.Temperature >= driveTemperatureRange.Min)
						{
							d.Status = DriveStatus.DRIVE_OVERTEMP;
						}
					}
				}
				if (d.Status == DriveStatus.DRIVE_GOOD)
				{
					SpacesController spacesController = SpacesController.GetSpacesController();
					SpacesDrive spacesDrive = null;
					a = spacesController.GetSpacesDrive(d.Serial, ref spacesDrive);
					if (a == StorApiStatusEnum.STOR_NO_ERROR && spacesDrive != null && spacesDrive.Status != DriveStatus.DRIVE_UNKNOWN)
					{
						d.Status = spacesDrive.Status;
					}
				}
			}
		}

		protected void UpdateAllDriveStatus()
		{
			foreach (Controller controller in this.controllers)
			{
				if (controller is MarvellController)
				{
					foreach (Drive drive in controller.Drives)
					{
						Drive drive2 = (MarvellDrive)drive.Clone();
						this.UpdateDriveStatus(drive);
						if (drive2.Status != drive.Status || drive2.Temperature != drive.Temperature)
						{
							this.OnDriveChanged(this, new DriveEvent
							{
								controller = controller,
								drive = drive,
								oldDrive = drive2,
								type = ControllerEventType.EVENT_TYPE_CHANGED
							});
						}
					}
				}
			}
		}

		protected void DriveStatusMonitor()
		{
			while (!this.driveStatusMonitorEvent.WaitOne(60000))
			{
				lock (this.modelLock)
				{
					this.UpdateAllDriveStatus();
				}
			}
		}

		public static ushort DEFAULT_DRIVE_ERROR_RECOVERY_TIMEOUT = 70;

		protected DriveBayMonitor driveBayMonitor;

		protected object modelLock;

		protected List<Controller> controllers;

		protected ControllerMonitor monitor;

		protected List<Controller> spacesControllers;

		protected SpacesControllerMonitor spacesMonitor;

		protected List<DriveBay> driveBays;

		protected bool needsVerify;

		protected bool sendMigrateWarning;

		protected Thread driveStatusMonitor;

		protected AutoResetEvent driveStatusMonitorEvent;

		protected PersistentDriveInfoList persistentDriveInfoList = new PersistentDriveInfoList();
	}
}
