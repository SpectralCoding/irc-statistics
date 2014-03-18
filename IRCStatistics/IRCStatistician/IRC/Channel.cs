using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRCStatistician.IRC {
	class Channel {
		private string m_Name;

		public string Name { get { return m_Name; } set { m_Name = value; } }

		public Channel(string ChanName) {
			m_Name = ChanName;
		}


		public void Join(string Sender) {
		}

		public void Part(string Sender, string Message) {
		}

		public void Kick(string Sender, string PersonKicked, string KickMsg) {
		}

		public void Quit(string Sender, string Message) {
		}

		public void Message(string Sender, string Message) {
		}

		public void Action(string Sender, string Message) {
		}

		public void Topic(string Sender, string Message) {
		}

		public void Mode(string Sender, string Message) {
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
				//if (!m_Users.Contains(FixedName.ToLower())) {
				//	m_Users.Add(FixedName.ToLower());
				//}
			}
		}

		public void Nick(string Sender, string NewNick) {

		}

	}
}
