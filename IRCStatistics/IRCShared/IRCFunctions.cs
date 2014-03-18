﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace IRCShared {
	public static class IRCFunctions {

		public static string Join(string Channel, string Password) {
			if (Password == "") {
				return "JOIN " + Channel;
			} else {
				return "JOIN " + Channel + " " + Password;
			}
		}

		public static string PrivMsg(string To, string Message) {
			return "PRIVMSG " + To + " :" + Message;
		}

		public static string CTCPVersionReply(string Username) {
			return "NOTICE " + Username + " :\x01VERSION IRC Statistics Bot v" + Assembly.GetExecutingAssembly().GetName().Version + "\x01";
		}

		public static string CTCPTimeReply(string Username) {
			// Not UTC because it should reflect local time.
			return "NOTICE " + Username + " :\x01TIME " + DateTime.Now.ToString("ddd MMM dd HH:mm:ss yyyy") + "\x01";
		}

		public static string CTCPPingReply(string Username, string Timestamp) {
			return "NOTICE " + Username + " :\x01PING " + Timestamp + "\x01";
		}


		public static string GetNickFromHostString(string HostString) {
			if (HostString.Contains("!") && HostString.Contains("@")) {
				string[] TempSplit = HostString.Split("!".ToCharArray());
				return TempSplit[0];
			} else {
				return HostString;
			}

		}

		public static string GetUserFromHostString(string HostString, bool RemoveTilde = true) {
			if (HostString.Contains("!") && HostString.Contains("@")) {
				string[] TempSplit = HostString.Split("!".ToCharArray());
				TempSplit = TempSplit[1].Split("@".ToCharArray());
				if (RemoveTilde) {
					if (TempSplit[0].Substring(0, 1) == "~") { TempSplit[0] = TempSplit[0].Substring(1); }
				}
				return TempSplit[0];
			} else {
				return string.Empty;
			}
		}

		public static string GetHostFromHostString(string HostString) {
			if (HostString.Contains("!") && HostString.Contains("@")) {
				string[] TempSplit = HostString.Split("@".ToCharArray());
				return TempSplit[1];
			} else {
				return string.Empty;
			}
		}

	}
}
