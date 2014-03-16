using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRCStatistician.LogMgmt {
	class LogWatcher {
		//public DBConnection MyDBConn;
		public List<string> Channels;
		private string m_ServerHost;
		private int m_ServerPort;
		private string m_ServerPass;
		private string m_RealName;
		private string m_Nick;
		private string m_AltNick;
		private string m_Network;
		private int m_ID;

		public string Hostname { get { return m_ServerHost; } set { m_ServerHost = value; } }
		public int Port { get { return m_ServerPort; } set { m_ServerPort = value; } }
		public string Pass { get { return m_ServerPass; } set { m_ServerPass = value; } }
		public string RealName { get { return m_RealName; } set { m_RealName = value; } }
		public string Nick { get { return m_Nick; } set { m_Nick = value; } }
		public string AltNick { get { return m_AltNick; } set { m_AltNick = value; } }
		public string Network { get { return m_Network; } set { m_Network = value; } }
		public int ID { get { return m_ID; } set { m_ID = value; } }

		public LogWatcher() {

		}

	}
}
