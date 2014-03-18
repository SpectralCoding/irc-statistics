using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IRCShared {
	public static class AppLog {
		private static int m_LogLevel = 5;
		private static FileStream m_LogFile;
		private static StreamWriter m_LogStream;
		private static bool m_Enabled = true;

		public static bool Enabled { get { return m_Enabled; } set { m_Enabled = value; } }

		/// <summary>
		/// Creates and Opens a log file for program output.
		/// </summary>
		private static void OpenLog() {
			if (!Directory.Exists("logs/")) {
				Directory.CreateDirectory("logs/");
			}
			m_LogFile = new FileStream(@"logs/" + DateTime.UtcNow.ToString("yyMMdd-HHmmss") + ".log", FileMode.Create, FileAccess.ReadWrite);
			m_LogStream = new StreamWriter(m_LogFile);
			m_LogStream.AutoFlush = true;
		}

		public static void SetLogLevel(int NewLogLevel) {
			// 1 = Minimum. Just Errors and major status changes
			// 2 = Unused
			// 3 = Unused
			// 4 = Configuration Output
			// 5 = Most Verbose. Everything.
			m_LogLevel = NewLogLevel;
			AppLog.WriteLine(1, "STATUS", "Logging level set to " + m_LogLevel + ".");
		}

		/// <summary>
		/// Closes an opened log for program output.
		/// </summary>
		public static void CloseLog() {
			m_LogStream.Close();
			m_LogFile.Close();
		}

		public static void WriteLine(int Level, string LogType, string LineToAdd, bool FirstLine = true) {
			if (m_Enabled) {
				if (Level <= m_LogLevel) {
					if (m_LogFile == null) {
						OpenLog();
					}
					string Output;
					if (FirstLine) {
						Output = String.Format("{0:HH:mm:ss.fff} [{1,-6}]\t{2}", DateTime.UtcNow, LogType, LineToAdd);
						m_LogStream.WriteLine(Output);
						Console.WriteLine(Output);
					} else {
						m_LogStream.WriteLine("\t{0}", LineToAdd);
						Console.WriteLine("\t{0}", LineToAdd);
					}
				}
			}
		}

	}
}
