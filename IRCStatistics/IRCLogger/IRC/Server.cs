using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using IRCLogger.Support;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using IRCShared;
using IRCLogger.Logging;

namespace IRCLogger.IRC {
	public class Server {
		public DBConnection MyDBConn;
		public List<Server> Servers;
		public Dictionary<string, Channel> Channels;
		public NetworkLog NetworkLog;
		public BotCommands BotCommands;
		public ConnectionWatchdog ConnectionWatchdog;
		private string m_ServerHost;
		private int m_ServerPort;
		private string m_ServerPass;
		private string m_RealName;
		private string m_Nick;
		private string m_AltNick;
		private string m_Network;
		private int m_ID;
		private ServerComm m_ServerComm;


		public string Hostname { get { return m_ServerHost; } set { m_ServerHost = value; } }
		public int Port { get { return m_ServerPort; } set { m_ServerPort = value; } }
		public string Pass { get { return m_ServerPass; } set { m_ServerPass = value; } }
		public string RealName { get { return m_RealName; } set { m_RealName = value; } }
		public string Nick { get { return m_Nick; } set { m_Nick = value; } }
		public string AltNick { get { return m_AltNick; } set { m_AltNick = value; } }
		public string Network { get { return m_Network; } set { m_Network = value; } }
		public int ID { get { return m_ID; } set { m_ID = value; } }

		public Server(List<Server> ServerList) {
			Servers = ServerList;
		}

		public void Connect() {
			MyDBConn = new DBConnection();
			Channels = new Dictionary<string, Channel>(StringComparer.InvariantCultureIgnoreCase);
			MyDBConn.Connect(Config.SQLServerHost, Config.SQLServerPort, Config.SQLUsername, Config.SQLPassword, Config.SQLDatabase);
			BotCommands = new BotCommands(this);
			AppLog.WriteLine(3, "STATUS", "Connecting to Server ID " + m_ID);
			NetworkLog = new NetworkLog(ID, Network);
			m_ServerComm = new IRC.ServerComm();
			m_ServerComm.StartClient(this);
		}

		public void ParseRawLine(string LineToParse) {
			AppLog.WriteLine(5, "DATA", m_Network + ": IN: " + LineToParse);
			NetworkLog.WriteLine(LineToParse);
			if (LineToParse.Substring(0, 1) == ":") {
				LineToParse = LineToParse.Substring(1);
				string[] ParameterSplit = LineToParse.Split(" ".ToCharArray(), 3, StringSplitOptions.RemoveEmptyEntries);
				string Sender = ParameterSplit[0];
				string Command = ParameterSplit[1];
				string Parameters = ParameterSplit[2];
				// Even though we've logged it, we still need to send it down
				// the line for stuff like PING, CTCP, joining channels, etc.
				Parse(Sender, Command, Parameters);
			} else {
				string[] Explode = LineToParse.Split(" ".ToCharArray());
				switch (Explode[0].ToUpper()) {
					case "PING":
						m_ServerComm.Send("PONG " + Explode[1]);
						break;
				}
			}
		}

