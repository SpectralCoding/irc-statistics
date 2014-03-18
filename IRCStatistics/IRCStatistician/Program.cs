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
using IRCStatistician.IRC;

namespace IRCStatistician {
	class Program {
		public static DBConnection MyDBConn = new DBConnection();

		static void Main(string[] args) {
			AppLog.WriteLine(1, "STATUS", "Entered IRCStatistician.Program.Main(). IRCStatistics v" + Assembly.GetExecutingAssembly().GetName().Version + " started.");
			Config.LoadConfig();
			// Compress non-current files, that is, anything before today's logs.
			Compressor.Compress();
			// Do all the work!
			CompileStats();
			Console.ReadLine();
		}

		public static void CompileStats() {
			AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.Program.CompileStats().");
			MyDBConn.Connect(Config.SQLServerHost, Config.SQLServerPort, Config.SQLUsername, Config.SQLPassword, Config.SQLDatabase);
			Dictionary<string, LogFileRow> LogTable = ReadLogTable();
			List<NetworkProcessor> NetProcs = CreateNetProcs();
			foreach (NetworkProcessor CurNetProc in NetProcs) {
				CurNetProc.Process(LogTable);
			}
		}

		public static List<NetworkProcessor> CreateNetProcs() {
			AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.Program.CreateNetProcs().");
			List<NetworkProcessor> returnList = new List<NetworkProcessor>();
			// Get the tbn_networks table
			MySqlCommand NetworkCmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "networks", MyDBConn.Connection);
			MySqlDataReader NetworkDataReader = NetworkCmd.ExecuteReader();
			DataTable NetworkTable = new DataTable();
			NetworkTable.Load(NetworkDataReader);
			NetworkDataReader.Close();
			// Get the tbn_channels table
			MySqlCommand ChannelCmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "channels", MyDBConn.Connection);
			MySqlDataReader ChannelDataReader = ChannelCmd.ExecuteReader();
			DataTable ChannelTable = new DataTable();
			ChannelTable.Load(ChannelDataReader);
			ChannelDataReader.Close();
			// Organize them together.
			foreach (DataRow CurNetwork in NetworkTable.Rows) {
				NetworkProcessor tempLW = new NetworkProcessor(CurNetwork);
				foreach (DataRow CurChannel in ChannelTable.Rows) {
					if (Convert.ToInt32(CurChannel["networkid"]) == tempLW.Network.Id) {
						tempLW.Network.Channels.Add(CurChannel["name"].ToString(), new Channel(CurChannel["name"].ToString()));
					}
				}
				returnList.Add(tempLW);
			}
			return returnList;
		}

		public static Dictionary<string, LogFileRow> ReadLogTable() {
			Dictionary<string, LogFileRow> returnDict = new Dictionary<string, LogFileRow>();
			AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.NetworkProcessor.ReadLogTable().");
			MySqlCommand Cmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "logfiles ORDER BY id", Program.MyDBConn.Connection);
			MySqlDataReader DataReader = Cmd.ExecuteReader();
			DataTable LogFileTable = new DataTable();
			LogFileTable.Load(DataReader);
			DataReader.Close();
			foreach (DataRow CurNetwork in LogFileTable.Rows) {
				LogFileRow Temp = new LogFileRow();
				Temp.Id = Convert.ToInt32(CurNetwork["Id"]);
				Temp.Filename = CurNetwork["Filename"].ToString();
				Temp.ProductVer = CurNetwork["ProductVer"].ToString();
				Temp.LastReadSize = Convert.ToInt32(CurNetwork["LastReadSize"]);
				returnDict.Add(Temp.Filename, Temp);
			}
			return returnDict;
		}



	}
}
