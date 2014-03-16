using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using IRCLogger.Support;
using IRCShared;

namespace IRCLogger.IRC {
	public class BotCommands {
		private Server m_ParentServer;
		private List<string> m_Administrators = new List<string>();

		public BotCommands(Server ParentServer) {
			m_ParentServer = ParentServer;
		}

		public void CheckAdminChange(string Sender, string NewNick) {
			if (m_Administrators.Contains(Sender)) {
				m_Administrators.Remove(Sender);
				m_Administrators.Add(NewNick + "!" + IRCFunctions.GetUserFromHostString(Sender, false) + "@" + IRCFunctions.GetHostFromHostString(Sender));
			}
		}

		public void HandlePM(string Sender, string[] Params) {
			if (Params[0].ToLower() == "!login") {
				HandleLogin(Sender, Params);
			} else if (Params[0].ToLower() == "!logout") {
				HandleLogout(Sender, Params);
			} else if (Params[0].ToLower() == "!help") {
				HandleHelp(Sender, Params);
			} else {
				if (m_Administrators.Contains(Sender)) {
					switch (Params[0].ToLower()) {
						case "!list":
							if (Params.Length >= 2) {
								if (Params[1].ToLower() == "networks") {
									HandleListNetworks(Sender, Params);
								} else if (Params[1].ToLower() == "channels") {
									HandleListChannels(Sender, Params);
								} else if ((Params[1].ToLower() == "admins") || (Params[1].ToLower() == "administrators")) {
									HandleListAdmins(Sender, Params);
								}
							}
							break;
						case "!add":
							if (Params.Length >= 2) {
								if (Params[1].ToLower() == "network") {
									HandleAddNetwork(Sender, Params);
								} else if (Params[1].ToLower() == "channel") {
									HandleAddChannel(Sender, Params);
								}
							}
							break;
						case "!delete":
							if (Params.Length >= 2) {
								if (Params[1].ToLower() == "network") {
									HandleDeleteNetwork(Sender, Params);
								} else if (Params[1].ToLower() == "channel") {
									HandleDeleteChannel(Sender, Params);
								}
							}
							break;
						case "!rehash":
							// This may need to change in the future.
							Program.ConnectToServers();
							break;
						case "!channels":
							HandleChannels(Sender, Params);
							break;
					}
				} else {
					m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "You are not an administrator."));
				}
			}
		}

		private void HandleHelp(string Sender, string[] Params) {
			if (Params.Length >= 2) {
				switch (Params[1].ToLower()) {
					case "add":
						if (Params.Length >= 3) {
							switch (Params[2].ToLower()) {
								case "channel": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Add Channel Command: !add channel <Server ID> <Channel Name> [Channel Password]")); break;
								case "network": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Add Network Command: !add network <Network Name> <IRC Server> <Server Port> <Real Name> <Nick> <Alternate Nick> [Server Password]")); break;
							}
						} else {
							m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Add Command: !add <channel|network>"));
						}
						break;
					case "delete":
						if (Params.Length >= 3) {
							switch (Params[2].ToLower()) {
								case "channel": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Delete Channel Command: !delete channel <Channel ID> <Channel Name>")); break;
								case "network": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Delete Network Command: !delete network <Network ID> <Network Name>")); break;
							}
						} else {
							m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Add Command: !delete <channel|network>"));
						}
						break;
					case "help": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Help Command: !help")); break;
					case "list":
						if (Params.Length >= 3) {
							switch (Params[2].ToLower()) {
								case "admins": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "List Admins Command: !list admins")); break;
								case "channels": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "List Channels Command: !list channels")); break;
								case "networks": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "List Networks Command: !list networks")); break;
							}
						} else {
							m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "List Command: !list <admins|channels|networks>"));
						}
						break;
					case "login": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Login Command: !login <Password>")); break;
					case "logout": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Logout Command: !logout")); break;
					case "rehash": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Rehash Command: !rehash")); break;
					case "channels": m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Channels Command: !channels")); break;
				}
			} else {
				// All help commands
				m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Help Command: !help [add|delete|channels|help|login|logout|rehash]"));
			}
		}

		private void HandleLogin(string Sender, string[] Params) {
			if (Params[1] == Config.AdminPass) {
				if (m_Administrators.Contains(Sender)) {
					m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "You are already identified."));
				} else {
					m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "You are now an Administrator."));
					m_Administrators.Add(Sender);
				}
			} else {
				m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Invalid Password."));
			}
		}

		private void HandleLogout(string Sender, string[] Params) {
			if (m_Administrators.Contains(Sender)) {
				m_Administrators.Remove(Sender);
				m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "You have been logged out."));
			} else {
				m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "You are not logged in."));
			}
		}

		private void HandleListNetworks(string Sender, string[] Params) {
			MySqlCommand Cmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "networks ORDER BY id", m_ParentServer.MyDBConn.Connection);
			MySqlDataReader DataReader = Cmd.ExecuteReader();
			DataTable NetworkTable = new DataTable();
			NetworkTable.Load(DataReader);
			DataReader.Close();
			m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "List of Networks:"));
			foreach (DataRow CurNetwork in NetworkTable.Rows) {
				m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), String.Format("     {0} - {1} ({2}:{3}) as {4}/{5} ({6})", CurNetwork["id"], CurNetwork["name"], CurNetwork["server"], CurNetwork["port"], CurNetwork["nickname"], CurNetwork["altnickname"], CurNetwork["realname"])));
			}
		}

		private void HandleListChannels(string Sender, string[] Params) {
			MySqlCommand Cmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "networks ORDER BY id", m_ParentServer.MyDBConn.Connection);
			MySqlDataReader DataReader = Cmd.ExecuteReader();
			DataTable NetworkTable = new DataTable();
			NetworkTable.Load(DataReader);
			DataReader.Close();
			Cmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "channels ORDER BY id", m_ParentServer.MyDBConn.Connection);
			DataReader = Cmd.ExecuteReader();
			DataTable ChannelTable = new DataTable();
			ChannelTable.Load(DataReader);
			DataReader.Close();
			m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "List of Channels:"));
			foreach (DataRow CurNetwork in NetworkTable.Rows) {
				m_ParentServer.Send(
					IRCFunctions.PrivMsg(
						IRCFunctions.GetNickFromHostString(Sender),
						String.Format("     Network {0} - {1} ({2}:{3}) as {4}/{5} ({6})",
						CurNetwork["id"], CurNetwork["name"], CurNetwork["server"], CurNetwork["port"], CurNetwork["nickname"], CurNetwork["altnickname"], CurNetwork["realname"]
				)));
				foreach (DataRow CurChannel in ChannelTable.Rows) {
					if (Convert.ToInt32(CurChannel["network"]) == Convert.ToInt32(CurNetwork["id"])) {
						m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), String.Format("          Channel {0} - {1}", CurChannel["id"], CurChannel["name"])));
					}
				}
			}
		}

		private void HandleListAdmins(string Sender, string[] Params) {
			if (m_Administrators.Count == 0) {
				m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "No current Administrators."));
			} else {
				m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "List of Administrators:"));
				foreach (string CurAdmin in m_Administrators) {
					m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "     " + CurAdmin));
				}
			}
		}

		private void HandleAddNetwork(string Sender, string[] Params) {
			// !add network "Some Network Name" irc.somenetwork.net 6667 "Real Name" Nick1 Nick2 [Server Password]
			List<string> NewParamList = new List<string>();
			string NewParams = Functions.CombineAfterIndex(Params, " ", 2);
			if (NewParams.Length > 11) {				// Shortest possible parameters. IE: "A B 1 C D E"
				// Parse through the parameters and populate NewParamList
				NewParamList = Functions.Parameterize(NewParams);
				if (!((NewParamList.Count == 6) || (NewParamList.Count == 7))) {
					NewParamList = null;
				}
				if (NewParamList == null) {
					m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Invalid Command Syntax."));
				} else {
					// Time to add the new network
					MySqlCommand Cmd = new MySqlCommand("INSERT INTO `" + Config.SQLTablePrefix + "networks` (`name`, `server`, `port`, `password`, `realname`, `nickname`, `altnickname`) VALUES (@netname, @server, @port, @password, @realname, @nickname, @altnickname)", m_ParentServer.MyDBConn.Connection);
					Cmd.Prepare();
					Cmd.Parameters.AddWithValue("@netname", NewParamList[0]);
					Cmd.Parameters.AddWithValue("@server", NewParamList[1]);
					Cmd.Parameters.AddWithValue("@port", Convert.ToInt32(NewParamList[2]));
					if (NewParamList.Count == 7) {
						Cmd.Parameters.AddWithValue("@password", NewParamList[6]);
					} else {
						Cmd.Parameters.AddWithValue("@password", null);
					}
					Cmd.Parameters.AddWithValue("@realname", NewParamList[3]);
					Cmd.Parameters.AddWithValue("@nickname", NewParamList[4]);
					Cmd.Parameters.AddWithValue("@altnickname", NewParamList[5]);
					int ReturnVal = Cmd.ExecuteNonQuery();
					if (ReturnVal == 1) {
						m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Successfully added network: " + NewParamList[0]));
						m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "     Use \"!list networks\" to see more info."));
					} else {
						m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Something went wrong. :("));
					}
				}
			}
		}

		private void HandleAddChannel(string Sender, string[] Params) {
			// !add channel 1 #channel [Password]
			List<string> NewParamList = new List<string>();
			string NewParams = Functions.CombineAfterIndex(Params, " ", 2);
			if (NewParams.Length > 4) {				// Shortest possible parameters. IE: "1 #A"
				// Parse through the parameters and populate NewParamList
				NewParamList = Functions.Parameterize(NewParams);
				if (!((NewParamList.Count == 2) || (NewParamList.Count == 3))) {
					NewParamList = null;
				}
				if (NewParamList == null) {
					m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Invalid Command Syntax."));
				} else {
					// Time to add the new network
					MySqlCommand Cmd = new MySqlCommand("SELECT * FROM `" + Config.SQLTablePrefix + "networks` WHERE id = @netid", m_ParentServer.MyDBConn.Connection);
					Cmd.Prepare();
					Cmd.Parameters.AddWithValue("@netid", NewParamList[0]);
					int MatchingNetworkIDs = Convert.ToInt32(Cmd.ExecuteScalar());
					if (MatchingNetworkIDs > 0) {
						Cmd = new MySqlCommand("INSERT INTO `" + Config.SQLTablePrefix + "channels` (`networkid`, `name`, `password`, `autorejoin`, `statsenabled`, `lastscanid`) VALUES (@netid, @channame, @password, @autorejoin, @statsenabled, @lastscanid)", m_ParentServer.MyDBConn.Connection);
						Cmd.Prepare();
						Cmd.Parameters.AddWithValue("@netid", NewParamList[0]);
						Cmd.Parameters.AddWithValue("@channame", NewParamList[1]);
						if (NewParamList.Count == 3) {
							Cmd.Parameters.AddWithValue("@password", NewParamList[2]);
						} else {
							Cmd.Parameters.AddWithValue("@password", null);
						}
						Cmd.Parameters.AddWithValue("@autorejoin", 1);
						Cmd.Parameters.AddWithValue("@statsenabled", 1);
						Cmd.Parameters.AddWithValue("@lastscanid", 0);
						int ReturnVal = Cmd.ExecuteNonQuery();
						if (ReturnVal == 1) {
							m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Successfully added channel: " + NewParamList[1]));
							m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "     Use \"!list channels\" to see more info."));
						} else {
							m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Something went wrong. :("));
						}
					} else {
						m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Invalid Network ID. Use \"!list networks\" to see a list of networks and their IDs."));
					}
				}
			}
		}

		private void HandleDeleteNetwork(string Sender, string[] Params) {

		}

		private void HandleDeleteChannel(string Sender, string[] Params) {

		}

		public void HandleNotice(string Sender, string[] Params) {
			if (IRCFunctions.GetNickFromHostString(Sender) == "NickServ") {
				// If it's NickServ and it looks like an identify request, then identify.
				if ((Array.IndexOf(Params, "nickname") > 0) && (Array.IndexOf(Params, "is") > 0) && ((Array.IndexOf(Params, "registered") > 0) || (Array.IndexOf(Params, "registered.") > 0))) {
					m_ParentServer.Send(IRCFunctions.PrivMsg("NickServ", "identify " + Config.NickPass));
				}
			}
		}

		public void HandleChannels(string Sender, string[] Params) {
			foreach (Server CurServer in m_ParentServer.Servers) {
				m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "Channels on " + CurServer.Hostname + ":"));
				foreach (KeyValuePair<string, Channel> CurChannel in CurServer.Channels) {
					if (CurChannel.Value.LastMessageTime == null) {
						m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "     " + CurChannel.Value.Name + " - Never"));
					} else {
						TimeSpan tempTS = DateTime.Now.Subtract(CurChannel.Value.LastMessageTime);
						m_ParentServer.Send(IRCFunctions.PrivMsg(IRCFunctions.GetNickFromHostString(Sender), "     " + CurChannel.Value.Name + " - " + Functions.MillisecondsToHumanReadable(tempTS.TotalMilliseconds) + " ago: " + CurChannel.Value.LastMessageText));
					}
				}
			}
		}

	}
}
