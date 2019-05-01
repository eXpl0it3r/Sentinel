using System;
using System.Diagnostics;
using System.Reflection;

namespace Stor
{
	internal class Logger
	{
		public static void Initialize(string appname)
		{
			Logger.shuttingDown = false;
			if (appname != null)
			{
				Logger.logSourceName = appname;
			}
			if (!EventLog.SourceExists(Logger.logSourceName))
			{
				EventLog.CreateEventSource(Logger.logSourceName, "Application");
			}
		}

		public static void ShuttingDown()
		{
			Logger.shuttingDown = true;
		}

		public static void Debug(string fmt, params object[] args)
		{
			if (Logger.debugSwitch.Enabled)
			{
				if (Logger.shuttingDown)
				{
					return;
				}
				try
				{
					EventLog.WriteEntry(Logger.logSourceName, string.Format(fmt, args));
				}
				catch (Exception)
				{
				}
			}
		}

		public static void Error(string fmt, params object[] args)
		{
			if (Logger.shuttingDown)
			{
				return;
			}
			try
			{
				EventLog.WriteEntry(Logger.logSourceName, string.Format(fmt, args), EventLogEntryType.Error);
			}
			catch (Exception)
			{
			}
		}

		public static void Warn(string fmt, params object[] args)
		{
			if (Logger.shuttingDown)
			{
				return;
			}
			try
			{
				EventLog.WriteEntry(Logger.logSourceName, string.Format(fmt, args), EventLogEntryType.Warning);
			}
			catch (Exception)
			{
			}
		}

		public static void Info(string fmt, params object[] args)
		{
			if (Logger.shuttingDown)
			{
				return;
			}
			try
			{
				EventLog.WriteEntry(Logger.logSourceName, string.Format(fmt, args), EventLogEntryType.Information);
			}
			catch (Exception)
			{
			}
		}

		private static bool shuttingDown = false;

		private static BooleanSwitch debugSwitch = new BooleanSwitch("debugSwitch", "Log debug events", "false");

		private static string logSourceName = Assembly.GetExecutingAssembly().GetName().Name;
	}
}
