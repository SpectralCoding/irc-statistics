using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using MySql;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Reflection;
using IRCShared;
using IRCLogger.Support;
using IRCLogger.IRC;

namespace IRCLogger {
	class Program {
		private static DBConnection EnumeraterDBConn = new DBConnection();
		private static List<Server> Servers = new List<Server>();

		static void Main(string[] args) {
			AppLog.WriteLine(1, "STATUS", "Entered IRCLogger.Program.Main(). IRCStatistics v" + Assembly.GetExecutingAssembly().GetName().Version + " started.");
			Config.LoadConfig();
			EnumeraterDBConn.Connect(Config.SQLServerHost, Config.SQLServerPort, Config.SQLUsername, Config.SQLPassword, Config.SQLDatabase);
			ConnectToServers();	// Connect to all the IRC servers and split off those threads
			Console.ReadLine();
		}

		public static void ConnectToServers() {
			AppLog.WriteLine(5, "STATUS", "Entered IRCLogger.Program.ConnectToServers().");
			MySqlCommand Cmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "networks", EnumeraterDBConn.Connection);
			MySqlDataReader DataReader = Cmd.ExecuteReader();
			DataTable NetworkTable = new DataTable();
			NetworkTable.Load(DataReader);
			DataReader.Close();
			bool ServerExists;
			foreach (DataRow CurNetwork in NetworkTable.Rows) {
				ServerExists = false;
				foreach (Server CurServer in Servers) {
					if (CurServer.ID == Convert.ToInt32(CurNetwork["id"])) {
						ServerExists = true;
						CurServer.JoinChannels();
					}
				}
				if (!ServerExists) {
					Server TempServer = new Server(Servers);
					TempServer.ID = Convert.ToInt32(CurNetwork["id"]);
					TempServer.Hostname = CurNetwork["server"].ToString();
					TempServer.Network = CurNetwork["name"].ToString();
					TempServer.Port = Convert.ToInt32(CurNetwork["port"]);
					TempServer.Pass = CurNetwork["password"].ToString();
					TempServer.RealName = CurNetwork["realname"].ToString();
					TempServer.Nick = CurNetwork["nickname"].ToString();
					TempServer.AltNick = CurNetwork["altnickname"].ToString();
					Servers.Add(TempServer);
					TempServer.Connect();
				}
			}
		}

	}
}
