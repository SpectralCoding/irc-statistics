using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace IRCShared {
	public class DBConnection {
		public MySqlConnection Connection;
		public void Connect(string Host, int Port, string Username, string Password, string Database) {
			AppLog.WriteLine(5, "STATUS", "Entering IRCStatistics.Database.DBConnection.Connect().");
			string ConnectionStr = String.Format("SERVER={0};PORT={1};DATABASE={2};UID={3};PASSWORD={4}", Host, Port, Database, Username, Password);
			AppLog.WriteLine(3, "DATABASE", "Conncting to database " + Database + " on " + Host + ":" + Port + " as " + Username + ".");
			try {
				Connection = new MySqlConnection(ConnectionStr);
				Connection.Open();
			} catch (MySqlException Ex) {
				AppLog.WriteLine(1, "ERROR", "Error " + Ex.Number + ": " + Ex.Message);
			}
		}

		public void Disconnect() {
			AppLog.WriteLine(5, "STATUS", "Entering IRCStatistics.Database.DBConnection.Disconnect().");
			try {
				Connection.Close();
			} catch (MySqlException Ex) {
				AppLog.WriteLine(1, "ERROR", "Error " + Ex.Number + ": Unknown. Look this number up.");
			}
		}

	}
}
