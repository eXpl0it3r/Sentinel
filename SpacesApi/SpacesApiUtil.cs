using System;
using System.Collections.Generic;
using System.Management;

namespace SpacesApi
{
	public class SpacesApiUtil
	{
		public static List<T> ManagementObjectArrayToList<T, R>(object m)
		{
			List<T> list = new List<T>();
			if (m != null)
			{
				Type type = m.GetType();
				if (type.IsArray && type.GetElementType() == typeof(R))
				{
					R[] array = m as R[];
					foreach (R r in array)
					{
						if (typeof(T).IsEnum)
						{
							list.Add((T)((object)Enum.ToObject(typeof(T), r)));
						}
						else
						{
							list.Add((T)((object)Convert.ChangeType(r, typeof(T))));
						}
					}
				}
			}
			return list;
		}

		public static T GetManagementObjectValue<T>(ManagementObject m, string field)
		{
			T result = default(T);
			try
			{
				object obj = m[field];
				if (obj != null)
				{
					result = (T)((object)obj);
				}
				else if (typeof(T) == typeof(string))
				{
					obj = string.Empty;
					result = (T)((object)obj);
				}
			}
			catch (Exception ex)
			{
				if (SpacesApi.DebugOn)
				{
					SpacesApi.Debug("GetManagementObjectValue failed for field {0} of type {1}: {2}", new object[]
					{
						field,
						typeof(T).Name,
						ex
					});
				}
				throw ex;
			}
			return result;
		}
	}
}
