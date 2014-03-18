using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ionic.Zip;
using IRCShared;
using MySql.Data;
using MySql.Data.MySqlClient;
using IRCStatistician.Support;
using System.Data;
using System.Reflection;

namespace IRCStatistician.LogMgmt {
	class NetworkProcessor {
		public List<string> Channels;
		private string m_ServerHost;
		private int m_ServerPort;
		private string m_ServerPass;
		private string m_RealName;
		private string m_Nick;
		private string m_AltNick;
		private string m_Network;
		private int m_ID;
		private Dictionary<string, LogFileRow> m_LogTable;

		public string Hostname { get { return m_ServerHost; } set { m_ServerHost = value; } }
		public int Port { get { return m_ServerPort; } set { m_ServerPort = value; } }
		public string Pass { get { return m_ServerPass; } set { m_ServerPass = value; } }
		public string RealName { get { return m_RealName; } set { m_RealName = value; } }
		public string Nick { get { return m_Nick; } set { m_Nick = value; } }
		public string AltNick { get { return m_AltNick; } set { m_AltNick = value; } }
		public string Network { get { return m_Network; } set { m_Network = value; } }
		public int ID { get { return m_ID; } set { m_ID = value; } }

		public NetworkProcessor() {
		}

		public void Process(Dictionary<string, LogFileRow> LogTable) {
			m_LogTable = LogTable;
			AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.NetworkProcessor.Process().");
			string[] LogFiles = Directory.GetFiles(@"chatlogs/", m_Network + "_" + m_ID + "_????????*.???");
			foreach (string CurLogFile in LogFiles) {
				string Extension = Path.GetExtension(CurLogFile);
				if (NeedToProcess(CurLogFile)) {
					if (Extension == ".zip") {
						AppLog.WriteLine(1, "STATUS", "Processing Zip File: " + CurLogFile);
						ProcessZip(CurLogFile);
						MarkProcessed(CreateLFR(CurLogFile));
					} else if (Extension == ".log") {
						AppLog.WriteLine(1, "STATUS", "Processing Log File: " + CurLogFile);
						StreamReader tempStreamReader = new StreamReader(CurLogFile);
						string LogData = tempStreamReader.ReadToEnd();
						ProcessLog(LogData);
						MarkProcessed(CreateLFR(CurLogFile));
					}
				} else {
					AppLog.WriteLine(1, "STATUS", "Skipping File: " + CurLogFile);
				}
			}

		}

		private LogFileRow CreateLFR(string Filename) {
			LogFileRow Temp = new LogFileRow();
			Temp.Filename = Filename;
			Temp.LastReadSize = (new FileInfo(Filename)).Length;
			Temp.ProductVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			return Temp;
		}

		private bool NeedToProcess(string Filename) {
			// If the entry exists in the table
			if (m_LogTable.ContainsKey(Filename)) {
				// And the size matches
				if ((new FileInfo(Filename)).Length == m_LogTable[Filename].LastReadSize) {
					// And we've processes it using the latest revision of code
					if (Assembly.GetExecutingAssembly().GetName().Version.ToString() == m_LogTable[Filename].ProductVer) {
						// Then we don't need to process it
						return false;
					}
				}
			}
			// Otherwise, process it
			return true;
		}

		public void ProcessZip(string ZipOfLogs) {
			using (ZipFile LogZip = ZipFile.Read(ZipOfLogs)) {
				foreach (ZipEntry ZipEntry in LogZip) {
					AppLog.WriteLine(1, "STATUS", "Processing Log File: " + ZipOfLogs + " => " + ZipEntry.FileName);
					MemoryStream tempMemStream = new MemoryStream();
					tempMemStream.Seek(0, SeekOrigin.Begin);
					ZipEntry.Extract(tempMemStream);
					tempMemStream.Seek(0, SeekOrigin.Begin);
					StreamReader tempStreamReader = new StreamReader(tempMemStream);
					string LogData = tempStreamReader.ReadToEnd();
					ProcessLog(LogData);
				}
			}
		}

		public void ProcessLog(string Data) {
			TextReader LogTextReader = new StringReader(Data);
			string CurLine;
			int i = 0;
			while ((CurLine = LogTextReader.ReadLine()) != null) {
				i++;
				//Console.WriteLine(CurLine);
			}
		}

		public void MarkProcessed(LogFileRow LogFileRow) {
			MySqlCommand Cmd;
			if (m_LogTable.ContainsKey(LogFileRow.Filename)) {
				AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.NetworkProcessor.MarkProcessed(). Updating Entry...");
				Cmd = new MySqlCommand("UPDATE `" + Config.SQLTablePrefix + "logfiles` SET `LastReadSize` = @lastreadsize, `ProductVer` = @productver WHERE `id` = @id", Program.MyDBConn.Connection);
				Cmd.Prepare();
				Cmd.Parameters.AddWithValue("@id", LogFileRow.Id);
			} else {
				AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.NetworkProcessor.MarkProcessed(). Creating Entry...");
				Cmd = new MySqlCommand("INSERT INTO `" + Config.SQLTablePrefix + "logfiles` (`Filename`, `LastReadSize`, `ProductVer`) VALUES (@filename, @lastreadsize, @productver)", Program.MyDBConn.Connection);
				Cmd.Prepare();
				Cmd.Parameters.AddWithValue("@filename", LogFileRow.Filename);
			}
			Cmd.Parameters.AddWithValue("@lastreadsize", LogFileRow.LastReadSize);
			Cmd.Parameters.AddWithValue("@productver", LogFileRow.ProductVer);
			int ReturnVal = Cmd.ExecuteNonQuery();
		}

	}
}
