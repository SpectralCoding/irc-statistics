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
using IRCStatistician.IRC;

namespace IRCStatistician.LogMgmt {
	class NetworkProcessor {
		public Network Network;
		private Dictionary<string, LogFileRow> m_LogTable;

		public NetworkProcessor(DataRow NetRow) {
			Network = new Network();
			Network.Id = Convert.ToInt32(NetRow["id"]);
			Network.Hostname = NetRow["server"].ToString();
			Network.Name = NetRow["name"].ToString();
			Network.Port = Convert.ToInt32(NetRow["port"]);
			Network.Pass = NetRow["password"].ToString();
			Network.RealName = NetRow["realname"].ToString();
			Network.Nick = NetRow["nickname"].ToString();
			Network.AltNick = NetRow["altnickname"].ToString();
			Network.Channels = new Dictionary<string, Channel>();
		}

		public void Process(Dictionary<string, LogFileRow> LogTable) {
			m_LogTable = LogTable;
			AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.NetworkProcessor.Process().");
			string[] LogFiles = Directory.GetFiles(@"chatlogs/", Network.Name + "_" + Network.Id + "_????????*.???");
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
			DateTime StartOfDay = DateTime.MinValue;
			DateTime Timestamp = DateTime.MinValue;
			while ((CurLine = LogTextReader.ReadLine()) != null) {
				if (CurLine.Substring(0, 1) == "#") {
					if (CurLine.Substring(0, 9) == "# Opened:") {
						string DayStr = CurLine.Substring(10);
						StartOfDay = Convert.ToDateTime(DayStr.Substring(0, DayStr.IndexOf(' ')));
					}
					// Is a comment
				} else {
					string TimeNum = CurLine.Substring(0, CurLine.IndexOf(' '));
					Timestamp = StartOfDay.AddMilliseconds(Convert.ToDouble(TimeNum));
					CurLine = CurLine.Substring(TimeNum.Length + 1);
					if (CurLine.Substring(0, 1) == ":") { 
						CurLine = CurLine.Substring(1);
						string[] ParameterSplit = CurLine.Split(" ".ToCharArray(), 3, StringSplitOptions.RemoveEmptyEntries);
						string Sender = ParameterSplit[0];
						string Command = ParameterSplit[1];
						string Parameters = ParameterSplit[2];
						// Even though we've logged it, we still need to send it down
						// the line for stuff like PING, CTCP, joining channels, etc.
						//Network.Parse(Timestamp, Sender, Command, Parameters);
					} else {
						AppLog.WriteLine(5, "DEBUG", "Unknown Line Format: " + CurLine);
					}
				}
				i++;
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
					// And we've processed it using the latest revision of code
					if (Assembly.GetExecutingAssembly().GetName().Version.ToString() == m_LogTable[Filename].ProductVer) {
						// Then we don't need to process it
						return false;
					}
				}
			}
			// Otherwise, process it
			return true;
		}



	}
}
