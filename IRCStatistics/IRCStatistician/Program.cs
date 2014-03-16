using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Reflection;
using IRCShared;
using IRCStatistician.Support;
using IRCStatistician.LogMgmt;

namespace IRCStatistician {
	class Program {
		private static DBConnection EnumeraterDBConn = new DBConnection();

		static void Main(string[] args) {
			AppLog.WriteLine(1, "STATUS", "Entered IRCStatistician.Program.Main(). IRCStatistics v" + Assembly.GetExecutingAssembly().GetName().Version + " started.");
			Config.LoadConfig();
			Compressor.Compress();
			CompileStats();
			Console.ReadLine();
		}

		public static void CompileStats() {
			AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.Program.CompileStats().");
			EnumeraterDBConn.Connect(Config.SQLServerHost, Config.SQLServerPort, Config.SQLUsername, Config.SQLPassword, Config.SQLDatabase);
			List<LogWatcher> LogWatchers = CreateLogWatchers();
			foreach (LogWatcher CurLogWatcher in LogWatchers) {
				//CurLogWatcher.Compress();
			}
		}

		public static List<LogWatcher> CreateLogWatchers() {
			AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.Program.CreateLogWatchers().");
			List<LogWatcher> returnList = new List<LogWatcher>();
			// Get the tbn_networks table
			MySqlCommand NetworkCmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "networks", EnumeraterDBConn.Connection);
			MySqlDataReader NetworkDataReader = NetworkCmd.ExecuteReader();
			DataTable NetworkTable = new DataTable();
			NetworkTable.Load(NetworkDataReader);
			NetworkDataReader.Close();
			// Get the tbn_channels table
			MySqlCommand ChannelCmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "channels", EnumeraterDBConn.Connection);
			MySqlDataReader ChannelDataReader = ChannelCmd.ExecuteReader();
			DataTable ChannelTable = new DataTable();
			ChannelTable.Load(ChannelDataReader);
			ChannelDataReader.Close();
			// Organize them together.
			foreach (DataRow CurNetwork in NetworkTable.Rows) {
				LogWatcher tempLW = new LogWatcher();
				tempLW.ID = Convert.ToInt32(CurNetwork["id"]);
				tempLW.Hostname = CurNetwork["server"].ToString();
				tempLW.Network = CurNetwork["name"].ToString();
				tempLW.Port = Convert.ToInt32(CurNetwork["port"]);
				tempLW.Pass = CurNetwork["password"].ToString();
				tempLW.RealName = CurNetwork["realname"].ToString();
				tempLW.Nick = CurNetwork["nickname"].ToString();
				tempLW.AltNick = CurNetwork["altnickname"].ToString();
				tempLW.Channels = new List<string>();
				foreach (DataRow CurChannel in ChannelTable.Rows) {
					if (Convert.ToInt32(CurChannel["networkid"]) == tempLW.ID) {
						tempLW.Channels.Add(CurChannel["name"].ToString());
					}
				}
				returnList.Add(tempLW);
			}
			return returnList;
		}

	}
}