		public void Parse(string Sender, string Command, string Parameters) {
			//AppLog.WriteLine(5, "DEBUG", "\tSender: " + Sender);
			//AppLog.WriteLine(5, "DEBUG", "\tCommand: " + Command);
			//AppLog.WriteLine(5, "DEBUG", "\tParameters: " + Parameters);
			if (ConnectionWatchdog != null) { ConnectionWatchdog.Reset(); }
			string[] ParamSplit = Parameters.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			switch (Command.ToUpper()) {
				case "376":
					// RAW: 376 - RPL_ENDOFMOTD - ":End of /MOTD command"
					ConnectionWatchdog = new ConnectionWatchdog(this);
					JoinChannels();
					break;
				case "353":
					// 353 - RPL_NAMREPLY - "<channel> :[[@|+]<nick> [[@|+]<nick> [...]]]"
					for (int i = 0; i < ParamSplit.Length; i++) {
						if (ParamSplit[i].Substring(0, 1) == "#") {
							// Skip to where we see the channel name.
							string Chan = ParamSplit[i];
							ParamSplit[i + 1] = ParamSplit[i + 1].Substring(1);
							string[] NameArr = new string[ParamSplit.Length - i - 1];
							Array.Copy(ParamSplit, i + 1, NameArr, 0, ParamSplit.Length - i - 1);
							Channels[ParamSplit[i]].Names(NameArr);
							break;
						}
					}
					break;
				case "433":
					//433 - ERR_NICKNAMEINUSE - "<nick> :Nickname is already in use"
					Send("NICK " + m_AltNick);
					m_Nick = m_AltNick;
					break;
				case "470":
					// Channel Forward
					// :adams.freenode.net 470 SomethingKewl #windows ##windows :Forwarding to another channel
					Channel OldChannel;
					if (Channels.ContainsKey(ParamSplit[1])) {
						OldChannel = Channels[ParamSplit[1]];
						OldChannel.Name = ParamSplit[2];
						// Should we really remove the old channel? Does it hurt?
						//m_Channels.Remove(ParamSplit[1]);
						Channels.Add(OldChannel.Name, OldChannel);
						// TODO: add code here to check for old channel and rename it.
					} else {
						// Conceivably this could happen if you were forcejoined to a channel which then got moved.
						OldChannel = new Channel(this);
						OldChannel.Name = ParamSplit[2];
						OldChannel.StatsEnabled = true;
						throw new Exception("This should never happen. How is this happening? Case 470: Else");
					}
					break;
				case "JOIN":
					if (ParamSplit[0].Contains(":")) {
						// Fix because some IRCds send "JOIN :#channel" instead of "JOIN #channel"
						ParamSplit[0] = ParamSplit[0].Substring(1);
					}
					Channels[ParamSplit[0]].Join(Sender);
					break;
				case "PART":
					if (ParamSplit.Length >= 2) {
						string PartMsg = Parameters.Substring(Parameters.IndexOf(":") + 1);
						if (PartMsg.Length == 0) {
							Channels[ParamSplit[0]].Part(Sender, String.Empty);
						} else {
							if ((PartMsg.Substring(0, 1) == "\"") && (PartMsg.Substring(PartMsg.Length - 1, 1) == "\"")) {
								PartMsg = PartMsg.Substring(1, PartMsg.Length - 2);
							}
						}
						Channels[ParamSplit[0]].Part(Sender, PartMsg);
					} else {
						Channels[ParamSplit[0]].Part(Sender, String.Empty);
					}
					break;
				case "KICK":
					Channels[ParamSplit[0]].Kick(Sender, ParamSplit[1], Functions.CombineAfterIndex(ParamSplit, " ", 2).Substring(1));
					break;
				case "INVITE":
					// TODO: Not sure how we want to handle this.
					break;
				case "NICK":
					if (IRCFunctions.GetNickFromHostString(Sender) == m_Nick) {
						m_Nick = Parameters.Substring(1);
					}
					foreach (KeyValuePair<string, Channel> CurKVP in Channels) {
						Channels[CurKVP.Key].Nick(Sender, Parameters.Substring(1));
					}
					BotCommands.CheckAdminChange(Sender, Parameters.Substring(1));
					break;
				case "QUIT":
					foreach (KeyValuePair<string, Channel> CurKVP in Channels) {
						Channels[CurKVP.Key].Quit(Sender, Parameters.Substring(1));
					}
					break;
				case "TOPIC":
					string Topic = Parameters.Substring(Parameters.IndexOf(":") + 1);
					Channels[ParamSplit[0]].Topic(Sender, Topic);
					break;
				case "MODE":
					if (ParamSplit[0].Substring(0, 1) == "#") {
						// Is a channel mode
						Channels[ParamSplit[0]].Mode(Sender, Functions.CombineAfterIndex(ParamSplit, " ", 1));
					} else {
						// Is not going to a channel. Probably me?
					}
					break;
				case "PRIVMSG":
					string MsgText = Parameters.Substring(Parameters.IndexOf(":") + 1);
					if (ParamSplit[0].Substring(0, 1) == "#") {
						// Is going to a channel
						if (MsgText.Substring(0, 1) == "\x1") {
							// If this is a special PRIVMSG, like an action or CTCP
							MsgText = MsgText.Substring(1, MsgText.Length - 2);
							string[] PrivMsgSplit = MsgText.Split(" ".ToCharArray(), 2);
							switch (PrivMsgSplit[0].ToUpper()) {
								case "ACTION":
									Channels[ParamSplit[0]].Action(Sender, PrivMsgSplit[1]);
									break;
								// Maybe other stuff goes here like channel wide CTCPs?
							}
						} else {
							// If this is just a normal PRIVMSG.
							Channels[ParamSplit[0]].Message(Sender, MsgText);
						}
					} else {
						// Is not going to a channel. Probably just me?
						if (MsgText.Substring(0, 1) == "\x1") {
							// If this is a special PRIVMSG, like an action or CTCP
							MsgText = MsgText.Substring(1, MsgText.Length - 2);
							string[] PrivMsgSplit = MsgText.Split(" ".ToCharArray(), 2);
							switch (PrivMsgSplit[0].ToUpper()) {
								case "ACTION":
									// Not sure what to do here...
									break;
								case "VERSION":
									Send(IRCFunctions.CTCPVersionReply(IRCFunctions.GetNickFromHostString(Sender)));
									break;
								case "TIME":
									Send(IRCFunctions.CTCPTimeReply(IRCFunctions.GetNickFromHostString(Sender)));
									break;
								case "PING":
									Send(IRCFunctions.CTCPPingReply(IRCFunctions.GetNickFromHostString(Sender), PrivMsgSplit[1]));
									break;
							}
						} else {
							// Private Message directly to me.
							string[] MsgSplitPrv = MsgText.Split(" ".ToCharArray());
							BotCommands.HandlePM(Sender, MsgSplitPrv);
						}
					}
					break;
				case "NOTICE":
					// Needed for NickServ stuff
					string[] MsgSplitNtc = Parameters.Substring(Parameters.IndexOf(":") + 1).Split(" ".ToCharArray());
					BotCommands.HandleNotice(Sender, MsgSplitNtc);
					break;
			}
		}

