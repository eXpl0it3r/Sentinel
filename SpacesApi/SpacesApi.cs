using System;
using System.Collections.Generic;
using System.Management;
using System.Text.RegularExpressions;

namespace SpacesApi
{
	public class SpacesApi
	{
		public static void SetDebug(bool on)
		{
			SpacesApi.DebugOn = on;
		}

		public static void Debug(string fmt, params object[] args)
		{
			if (SpacesApi.DebugOn)
			{
				Console.WriteLine(fmt, args);
			}
		}

		public static SpacesApiError Initialize()
		{
			return SpacesApi.ConnectToStorageScope();
		}

		public static SpacesApiError Finalize()
		{
			return SpacesApiError.Success;
		}

		public static SpacesApiError ConvertError(object status)
		{
			SpacesApiError result = SpacesApiError.Unknown;
			try
			{
				result = (SpacesApiError)Enum.ToObject(typeof(SpacesApiError), status);
			}
			catch (Exception)
			{
				result = SpacesApiError.Unknown;
			}
			return result;
		}

		protected static SpacesApiError ConnectToStorageScope()
		{
			SpacesApiError result = SpacesApiError.Success;
			try
			{
				SpacesApi.scope = new ManagementScope(SpacesApi.StorageNamespace);
				SpacesApi.scope.Connect();
			}
			catch (Exception ex)
			{
				result = SpacesApiError.Failed;
				SpacesApi.scope = null;
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to connect to {0}: {1}", new object[]
					{
						SpacesApi.StorageNamespace,
						ex.ToString()
					});
				}
				throw ex;
			}
			return result;
		}

		protected static ManagementScope GetStorageScope()
		{
			if (SpacesApi.scope != null && SpacesApi.scope.IsConnected)
			{
				return SpacesApi.scope;
			}
			if (SpacesApi.ConnectToStorageScope() == SpacesApiError.Success)
			{
				return SpacesApi.scope;
			}
			return null;
		}

		protected static SpacesApiError GetWin32ObjectsByQuery(string query, ref ManagementObjectCollection collection)
		{
			SpacesApiError result = SpacesApiError.Success;
			ObjectQuery query2 = new ObjectQuery(query);
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(query2);
			collection = managementObjectSearcher.Get();
			return result;
		}

		public static SpacesApiError GetStorageObjectsByQuery(string query, ref ManagementObjectCollection collection)
		{
			SpacesApiError result = SpacesApiError.Success;
			ManagementScope storageScope = SpacesApi.GetStorageScope();
			if (storageScope == null)
			{
				result = SpacesApiError.Failed;
			}
			else
			{
				ObjectQuery query2 = new ObjectQuery(query);
				ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(storageScope, query2);
				collection = managementObjectSearcher.Get();
			}
			return result;
		}

		public static SpacesApiError GetStorageEventWatcher(string query, ref ManagementEventWatcher watcher)
		{
			SpacesApiError result = SpacesApiError.Success;
			if (SpacesApi.GetStorageScope() == null)
			{
				result = SpacesApiError.Failed;
			}
			else
			{
				watcher = new ManagementEventWatcher(query);
			}
			return result;
		}

		public static SpacesApiError GetPhysicalDisk(string serial, ref PhysicalDisk disk)
		{
			SpacesApiError spacesApiError = SpacesApiError.Success;
			try
			{
				ManagementObjectCollection managementObjectCollection = null;
				disk = null;
				string query = string.Format("Select * From MSFT_PhysicalDisk Where SerialNumber like '%{0}'", serial);
				spacesApiError = SpacesApi.GetStorageObjectsByQuery(query, ref managementObjectCollection);
				if (spacesApiError != SpacesApiError.Success)
				{
					return spacesApiError;
				}
				foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
				{
					ManagementObject m = (ManagementObject)managementBaseObject;
					string b = SpacesApiUtil.GetManagementObjectValue<string>(m, "SerialNumber").Trim();
					if (serial == b)
					{
						disk = new PhysicalDisk();
						disk.FromManagementObject(m);
						break;
					}
				}
			}
			catch (Exception ex)
			{
				spacesApiError = SpacesApiError.Failed;
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to get physical disk: {0}", new object[]
					{
						ex
					});
				}
			}
			return spacesApiError;
		}

		public static SpacesApiError GetPhysicalDisks(ref List<PhysicalDisk> disks)
		{
			SpacesApiError spacesApiError = SpacesApiError.Success;
			try
			{
				ManagementObjectCollection managementObjectCollection = null;
				spacesApiError = SpacesApi.GetStorageObjectsByQuery("Select * From MSFT_PhysicalDisk", ref managementObjectCollection);
				if (spacesApiError != SpacesApiError.Success)
				{
					return spacesApiError;
				}
				foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
				{
					ManagementObject m = (ManagementObject)managementBaseObject;
					PhysicalDisk physicalDisk = new PhysicalDisk();
					physicalDisk.FromManagementObject(m);
					disks.Add(physicalDisk);
				}
			}
			catch (Exception ex)
			{
				spacesApiError = SpacesApiError.Failed;
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to get physical disks: {0}", new object[]
					{
						ex
					});
				}
			}
			return spacesApiError;
		}

		public static SpacesApiError GetVirtualDisks(ref List<VirtualDisk> disks)
		{
			SpacesApiError spacesApiError = SpacesApiError.Success;
			try
			{
				ManagementObjectCollection managementObjectCollection = null;
				spacesApiError = SpacesApi.GetStorageObjectsByQuery("Select * From MSFT_VirtualDisk", ref managementObjectCollection);
				if (spacesApiError != SpacesApiError.Success)
				{
					return spacesApiError;
				}
				foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
				{
					ManagementObject m = (ManagementObject)managementBaseObject;
					VirtualDisk virtualDisk = new VirtualDisk();
					virtualDisk.FromManagementObject(m);
					disks.Add(virtualDisk);
				}
			}
			catch (Exception ex)
			{
				spacesApiError = SpacesApiError.Failed;
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to get virtual disks: {0}", new object[]
					{
						ex
					});
				}
			}
			return spacesApiError;
		}

		public static SpacesApiError GetStoragePools(ref List<StoragePool> pools)
		{
			SpacesApiError spacesApiError = SpacesApiError.Success;
			try
			{
				ManagementObjectCollection managementObjectCollection = null;
				spacesApiError = SpacesApi.GetStorageObjectsByQuery("Select * From MSFT_StoragePool", ref managementObjectCollection);
				if (spacesApiError != SpacesApiError.Success)
				{
					return spacesApiError;
				}
				foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
				{
					ManagementObject m = (ManagementObject)managementBaseObject;
					StoragePool storagePool = new StoragePool();
					storagePool.FromManagementObject(m);
					pools.Add(storagePool);
				}
			}
			catch (Exception ex)
			{
				spacesApiError = SpacesApiError.Failed;
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to get storage pools: {0}", new object[]
					{
						ex
					});
				}
			}
			return spacesApiError;
		}

		public static SpacesApiError GetPhysicalDisksForVirtualDisk(string vdid, ref List<PhysicalDisk> disks)
		{
			SpacesApiError spacesApiError = SpacesApiError.Success;
			try
			{
				Regex regex = new Regex("(.+)MSFT_VirtualDisk.ObjectId=\"([{}0-9a-zA-Z-]+)\"");
				Regex regex2 = new Regex("(.+)MSFT_PhysicalDisk.ObjectId=\"([{}0-9a-zA-Z-]+)\"");
				ManagementObjectCollection managementObjectCollection = null;
				spacesApiError = SpacesApi.GetStorageObjectsByQuery("Select * from MSFT_VirtualDiskToPhysicalDisk", ref managementObjectCollection);
				if (spacesApiError != SpacesApiError.Success)
				{
					return spacesApiError;
				}
				foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
				{
					ManagementObject managementObject = (ManagementObject)managementBaseObject;
					string text = null;
					string text2 = null;
					if (managementObject["VirtualDisk"] != null)
					{
						Match match = regex.Match((string)managementObject["VirtualDisk"]);
						if (match.Success && match.Groups.Count > 2)
						{
							text = match.Groups[2].Value;
							if (text != vdid)
							{
								continue;
							}
						}
					}
					if (text != null && managementObject["PhysicalDisk"] != null)
					{
						Match match2 = regex2.Match((string)managementObject["PhysicalDisk"]);
						if (match2.Success && match2.Groups.Count > 2)
						{
							text2 = match2.Groups[2].Value;
						}
					}
					if (text2 != null)
					{
						ManagementObjectCollection managementObjectCollection2 = null;
						spacesApiError = SpacesApi.GetStorageObjectsByQuery("Select * From MSFT_PhysicalDisk where ObjectId = '" + text2 + "'", ref managementObjectCollection2);
						if (spacesApiError != SpacesApiError.Success)
						{
							return spacesApiError;
						}
						foreach (ManagementBaseObject managementBaseObject2 in managementObjectCollection2)
						{
							ManagementObject m = (ManagementObject)managementBaseObject2;
							PhysicalDisk physicalDisk = new PhysicalDisk();
							physicalDisk.FromManagementObject(m);
							if (disks == null)
							{
								disks = new List<PhysicalDisk>();
							}
							disks.Add(physicalDisk);
						}
					}
				}
			}
			catch (Exception ex)
			{
				spacesApiError = SpacesApiError.Failed;
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to get physical disks for virtual disk {1}: {0}", new object[]
					{
						ex,
						vdid
					});
				}
			}
			return spacesApiError;
		}

		public static SpacesApiError GetVirtualDisksForPhysicalDisk(string pdid, ref List<VirtualDisk> disks)
		{
			SpacesApiError spacesApiError = SpacesApiError.Success;
			try
			{
				Regex regex = new Regex("(.+)MSFT_VirtualDisk.ObjectId=\"([{}0-9a-zA-Z-]+)\"");
				Regex regex2 = new Regex("(.+)MSFT_PhysicalDisk.ObjectId=\"([{}0-9a-zA-Z-]+)\"");
				ManagementObjectCollection managementObjectCollection = null;
				spacesApiError = SpacesApi.GetStorageObjectsByQuery("Select * from MSFT_VirtualDiskToPhysicalDisk", ref managementObjectCollection);
				if (spacesApiError != SpacesApiError.Success)
				{
					return spacesApiError;
				}
				foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
				{
					ManagementObject managementObject = (ManagementObject)managementBaseObject;
					string text = null;
					string text2 = null;
					if (managementObject["PhysicalDisk"] != null)
					{
						Match match = regex2.Match((string)managementObject["PhysicalDisk"]);
						if (match.Success && match.Groups.Count > 2)
						{
							text2 = match.Groups[2].Value;
							if (text2 != pdid)
							{
								continue;
							}
						}
					}
					if (text2 != null && managementObject["VirtualDisk"] != null)
					{
						Match match2 = regex.Match((string)managementObject["VirtualDisk"]);
						if (match2.Success && match2.Groups.Count > 2)
						{
							text = match2.Groups[2].Value;
						}
					}
					if (text != null)
					{
						ManagementObjectCollection managementObjectCollection2 = null;
						spacesApiError = SpacesApi.GetStorageObjectsByQuery("Select * From MSFT_VirtualDisk where ObjectId = '" + text + "'", ref managementObjectCollection2);
						if (spacesApiError != SpacesApiError.Success)
						{
							return spacesApiError;
						}
						foreach (ManagementBaseObject managementBaseObject2 in managementObjectCollection2)
						{
							ManagementObject m = (ManagementObject)managementBaseObject2;
							VirtualDisk virtualDisk = new VirtualDisk();
							virtualDisk.FromManagementObject(m);
							if (disks == null)
							{
								disks = new List<VirtualDisk>();
							}
							disks.Add(virtualDisk);
						}
					}
				}
			}
			catch (Exception ex)
			{
				spacesApiError = SpacesApiError.Failed;
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to get virtual disks for physical disk {1}: {0}", new object[]
					{
						ex,
						pdid
					});
				}
			}
			return spacesApiError;
		}

		public static bool IsPhysicalDiskSystem(PhysicalDisk pd)
		{
			bool result = false;
			try
			{
				string text = null;
				string text2 = null;
				Regex regex = new Regex("PhysicalDisk([0-9]+)");
				Match match = regex.Match(pd.FriendlyName);
				if (match != null && match.Success && match.Groups.Count > 1)
				{
					text = match.Groups[1].Value;
				}
				if (text != null)
				{
					ManagementObjectCollection managementObjectCollection = null;
					if (SpacesApi.GetWin32ObjectsByQuery("select * from Win32_DiskPartition where BootPartition=True and PrimaryPartition=True", ref managementObjectCollection) == SpacesApiError.Success)
					{
						foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
						{
							ManagementObject managementObject = (ManagementObject)managementBaseObject;
							if (managementObject["DeviceId"] != null)
							{
								string input = (string)managementObject["DeviceId"];
								Regex regex2 = new Regex("Disk #([0-9]+), Partition #([0-9])");
								Match match2 = regex2.Match(input);
								if (match2 != null && match2.Success && match2.Groups.Count > 1)
								{
									text2 = match2.Groups[1].Value;
									break;
								}
							}
						}
					}
				}
				if (text != null && text2 != null)
				{
					result = (text == text2);
				}
			}
			catch (Exception ex)
			{
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to determine if physial disk {0} is system disk: {1}", new object[]
					{
						pd.FriendlyName,
						ex
					});
				}
			}
			return result;
		}

		public static SpacesApiError SetPoolAttributes(string PoolId, StoragePoolAttributes attrs)
		{
			SpacesApiError result = SpacesApiError.Success;
			try
			{
				ManagementObject managementObject = new ManagementObject(SpacesApi.GetStorageScope(), new ManagementPath("MSFT_StoragePool.ObjectId='" + PoolId + "'"), null);
				ManagementBaseObject methodParameters = managementObject.GetMethodParameters("SetAttributes");
				if (attrs.IsPowerProtected.IsPropertySet)
				{
					methodParameters["IsPowerProtected"] = attrs.IsPowerProtected;
				}
				if (attrs.ClearOnDeallocate.IsPropertySet)
				{
					methodParameters["ClearOnDeallocate"] = attrs.ClearOnDeallocate;
				}
				if (attrs.RetireMissingPhysicalDisks.IsPropertySet)
				{
					methodParameters["RetireMissingPhysicalDisks"] = attrs.RetireMissingPhysicalDisks;
				}
				if (attrs.ThinProvisioningAlertThresholds.IsPropertySet)
				{
					methodParameters["ThinProvisioningAlertThresholds"] = attrs.ThinProvisioningAlertThresholds;
				}
				ManagementBaseObject managementBaseObject = managementObject.InvokeMethod("SetAttributes", methodParameters, null);
				result = SpacesApi.ConvertError(managementBaseObject["returnValue"]);
			}
			catch (Exception ex)
			{
				result = SpacesApiError.Failed;
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to set pool attributes: {0}", new object[]
					{
						ex
					});
				}
			}
			return result;
		}

		public static SpacesApiError GetPhysicalDiskPool(string pdid, ref StoragePool pool)
		{
			SpacesApiError spacesApiError = SpacesApiError.Success;
			List<StoragePool> list = null;
			pool = null;
			spacesApiError = SpacesApi.GetPhysicalDiskPools(pdid, ref list);
			if (spacesApiError == SpacesApiError.Success)
			{
				if (list.Count == 1)
				{
					pool = list[0];
				}
				else
				{
					foreach (StoragePool storagePool in list)
					{
						if (!storagePool.IsPrimordial)
						{
							pool = storagePool;
							break;
						}
					}
				}
			}
			return spacesApiError;
		}

		public static string EscapedChar(Match match)
		{
			if (match.Success)
			{
				string value = match.Value;
				if (value.Length > 1)
				{
					return value.Substring(1, 1);
				}
			}
			return " ";
		}

		public static string UnescapeString(string str)
		{
			try
			{
				Regex regex = new Regex("(\\\\.)");
				return regex.Replace(str, new MatchEvaluator(SpacesApi.EscapedChar));
			}
			catch (Exception ex)
			{
				SpacesApi.Debug("UnescapeString exception: {0}", new object[]
				{
					ex
				});
			}
			return str;
		}

		public static SpacesApiError GetPhysicalDiskPools(string pdid, ref List<StoragePool> pools)
		{
			SpacesApiError spacesApiError = SpacesApiError.Success;
			pools = new List<StoragePool>();
			List<StoragePool> list = new List<StoragePool>();
			try
			{
				spacesApiError = SpacesApi.GetStoragePools(ref list);
				if (spacesApiError != SpacesApiError.Success)
				{
					SpacesApi.Debug("GetPhysicalDiskPools failed to get pools", new object[0]);
					return spacesApiError;
				}
				if (list.Count == 0 || (list.Count == 1 && list[0].IsPrimordial))
				{
					SpacesApi.Debug("GetPhysicalDiskPools only primordial pool existss", new object[0]);
					return spacesApiError;
				}
				Regex regex = new Regex("(.+)MSFT_StoragePool.ObjectId=\"(.+)\"$");
				Regex regex2 = new Regex("(.+)MSFT_PhysicalDisk.ObjectId=\"(.+)\"$");
				ManagementObjectCollection managementObjectCollection = null;
				spacesApiError = SpacesApi.GetStorageObjectsByQuery("Select * from MSFT_StoragePoolToPhysicalDisk", ref managementObjectCollection);
				if (spacesApiError != SpacesApiError.Success)
				{
					SpacesApi.Debug("GetPhysicalDiskPools failed to get MSFT_StoragePoolToPhysicalDisk", new object[0]);
					return spacesApiError;
				}
				foreach (ManagementBaseObject managementBaseObject in managementObjectCollection)
				{
					ManagementObject managementObject = (ManagementObject)managementBaseObject;
					string text = null;
					string text2 = null;
					if (managementObject["PhysicalDisk"] != null)
					{
						Match match = regex2.Match((string)managementObject["PhysicalDisk"]);
						if (match.Success && match.Groups.Count > 2)
						{
							text2 = match.Groups[2].Value;
							text2 = SpacesApi.UnescapeString(text2);
							if (!string.Equals(text2, pdid, StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}
						}
					}
					if (text2 != null && managementObject["StoragePool"] != null)
					{
						Match match2 = regex.Match((string)managementObject["StoragePool"]);
						if (match2.Success && match2.Groups.Count > 2)
						{
							text = match2.Groups[2].Value;
							text = SpacesApi.UnescapeString(text);
						}
					}
					if (text != null)
					{
						foreach (StoragePool storagePool in list)
						{
							if (string.Equals(text, storagePool.ObjectId, StringComparison.OrdinalIgnoreCase))
							{
								pools.Add(storagePool);
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				spacesApiError = SpacesApiError.Failed;
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("Failed to get storage pools for physical disk {1}: {0}", new object[]
					{
						ex,
						pdid
					});
				}
			}
			return spacesApiError;
		}

		protected static ManagementScope scope = null;

		public static bool DebugOn = false;

		public static string StorageNamespace = "\\\\.\\Root\\Microsoft\\Windows\\Storage";
	}
}
