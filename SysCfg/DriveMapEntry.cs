using System.Collections.Generic;

namespace WDSystemConfig
{
	public class DriveMapEntry
	{
		public Dictionary<MapIndexType, MapIndexData> indexData { get; set; }

		public DriveMapEntry()
		{
			this.indexData = new Dictionary<MapIndexType, MapIndexData>();
		}

		public void Add(MapIndexType type, string index)
		{
			MapIndexData mapIndexData = new MapIndexData();
			mapIndexData.type = type;
			mapIndexData.index = index;
			this.indexData[type] = mapIndexData;
		}

		public string GetData(MapIndexType type)
		{
			return this.indexData[type].index;
		}

		public bool IsBoot
		{
			get
			{
				return this.isBoot;
			}
		}

		public bool isBoot;
	}
}
