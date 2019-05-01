using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stor
{
	public class MarvellVolume : Volume
	{
		public MarvellVolume(short id, MarvellController c) : base(id.ToString(), c)
		{
			this.mvid = id;
			this.blockIds = new List<short>();
		}

		public short MvId
		{
			get
			{
				return this.mvid;
			}
		}

		public List<short> BlockIds
		{
			get
			{
				return this.blockIds;
			}
			set
			{
				this.blockIds = value;
			}
		}

		public override StorApiStatus Verify(bool fixError)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			byte type = 0;
			if (fixError)
			{
				type = 1;
			}
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_LD_StartConsistencyCheck(((MarvellController)this.controller).AdapterId, this.mvid, type);
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_LD_StartConsistencyCheck exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			if (b != 0)
			{
				storApiStatus = MarvellUtil.ToStorApiStatus(b);
				storApiStatus.internalIntData = (int)b;
			}
			return storApiStatus;
		}

		public override StorApiStatus Rebuild(List<Drive> newDrives)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			if (newDrives == null || newDrives.Count == 0)
			{
				return StorApiStatusEnum.STOR_INVALID_PARAM;
			}
			if (!(newDrives[0] is MarvellDrive))
			{
				return StorApiStatusEnum.STOR_INVALID_PARAM;
			}
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_LD_StartRebuild(((MarvellController)this.controller).AdapterId, this.mvid, ((MarvellDrive)newDrives[0]).MvId);
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_LD_StartRebuild exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			if (b != 0)
			{
				storApiStatus = MarvellUtil.ToStorApiStatus(b);
				storApiStatus.internalIntData = (int)b;
			}
			return storApiStatus;
		}

		public override StorApiStatus Migrate(RaidLevel newLevel, List<Drive> newDrives)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			MvApi.MvApi.Create_LD_Param create_LD_Param = default(MvApi.MvApi.Create_LD_Param);
			MvApi.MvApi.LD_Config_Request ld_Config_Request = default(MvApi.MvApi.LD_Config_Request);
			int num = 0;
			ld_Config_Request.header.Init();
			ld_Config_Request.header.requestType = 2;
			ld_Config_Request.header.startingIndexOrId = this.mvid;
			ld_Config_Request.header.numRequested = 1;
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_LD_GetConfig(((MarvellController)this.controller).AdapterId, ref ld_Config_Request);
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_LD_GetConfig exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
				if (b != 0)
				{
					storApiStatus = MarvellUtil.ToStorApiStatus(b);
					storApiStatus.internalIntData = (int)b;
					Logger.Warn("MV_LD_GetConfig failed: {0}", new object[]
					{
						b
					});
					return storApiStatus;
				}
				string @string = Encoding.ASCII.GetString(ld_Config_Request.ldConfig[0].Name);
				create_LD_Param.LDID = this.MvId;
				create_LD_Param.RaidMode = MarvellUtil.ToMarvellRaidLevel(newLevel);
				create_LD_Param.RoundingScheme = 0;
				create_LD_Param.InitializationOption = 0;
				create_LD_Param.HDIDs = new short[128];
				foreach (Drive drive in this.drives)
				{
					if (num >= 128)
					{
						storApiStatus = StorApiStatusEnum.STOR_INVALID_PARAM;
						return storApiStatus;
					}
					create_LD_Param.HDIDs[num] = ((MarvellDrive)drive).MvId;
					num++;
				}
				foreach (Drive drive2 in newDrives)
				{
					if (num >= 128)
					{
						storApiStatus = StorApiStatusEnum.STOR_INVALID_PARAM;
						return storApiStatus;
					}
					create_LD_Param.HDIDs[num] = ((MarvellDrive)drive2).MvId;
					num++;
				}
				create_LD_Param.HDCount = (byte)num;
				create_LD_Param.Name = new byte[16];
				ld_Config_Request.ldConfig[0].Name.CopyTo(create_LD_Param.Name, 0);
				Logger.Debug("Migrate parameter: LD={0}, RAID={1}, HDCount={2}, HD={3}, Name={4}", new object[]
				{
					create_LD_Param.LDID,
					create_LD_Param.RaidMode,
					create_LD_Param.HDCount,
					string.Join<short>(" ", create_LD_Param.HDIDs.ToList<short>().GetRange(0, (int)create_LD_Param.HDCount)),
					@string
				});
				try
				{
					b = MvApi.MvApi.MV_LD_StartMigration(((MarvellController)this.controller).AdapterId, ref create_LD_Param);
				}
				catch (Exception ex2)
				{
					Logger.Warn("MV_LD_StartMigration exception: {0}", new object[]
					{
						ex2
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
				if (b != 0)
				{
					storApiStatus = MarvellUtil.ToStorApiStatus(b);
					storApiStatus.internalIntData = (int)b;
					Logger.Warn("MV_LD_StartMigration failed: {0}", new object[]
					{
						b
					});
				}
			}
			return storApiStatus;
		}

		public override StorApiStatus Initialize(VolumeInitializeType type)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			byte init_Type = 3;
			switch (type)
			{
			case VolumeInitializeType.INIT_QUICK:
				init_Type = 0;
				break;
			case VolumeInitializeType.INIT_FULLFG:
				init_Type = 1;
				break;
			case VolumeInitializeType.INIT_FULLBG:
				init_Type = 2;
				break;
			}
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_LD_StartINIT(((MarvellController)this.controller).AdapterId, this.mvid, init_Type);
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_LD_StartINIT exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			if (b != 0)
			{
				storApiStatus = MarvellUtil.ToStorApiStatus(b);
				storApiStatus.internalIntData = (int)b;
			}
			return storApiStatus;
		}

		protected short mvid;

		protected List<short> blockIds;
	}
}
