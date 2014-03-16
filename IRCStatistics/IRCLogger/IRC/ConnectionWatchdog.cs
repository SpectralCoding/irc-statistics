using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using IRCShared;
using IRCLogger.Support;

namespace IRCLogger.IRC {
	public class ConnectionWatchdog {
		private Timer m_InitiatePingTimer;
		private Timer m_DetectDisconnectTimer;
		private Server m_IRCServer;

		public ConnectionWatchdog(Server IRCServer) {
			m_IRCServer = IRCServer;
			TimerCallback PingTimerDelegate = new TimerCallback(SendPing);
			m_InitiatePingTimer = new Timer(PingTimerDelegate, m_IRCServer, Config.InactivityTimeout * 1000, Timeout.Infinite);
			AppLog.WriteLine(5, "TIMR", "Ping Timer Started...");
		}

		public void Destroy() {
			if (m_DetectDisconnectTimer != null) { m_DetectDisconnectTimer.Dispose(); }
			if (m_InitiatePingTimer != null) { m_InitiatePingTimer.Dispose(); }
		}

		public void Reset() {
			//AppLog.WriteLine(5, "TIMR", "Ping Timer Reset...");
			if (m_DetectDisconnectTimer != null) {
				AppLog.WriteLine(5, "TIMR", "Disconnect Timer Destroyed...");
				m_DetectDisconnectTimer.Dispose();
				m_DetectDisconnectTimer = null;
			}
			m_InitiatePingTimer.Change(Config.InactivityTimeout * 1000, Timeout.Infinite);
		}

		public void SendPing(object IRCServer) {
			AppLog.WriteLine(5, "TIMR", "No activity after " + Config.InactivityTimeout + " seconds. Ping Timer Executed!");
			bool StillConnected = m_IRCServer.SendPing();
			if (StillConnected == true) {
				TimerCallback DisconnectTimerDelegate = new TimerCallback(Disconnected);
				m_DetectDisconnectTimer = new Timer(DisconnectTimerDelegate, IRCServer, Config.PingTimeout * 1000, Timeout.Infinite);
				AppLog.WriteLine(5, "TIMR", "Disconnect Timer Started...");
				//m_InitiatePingTimer.Change(Config.InactivityTimeout, Timeout.Infinite);
			}
		}

		public void Disconnected(object IRCServer) {
			AppLog.WriteLine(5, "TIMR", "No ping response or activity after " + Config.PingTimeout + " seconds. Disconnect Timer Executed...");
			AppLog.WriteLine(5, "TIMR", "Disconnect Timer Destroyed...");
			m_DetectDisconnectTimer.Dispose();
			AppLog.WriteLine(5, "TIMR", "Initiating reconnection...");
			m_IRCServer.Connect();
		}
		

	}
}
