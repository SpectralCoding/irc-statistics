using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Reflection;
using IRCShared;

namespace IRCStatistician {
	class Program {
		//private static DBConnection EnumeraterDBConn = new DBConnection();
		//private static List<Server> Servers = new List<Server>();

		static void Main(string[] args) {
			//AppLog.WriteLine(1, "STATUS", "Entered IRCLogger.Program.Main(). IRCStatistics v" + Assembly.GetExecutingAssembly().GetName().Version + " started.");
			//Config.LoadConfig();
			//EnumeraterDBConn.Connect(Config.SQLServerHost, Config.SQLServerPort, Config.SQLUsername, Config.SQLPassword, Config.SQLDatabase);
			//ConnectToServers();	// Connect to all the IRC servers and split off those threads
			Console.ReadLine();
		}
	}
}
