﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IRCShared;

namespace IRCStatistician.Support {
	public static class Config {
		private static string m_SQLServerHost;
		private static int m_SQLServerPort;
		private static string m_SQLUsername;
		private static string m_SQLPassword;
		private static string m_SQLDatabase;
		private static string m_SQLTablePrefix;
		private static int m_Resolution;

		public static string SQLServerHost { get { return m_SQLServerHost; } set { m_SQLServerHost = value; } }
		public static int SQLServerPort { get { return m_SQLServerPort; } set { m_SQLServerPort = value; } }
		public static string SQLUsername { get { return m_SQLUsername; } set { m_SQLUsername = value; } }
		public static string SQLPassword { get { return m_SQLPassword; } set { m_SQLPassword = value; } }
		public static string SQLDatabase { get { return m_SQLDatabase; } set { m_SQLDatabase = value; } }
		public static string SQLTablePrefix { get { return m_SQLTablePrefix; } set { m_SQLTablePrefix = value; } }
		public static int Resolution { get { return m_Resolution; } set { m_Resolution = value; } }
		public static void LoadConfig() {
			if (File.Exists("IRCStatistician.conf")) {
				AppLog.WriteLine(1, "STATUS", "Loading configuration from IRCStatistician.conf");
				string CurLine;
				StreamReader ConfigFile = new StreamReader("IRCStatistician.conf");
				while ((CurLine = ConfigFile.ReadLine()) != null) {
					if (CurLine.Substring(0, 1) != "#") {
						string[] CurLineSplit = CurLine.Split(new char[1] { '=' }, 2);
						AppLog.WriteLine(4, "CONFIG", "Configuration Option: " + CurLineSplit[0] + " = " + CurLineSplit[1]);
						switch (CurLineSplit[0].ToLower()) {
							case "sqlserverhost":
								m_SQLServerHost = CurLineSplit[1];
								break;
							case "sqlserverport":
								m_SQLServerPort = Convert.ToInt32(CurLineSplit[1]);
								break;
							case "sqlusername":
								m_SQLUsername = CurLineSplit[1];
								break;
							case "sqlpassword":
								m_SQLPassword = CurLineSplit[1];
								break;
							case "sqldatabase":
								m_SQLDatabase = CurLineSplit[1];
								break;
							case "sqltableprefix":
								m_SQLTablePrefix = CurLineSplit[1];
								break;
							case "resolution":
								m_Resolution = Convert.ToInt32(CurLineSplit[1]);
								break;
							default:
								AppLog.WriteLine(1, "ERROR", "Unknown Configuration Option: " + CurLineSplit[0] + " = " + CurLineSplit[1]);
								break;
						}
					}
				}
			} else {
				AppLog.WriteLine(1, "ERROR", "No Configuration File Found. Did you copy IRCStatistician.sample.conf and change the settings?");
			}
		}
	}
}
