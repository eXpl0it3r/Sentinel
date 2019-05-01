using System;
using System.Collections.Generic;

namespace Stor
{
	public class Storage
	{
		public static void SetDebug(bool on)
		{
			Storage.DebugOn = on;
			MvApi.MvApi.SetDebug(on);
			SpacesApi.SpacesApi.SetDebug(on);
		}

		public static void Debug(string fmt, params object[] args)
		{
			if (Storage.DebugOn)
			{
				Console.WriteLine(fmt, args);
			}
		}

		public static StorApiStatus Initialize()
		{
			StorApiStatus storApiStatus = StorApiStatusEnum.STOR_NO_ERROR;
			if (Storage.initialized)
			{
				return storApiStatus;
			}
			storApiStatus = MarvellUtil.Initialize();
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Storage.Debug("Marvell initialization failed: {0}", new object[]
				{
					storApiStatus
				});
			}
			storApiStatus = SpacesUtil.Initialize();
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Storage.Debug("Storage Spaces initialization failed: {0}", new object[]
				{
					storApiStatus
				});
			}
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public static StorApiStatus Finalize()
		{
			StorApiStatus storApiStatus = MarvellUtil.Finalize();
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Storage.Debug("Marvell finalize failed: {0}", new object[]
				{
					storApiStatus
				});
			}
			storApiStatus = SpacesUtil.Finalize();
			if (storApiStatus != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Storage.Debug("Storage Spaces finalize failed: {0}", new object[]
				{
					storApiStatus
				});
			}
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public static StorApiStatus GetControllers(ref List<Controller> controllers)
		{
			StorApiStatus controllers2 = MarvellController.GetControllers(ref controllers);
			if (controllers2 != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Storage.Debug("Marvell get controllers failed: {0}", new object[]
				{
					controllers2
				});
			}
			controllers2 = SpacesController.GetControllers(ref controllers);
			if (controllers2 != StorApiStatusEnum.STOR_NO_ERROR)
			{
				Storage.Debug("Storage Spaces get controllers failed: {0}", new object[]
				{
					controllers2
				});
			}
			return StorApiStatusEnum.STOR_NO_ERROR;
		}

		public static bool DebugOn = false;

		public static int ATA_PASSWORD_MAX_SIZE_NET = 16;

		public static bool initialized = false;
	}
}