		public void JoinChannels() {
			AppLog.WriteLine(1, "CONN", "Fully connected to " + m_ServerHost);
			MySqlCommand Cmd = new MySqlCommand("SELECT * FROM " + Config.SQLTablePrefix + "channels WHERE networkid = @networkid", MyDBConn.Connection);
			Cmd.Prepare();
			Cmd.Parameters.AddWithValue("@networkid", m_ID);
			MySqlDataReader DataReader = Cmd.ExecuteReader();
			DataTable ChannelTable = new DataTable();
			ChannelTable.Load(DataReader);
			DataReader.Close();
			bool ChannelExists;
			foreach (DataRow CurChannel in ChannelTable.Rows) {
				ChannelExists = false;
				foreach (KeyValuePair<string, Channel> CurKVP in Channels) {
					if (CurKVP.Value.ID == Convert.ToInt32(CurChannel["id"])) {
						ChannelExists = true;
					}
				}
				if (!ChannelExists) {
					Channel TempChan = new Channel(this);
					TempChan.Name = CurChannel["name"].ToString();
					TempChan.Password = CurChannel["password"].ToString();
					TempChan.AutoRejoin = Convert.ToBoolean(CurChannel["autorejoin"]);
					TempChan.StatsEnabled = Convert.ToBoolean(CurChannel["statsenabled"]);
					TempChan.ID = Convert.ToInt32(CurChannel["id"]);
					TempChan.NetworkID = Convert.ToInt32(CurChannel["networkid"]);
					Channels.Add(CurChannel["name"].ToString(), TempChan);
					TempChan.JoinMe();
				}
			}
		}
		public bool Send(string DataToSend) {
			return m_ServerComm.Send(DataToSend);
		}

		public bool SendPing() {
			return Send("PING " + m_ServerHost);
		}
	}
}
