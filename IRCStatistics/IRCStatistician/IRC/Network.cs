using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRCShared;
using IRCStatistician.Support;

namespace IRCStatistician.IRC {
	class Network {
		public Dictionary<string, Channel> Channels;
		private string m_ServerHost;
		private int m_ServerPort;
		private string m_ServerPass;
		private string m_RealName;
		private string m_Nick;
		private string m_AltNick;
		private string m_Name;
		private int m_Id;

		public string Hostname { get { return m_ServerHost; } set { m_ServerHost = value; } }
		public int Port { get { return m_ServerPort; } set { m_ServerPort = value; } }
		public string Pass { get { return m_ServerPass; } set { m_ServerPass = value; } }
		public string RealName { get { return m_RealName; } set { m_RealName = value; } }
		public string Nick { get { return m_Nick; } set { m_Nick = value; } }
		public string AltNick { get { return m_AltNick; } set { m_AltNick = value; } }
		public string Name { get { return m_Name; } set { m_Name = value; } }
		public int Id { get { return m_Id; } set { m_Id = value; } }

		public Network() {

		}
		public void Parse(DateTime Timestamp, uint TimeframeID, Sender Sender, string Command, string Parameters) {
			string[] ParamSplit = Parameters.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			switch (Command.ToUpper()) {
				case "353":
					// 353 - RPL_NAMREPLY - "<channel> :[[@|+]<nick> [[@|+]<nick> [...]]]"
					for (int i = 0; i < ParamSplit.Length; i++) {
						if (ParamSplit[i].Substring(0, 1) == "#") {
							// Skip to where we see the channel name.
							string Chan = ParamSplit[i].ToLower();
							ParamSplit[i + 1] = ParamSplit[i + 1].Substring(1);
							string[] NameArr = new string[ParamSplit.Length - i - 1];
							Array.Copy(ParamSplit, i + 1, NameArr, 0, ParamSplit.Length - i - 1);
							Channels[ParamSplit[i]].Names(NameArr);
							break;
						}
					}
					break;
				case "470":
					// Channel Forward
					// :adams.freenode.net 470 SomethingKewl #windows ##windows :Forwarding to another channel
					// Do we need to do anything here for stats? More code in IRCLogger.Server.Parse()
				case "JOIN":
					ParamSplit[0] = ParamSplit[0].ToLower();
					if (ParamSplit[0].Contains(":")) {
						// Fix because some IRCds send "JOIN :#channel" instead of "JOIN #channel"
						ParamSplit[0] = ParamSplit[0].Substring(1).ToLower();
					}
					Channels[ParamSplit[0]].Join(Timestamp, TimeframeID, Sender);
					break;
				case "PART":
					ParamSplit[0] = ParamSplit[0].ToLower();
					if (ParamSplit.Length >= 2) {
						string PartMsg = Parameters.Substring(Parameters.IndexOf(":") + 1);
						if (PartMsg.Length == 0) {
							Channels[ParamSplit[0]].Part(Timestamp, TimeframeID, Sender, String.Empty);
						} else {
							if ((PartMsg.Substring(0, 1) == "\"") && (PartMsg.Substring(PartMsg.Length - 1, 1) == "\"")) {
								PartMsg = PartMsg.Substring(1, PartMsg.Length - 2);
							}
						}
						Channels[ParamSplit[0]].Part(Timestamp, TimeframeID, Sender, PartMsg);
					} else {
						Channels[ParamSplit[0]].Part(Timestamp, TimeframeID, Sender, String.Empty);
					}
					break;
				case "KICK":
					ParamSplit[0] = ParamSplit[0].ToLower();
					Channels[ParamSplit[0]].Kick(Timestamp, TimeframeID, Sender, ParamSplit[1], Functions.CombineAfterIndex(ParamSplit, " ", 2).Substring(1));
					break;
				case "INVITE":
					// TODO: Not sure how we want to handle this.
					break;
				case "NICK":
					if (IRCFunctions.GetNickFromHostString(Sender.SenderStr) == m_Nick) {
						m_Nick = Parameters.Substring(1);
					}
					foreach (KeyValuePair<string, Channel> CurKVP in Channels) {
						Channels[CurKVP.Key].Nick(Timestamp, TimeframeID, Sender, Parameters.Substring(1));
					}
					break;
				case "QUIT":
					foreach (KeyValuePair<string, Channel> CurKVP in Channels) {
						Channels[CurKVP.Key].Quit(Timestamp, TimeframeID, Sender, Parameters.Substring(1));
					}
					break;
				case "TOPIC":
					ParamSplit[0] = ParamSplit[0].ToLower();
					string Topic = Parameters.Substring(Parameters.IndexOf(":") + 1);
					Channels[ParamSplit[0]].Topic(Timestamp, TimeframeID, Sender, Topic);
					break;
				case "MODE":
					ParamSplit[0] = ParamSplit[0].ToLower();
					if (ParamSplit[0].Substring(0, 1) == "#") {
						// Is a channel mode
						Channels[ParamSplit[0]].Mode(Timestamp, TimeframeID, Sender, Functions.CombineAfterIndex(ParamSplit, " ", 1));
					} else {
						// Is not going to a channel. Probably me?
					}
					break;
				case "PRIVMSG":
					ParamSplit[0] = ParamSplit[0].ToLower();
					string MsgText = Parameters.Substring(Parameters.IndexOf(":") + 1);
					if (ParamSplit[0].Substring(0, 1) == "#") {
						// Is going to a channel
						if (MsgText.Substring(0, 1) == "\x1") {
							// If this is a special PRIVMSG, like an action or CTCP
							MsgText = MsgText.Substring(1, MsgText.Length - 2);
							string[] PrivMsgSplit = MsgText.Split(" ".ToCharArray(), 2);
							switch (PrivMsgSplit[0].ToUpper()) {
								case "ACTION":
									Channels[ParamSplit[0]].Action(Timestamp, TimeframeID, Sender, PrivMsgSplit[1]);
									break;
								// Maybe other stuff goes here like channel wide CTCPs?
							}
						} else {
							// If this is just a normal PRIVMSG.
							Channels[ParamSplit[0]].Message(Timestamp, TimeframeID, Sender, MsgText);
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
									//Send(IRCFunctions.CTCPVersionReply(IRCFunctions.GetNickFromHostString(Sender)));
									break;
								case "TIME":
									//Send(IRCFunctions.CTCPTimeReply(IRCFunctions.GetNickFromHostString(Sender)));
									break;
								case "PING":
									//Send(IRCFunctions.CTCPPingReply(IRCFunctions.GetNickFromHostString(Sender), PrivMsgSplit[1]));
									break;
							}
						} else {
							// Private Message directly to me.
							string[] MsgSplitPrv = MsgText.Split(" ".ToCharArray());
							//BotCommands.HandlePM(Sender, MsgSplitPrv);
						}
					}
					break;
				case "NOTICE":
					// Needed for NickServ stuff
					string[] MsgSplitNtc = Parameters.Substring(Parameters.IndexOf(":") + 1).Split(" ".ToCharArray());
					//BotCommands.HandleNotice(Sender, MsgSplitNtc);
					break;
			}
		}

	}
}
