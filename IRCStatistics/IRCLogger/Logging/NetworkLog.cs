using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IRCShared;
using System.Reflection;

namespace IRCLogger.Logging {
	public class NetworkLog {
		private FileStream m_LogFile;
		private StreamWriter m_LogStream;
		private string m_Network;
		private int m_ID;

		public NetworkLog(int ID, string Network) {
			m_ID = ID;
			m_Network = Network;
		}

		private void OpenLog() {
			if (!Directory.Exists("chatlogs/")) {
				Directory.CreateDirectory("chatlogs/");
			}
			string logname = @"chatlogs/" + String.Format("{0}_{1}_{2:yyyyMMdd-HH}.log", m_Network, m_ID, DateTime.UtcNow);
			if (File.Exists(logname)) {
				m_LogFile = new FileStream(@"chatlogs/" + String.Format("{0}_{1}_{2:yyyyMMdd-HH}.log", m_Network, m_ID, DateTime.UtcNow), FileMode.Append, FileAccess.Write);
			} else {
				m_LogFile = new FileStream(@"chatlogs/" + String.Format("{0}_{1}_{2:yyyyMMdd-HH}.log", m_Network, m_ID, DateTime.UtcNow), FileMode.Create, FileAccess.Write);
			}
			m_LogStream = new StreamWriter(m_LogFile);
			m_LogStream.AutoFlush = true;
			WriteLine("Opened (UTC): " + DateTime.UtcNow, true);
			WriteLine("Network: " + m_Network + " (ID: " + m_ID + ")", true);
			WriteLine("Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString(), true);
		}

		public void CloseLog() {
			WriteLine("Closed (UTC): " + DateTime.UtcNow, true);
			m_LogStream.Close();
			m_LogFile.Close();
		}

		public void WriteLine(string LineToAdd, bool Meta = false) {
			TimeSpan TimeSinceMidnight = DateTime.UtcNow.TimeOfDay;
			double MSOfDay = Math.Floor(TimeSinceMidnight.TotalMilliseconds);
			string Output;
			if (Meta) {
				Output = String.Format("# {0}", LineToAdd);
			} else {
				Output = String.Format("{0} {1}", MSOfDay, LineToAdd);
			}
			if (m_LogFile == null) {
				OpenLog();
			} else {
				if (Path.GetFileName(m_LogFile.Name) != String.Format("{0}_{1}_{2:yyyyMMdd-HH}.log", m_Network, m_ID, DateTime.UtcNow)) {
					m_LogStream.WriteLine("# Closed (UTC): " + DateTime.UtcNow);
					m_LogStream.Close();
					m_LogFile.Close();
					OpenLog();
				}
			}
			m_LogStream.WriteLine(Output);
		}



	}
}
