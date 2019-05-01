using System;

namespace Stor
{
	public class MarvellDrive : Drive
	{
		public MarvellDrive(short id, MarvellController c) : base(id.ToString(), c)
		{
			this.mvid = id;
		}

		public override StorApiStatus ATACmd(ref ATACmdInfo ata)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			MvApi.MvApi.ATA_REGS ata_REGS = default(MvApi.MvApi.ATA_REGS);
			ata_REGS.Init();
			Array.Copy(ata.registers, ata_REGS.drive_regs, ata_REGS.drive_regs.Length);
			if (ata.datalen > 0u)
			{
				if (ata.data == null || (long)ata.data.Length != (long)((ulong)ata.datalen))
				{
					return StorApiStatusEnum.STOR_INVALID_PARAM;
				}
				ata_REGS.buffer_size = (short)ata.datalen;
			}
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_PassThrough_ATA_Helper(((MarvellController)this.controller).AdapterId, this.mvid, ref ata_REGS, ref ata.data);
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_PassThrough_ATA_Helper exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			if (b == 0)
			{
				Array.Copy(ata_REGS.drive_regs, ata.registers, ata.registers.Length);
			}
			return MarvellUtil.ToStorApiStatus(b);
		}

		public override StorApiStatus GetSmart(ref SmartInfo smart)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			MvApi.MvApi.SMART_Info_Request smart_Info_Request = default(MvApi.MvApi.SMART_Info_Request);
			smart_Info_Request.header.Init();
			smart_Info_Request.header.requestType = 1;
			smart_Info_Request.header.startingIndexOrId = 0;
			smart_Info_Request.header.numRequested = 30;
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_PD_GET_SMART_INFO(((MarvellController)this.controller).AdapterId, this.mvid, ref smart_Info_Request);
				}
				catch (Exception ex)
				{
					Logger.Warn("MV_PD_GET_SMART_INFO exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			if (b == 0)
			{
				if (smart == null)
				{
					smart = new SmartInfo();
				}
				smart.attributes = new SmartAttribute[(int)smart_Info_Request.header.numReturned];
				smart.thresholds = new SmartThreshold[(int)smart_Info_Request.header.numReturned];
				for (short num = 0; num < smart_Info_Request.header.numReturned; num += 1)
				{
					SmartAttribute smartAttribute = new SmartAttribute();
					SmartThreshold smartThreshold = new SmartThreshold();
					smartAttribute.id = smart_Info_Request.SmartInfo[(int)num].Id;
					smartAttribute.flags = (ushort)((int)smart_Info_Request.SmartInfo[(int)num].StatusFlags1 << 8 | (int)smart_Info_Request.SmartInfo[(int)num].StatusFlags2);
					smartAttribute.current = smart_Info_Request.SmartInfo[(int)num].CurrentValue;
					smartAttribute.worst = smart_Info_Request.SmartInfo[(int)num].WorstValue;
					Array.Copy(smart_Info_Request.SmartInfo[(int)num].RawValue, smartAttribute.raw, smartAttribute.raw.Length);
					smartThreshold.id = smart_Info_Request.SmartInfo[(int)num].Id;
					smartThreshold.value = smart_Info_Request.SmartInfo[(int)num].ThresholdValue;
					smart.attributes[(int)num] = smartAttribute;
					smart.thresholds[(int)num] = smartThreshold;
				}
			}
			return MarvellUtil.ToStorApiStatus(b);
		}

		public short MvId
		{
			get
			{
				return this.mvid;
			}
		}

		public override StorApiStatus IsWriteCacheOn(ref bool on)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			MvApi.MvApi.HD_Config_Request hd_Config_Request = default(MvApi.MvApi.HD_Config_Request);
			hd_Config_Request.header.Init();
			hd_Config_Request.header.requestType = 2;
			hd_Config_Request.header.startingIndexOrId = this.mvid;
			hd_Config_Request.header.numRequested = 1;
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_PD_GetConfig(((MarvellController)this.controller).AdapterId, ref hd_Config_Request);
				}
				catch (Exception ex)
				{
					Logger.Warn("IsWriteCacheOn MV_PD_GetConfig exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			if (b == 0)
			{
				if (hd_Config_Request.header.numReturned != 1)
				{
					b = 55;
				}
				else
				{
					on = (hd_Config_Request.hdConfig[0].WriteCacheOn != 0);
				}
			}
			return MarvellUtil.ToStorApiStatus(b);
		}

		public override StorApiStatus SetWriteCache(bool on)
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			byte b = 0;
			MvApi.MvApi.HD_Config_Request hd_Config_Request = default(MvApi.MvApi.HD_Config_Request);
			hd_Config_Request.header.Init();
			hd_Config_Request.header.requestType = 2;
			hd_Config_Request.header.startingIndexOrId = this.mvid;
			hd_Config_Request.header.numRequested = 1;
			lock (MarvellUtil.mvApiLock)
			{
				try
				{
					b = MvApi.MvApi.MV_PD_GetConfig(((MarvellController)this.controller).AdapterId, ref hd_Config_Request);
				}
				catch (Exception ex)
				{
					Logger.Warn("SetWriteCache MV_PD_GetConfig exception: {0}", new object[]
					{
						ex
					});
					return StorApiStatusEnum.STOR_API_ERROR;
				}
			}
			if (b == 0)
			{
				if (hd_Config_Request.header.numReturned != 1)
				{
					b = 55;
				}
				else
				{
					bool writeCacheOn = hd_Config_Request.hdConfig[0].WriteCacheOn != 0;
					if (writeCacheOn != on)
					{
						hd_Config_Request.hdConfig[0].WriteCacheOn = (byte)((!on) ? 0 : 1);
						IntPtr intPtr = StorHelper.AllocateIntPtr<MvApi.MvApi.HD_Config>(hd_Config_Request.hdConfig[0]);
						if (intPtr != IntPtr.Zero)
						{
							lock (MarvellUtil.mvApiLock)
							{
								try
								{
									b = MvApi.MvApi.MV_PD_SetConfig(((MarvellController)this.controller).AdapterId, this.mvid, intPtr);
								}
								catch (Exception ex2)
								{
									StorHelper.FreeIntPtr(intPtr);
									Logger.Warn("MV_PD_SetConfig exception: {0}", new object[]
									{
										ex2
									});
									return StorApiStatusEnum.STOR_API_ERROR;
								}
							}
							StorHelper.FreeIntPtr(intPtr);
						}
					}
				}
			}
			return MarvellUtil.ToStorApiStatus(b);
		}

		protected short mvid;
	}
}
