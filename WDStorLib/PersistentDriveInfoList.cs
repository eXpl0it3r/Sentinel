using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Stor
{
	public class PersistentDriveInfoList
	{
		public PersistentDriveInfo Find(string serial)
		{
			foreach (PersistentDriveInfo persistentDriveInfo in this.persistentDriveInfos)
			{
				if (persistentDriveInfo.serial == serial)
				{
					return persistentDriveInfo;
				}
			}
			return null;
		}

		public void Load()
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "\\" + PersistentDriveInfoList.PersistedDriveInfosFile;
			TextReader textReader = null;
			try
			{
				if (File.Exists(text))
				{
					textReader = new StreamReader(text);
					XmlSerializer xmlSerializer = new XmlSerializer(base.GetType());
					PersistentDriveInfoList persistentDriveInfoList = (PersistentDriveInfoList)xmlSerializer.Deserialize(textReader);
					if (persistentDriveInfoList != null)
					{
						this.persistentDriveInfos = persistentDriveInfoList.persistentDriveInfos;
					}
					Logger.Info("Read persisted drives:\n{0}", new object[]
					{
						string.Join<PersistentDriveInfo>(Environment.NewLine, this.persistentDriveInfos)
					});
				}
			}
			catch (Exception ex)
			{
				Logger.Warn("Failed to persisted drives from {0}: {1}", new object[]
				{
					text,
					ex
				});
			}
			finally
			{
				if (this.persistentDriveInfos.Count == 0)
				{
					Logger.Info("Persistent drive list was empty\n", new object[0]);
					this.wasEmptyOnLoad = true;
				}
				if (textReader != null)
				{
					textReader.Close();
				}
			}
		}

		public void Save()
		{
			string text = AppDomain.CurrentDomain.BaseDirectory + "\\" + PersistentDriveInfoList.PersistedDriveInfosFile;
			TextWriter textWriter = null;
			try
			{
				Logger.Info("Saving persisted drives\n{0}", new object[]
				{
					string.Join<PersistentDriveInfo>(";", this.persistentDriveInfos)
				});
				textWriter = new StreamWriter(text, false);
				XmlSerializer xmlSerializer = new XmlSerializer(base.GetType());
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception ex)
			{
				Logger.Warn("Failed to save persisted drives to {0}: {1}", new object[]
				{
					text,
					ex
				});
			}
			finally
			{
				if (textWriter != null)
				{
					textWriter.Close();
				}
			}
		}

		public void Add(PersistentDriveInfo d)
		{
			bool flag = false;
			if (this.Find(d.serial) == null)
			{
				if (this.wasEmptyOnLoad)
				{
					d.creationTime -= PersistentDriveInfoList.PersistentDriveExpiration;
				}
				this.persistentDriveInfos.Add(d);
				flag = true;
			}
			if (flag)
			{
				this.Save();
			}
		}

		public void Update(PersistentDriveInfo d)
		{
			if (d != null)
			{
				d.lastInserted = DateTime.Now;
				this.Save();
			}
		}

		public void Remove(PersistentDriveInfo d)
		{
			this.Remove(d.serial);
		}

		public void Remove(string serial)
		{
			bool flag = false;
			PersistentDriveInfo persistentDriveInfo = this.Find(serial);
			if (persistentDriveInfo != null)
			{
				this.persistentDriveInfos.Remove(persistentDriveInfo);
				flag = true;
			}
			if (flag)
			{
				this.Save();
			}
		}

		public void RemoveAll()
		{
			this.persistentDriveInfos.Clear();
		}

		public int RemoveAll(Predicate<PersistentDriveInfo> pred)
		{
			return this.persistentDriveInfos.RemoveAll(pred);
		}

		public void ForEach(Action<PersistentDriveInfo> act)
		{
			this.persistentDriveInfos.ForEach(act);
		}

		public void Scrub()
		{
		}

		public static string PersistedDriveInfosFile = "PersistedDriveInfos.xml";

		public static TimeSpan PersistentDriveExpiration = new TimeSpan(24, 0, 0);

		public static TimeSpan PersistentDriveJustInsertedExpiration = new TimeSpan(0, 1, 0);

		[XmlIgnore]
		public bool wasEmptyOnLoad;

		public List<PersistentDriveInfo> persistentDriveInfos = new List<PersistentDriveInfo>();
	}
}
