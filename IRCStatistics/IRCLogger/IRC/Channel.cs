using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using IRCLogger.Support;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using IRCShared;

namespace IRCLogger.IRC {
	public class Channel {
		private Server m_ParentServer;
		private string m_Name;
		private string m_Password;
		private int m_Network;
		private int m_ID;
		private bool m_StatsEnabled;
		private bool m_AutoRejoin;
		private List<string> m_Users = new List<string>();
		private DateTime m_LastMessageTime;
		private string m_LastMessageText;

		public string Name { get { return m_Name; } set { m_Name = value; } }
		public string Password {
			get { return m_Password; }
			set {
				m_Password = value;
				// Code for updating the database?
			}
		}
		public bool StatsEnabled {
			get { return m_StatsEnabled; }
			set {
				m_StatsEnabled = value;
				// Code for updating the database?
			}
		}
		public int NetworkID { get { return m_Network; } set { m_Network = value; } }
		public int ID { get { return m_ID; } set { m_ID = value; } }
		public bool AutoRejoin { get { return m_AutoRejoin; } set { m_AutoRejoin = value; } }
		public DateTime LastMessageTime { get { return m_LastMessageTime; } set { m_LastMessageTime = value; } }
		public string LastMessageText { get { return m_LastMessageText; } set { m_LastMessageText = value; } }

		public Channel(Server ParentServer) {
			m_ParentServer = ParentServer;
		}

		public void JoinMe() {
			AppLog.WriteLine(3, "IRC", "Joining " + m_Name);
			m_ParentServer.Send(IRCFunctions.Join(m_Name, m_Password));
		}

		public void Join(string Sender) {
			if (IRCFunctions.GetNickFromHostString(Sender) == m_ParentServer.Nick) {
				JoinMe();
			}
			if (!m_Users.Contains(IRCFunctions.GetNickFromHostString(Sender).ToLower())) {
				m_Users.Add(IRCFunctions.GetNickFromHostString(Sender).ToLower());
			} else {
				AppLog.WriteLine(1, "ERROR", "User " + Sender + " tried to join channel. IRC let it happen, but they're already in my userlist. Why?");
			}
			LastMessageText = "JOIN: " + Sender; LastMessageTime = DateTime.UtcNow;
		}

		public void Part(string Sender, string Message) {
			if (m_Users.Contains(IRCFunctions.GetNickFromHostString(Sender).ToLower())) {
				m_Users.Remove(IRCFunctions.GetNickFromHostString(Sender).ToLower());
			}
			LastMessageText = "PART: " + Sender; LastMessageTime = DateTime.UtcNow;
		}

		public void Kick(string Sender, string PersonKicked, string KickMsg) {
			if (m_Users.Contains(PersonKicked.ToLower())) {
				m_Users.Remove(PersonKicked.ToLower());
			}
			LastMessageText = "KICK: " + Sender + " kicked " + PersonKicked + " (" + KickMsg + ")"; LastMessageTime = DateTime.UtcNow;
			if (PersonKicked == m_ParentServer.Nick) {
				if (m_AutoRejoin) {
					// If I was kicked and autorejoin is enabled.
					m_ParentServer.Send(IRCFunctions.Join(m_Name, m_Password));
				}
			}
		}

		public void Quit(string Sender, string Message) {
			if (m_Users.Contains(IRCFunctions.GetNickFromHostString(Sender).ToLower())) {
				m_Users.Remove(IRCFunctions.GetNickFromHostString(Sender).ToLower());
				LastMessageText = "QUIT: " + Sender; LastMessageTime = DateTime.UtcNow;
			}
		}

		public void Message(string Sender, string Message) {
			LastMessageText = "PRIVMSG: " + Sender + ": " + Message; LastMessageTime = DateTime.UtcNow;
		}

		public void Action(string Sender, string Message) {
			LastMessageText = "ACTION: " + Sender + ": " + Message; LastMessageTime = DateTime.UtcNow;
		}

		public void Topic(string Sender, string Message) {
			LastMessageText = "TOPIC: " + Sender + ": " + Message; LastMessageTime = DateTime.UtcNow;
		}

		public void Mode(string Sender, string Message) {
			LastMessageText = "MODE: " + Sender + ": " + Message; LastMessageTime = DateTime.UtcNow;
		}

		public void Names(string[] NameArr) {
			foreach (string CurName in NameArr) {
				string FixedName = CurName;
				string FirstLetter = CurName.Substring(0, 1);
				// ~ for owners – to get this, you need to be +q in the channel
				// & for admins – to get this, you need to be +a in the channel
				// @ for full operators – to get this, you need to be +o in the channel
				// % for half operators – to get this, you need to be +h in the channel
				// + for voiced users – to get this, you need to be +v in the channel
				if ((FirstLetter == "~") || (FirstLetter == "&") || (FirstLetter == "@") || (FirstLetter == "%") || (FirstLetter == "+")) {
					FixedName = CurName.Substring(1);
				}
				if (!m_Users.Contains(FixedName.ToLower())) {
					m_Users.Add(FixedName.ToLower());
				}
			}
		}

		public void Nick(string Sender, string NewNick) {
			if (m_Users.Contains(IRCFunctions.GetNickFromHostString(Sender).ToLower())) {
				m_Users.Remove(IRCFunctions.GetNickFromHostString(Sender).ToLower());
				if (!m_Users.Contains(NewNick.ToLower())) {
					m_Users.Add(NewNick.ToLower());
				} else {
					AppLog.WriteLine(1, "ERROR", "User " + Sender + " tried to change nick to " + NewNick + ". IRC let it happen, but it's still in my userlist. Why?");
				}
			}
			LastMessageText = "NICK: " + Sender + " => " + NewNick; LastMessageTime = DateTime.UtcNow;
		}

	}
}