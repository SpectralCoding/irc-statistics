using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using IRCLogger.Support;
using System.IO;
using System.Threading;
using IRCShared;

namespace IRCLogger.IRC {
	public class ServerComm {
		private ManualResetEvent ConnectDone = new ManualResetEvent(false);
		private ManualResetEvent SendDone = new ManualResetEvent(false);
		private ManualResetEvent ReceiveDone = new ManualResetEvent(false);
		private String Response = String.Empty;
		private Socket ClientSock;
		private Server ParentServer;

		public void StartClient(Server i_ParentServer) {
			if ((ClientSock != null) && (ClientSock.Connected)) {
				Close(ClientSock);
			}
			ParentServer = i_ParentServer;
			AppLog.WriteLine(5, "STATUS", "Entering IRC.Server.StartClient().");
			int i = 0;
			IPAddress IPAddr;
			if (int.TryParse(ParentServer.Hostname.Substring(0, 1), out i)) {
				IPAddr = IPAddress.Parse(ParentServer.Hostname);
			} else {
				IPHostEntry ipHostInfo = Dns.GetHostEntry(ParentServer.Hostname);
				IPAddr = ipHostInfo.AddressList[0];
			}
			IPEndPoint ConnectSock = new IPEndPoint(IPAddr, ParentServer.Port);
			ClientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			AppLog.WriteLine(5, "STATUS", "Initiating Connection.");
			ClientSock.BeginConnect(ConnectSock, new AsyncCallback(OnConnect), ClientSock);
		}

		private void OnConnect(IAsyncResult AsyncResult) {
			Socket ServerSock = (Socket)AsyncResult.AsyncState;
			ServerSock.EndConnect(AsyncResult);
			AppLog.WriteLine(5, "STATUS", "Connected.");
			ConnectDone.Set();
			IRCComm ServerComm = new IRCComm();
			ServerComm.ParentServerComm = this;
			ServerComm.WorkSocket = ServerSock;
			AppLog.WriteLine(5, "STATUS", "Waiting for data...");
			ServerComm.WorkSocket.BeginReceive(ServerComm.Buffer, 0, IRCComm.BufferSize, 0, new AsyncCallback(OnDataReceived), ServerComm);
			if (ParentServer.Pass != "") {
				Send("PASS " + ParentServer.Pass);
				Send("USER " + ParentServer.Nick + " 8 * : " + ParentServer.RealName);
				Send("NICK " + ParentServer.Nick);
			} else {
				Send("USER " + ParentServer.Nick + " 8 * : " + ParentServer.RealName);
				Send("NICK " + ParentServer.Nick);
			}
		}

		private void OnDataReceived(IAsyncResult AsyncResult) {
			IRCComm ServerComm = (IRCComm)AsyncResult.AsyncState;
			Socket SockHandler = ServerComm.WorkSocket;
			try {
				int BytesRead = SockHandler.EndReceive(AsyncResult);
				if (BytesRead > 0) {
					char[] TempByteArr = new char[BytesRead];
					int ReceivedLen = Encoding.UTF8.GetChars(ServerComm.Buffer, 0, BytesRead, TempByteArr, 0);
					char[] ReceivedCharArr = new char[ReceivedLen];
					Array.Copy(TempByteArr, ReceivedCharArr, ReceivedLen);
					String ReceivedData = new String(ReceivedCharArr);
					ServerComm.StringBuffer += ReceivedData;
					if (Functions.OccurancesInString(ServerComm.StringBuffer, "\n") > 0) {
						String[] splitIncommingData = ServerComm.StringBuffer.Split("\n".ToCharArray());
						for (int i = 0; i < splitIncommingData.Length; i++) {
							if (splitIncommingData[i].Contains("\r")) {
								if (splitIncommingData[i].Length + 1 > ServerComm.StringBuffer.Length) {
									ServerComm.StringBuffer = ServerComm.StringBuffer.Remove(0, splitIncommingData[i].Length);
								} else {
									ServerComm.StringBuffer = ServerComm.StringBuffer.Remove(0, splitIncommingData[i].Length + 1);
								}
								ParentServer.ParseRawLine(splitIncommingData[i].Substring(0, splitIncommingData[i].Length - 1));
							}
						}
					}
					SockHandler.BeginReceive(ServerComm.Buffer, 0, IRCComm.BufferSize, 0, new AsyncCallback(OnDataReceived), ServerComm);
				} else {
					Close(SockHandler);
				}
			} catch (SocketException Se) {
				if (Se.ErrorCode == 10054) {
					Close(SockHandler);
				}
			}
		}

		public bool Send(string DataToSend) {
			if (ClientSock.Connected) {
				AppLog.WriteLine(5, "DATA", ParentServer.Network + ": OUT: " + DataToSend);
				DataToSend += "\n";
				byte[] NewData = new byte[DataToSend.Length];
				ClientSock.BeginSend(Encoding.UTF8.GetBytes(DataToSend), 0, DataToSend.Length, 0, new AsyncCallback(OnSendComplete), ClientSock);
				return true;
			} else {
				ParentServer.Connect();
				return false;
			}
		}

		private void OnSendComplete(IAsyncResult AsyncResult) {
			Socket ServerSock = (Socket)AsyncResult.AsyncState;
			int BytesSent = ServerSock.EndSend(AsyncResult);
			SendDone.Set();
		}

		public void Close(Socket SockHandler) {
			try {
				AppLog.WriteLine(5, "CONN", "Connection Closed: " + ParentServer.Network);
				SockHandler.Shutdown(SocketShutdown.Both);
				SockHandler.Close();
				ParentServer.NetworkLog.CloseLog();
				ParentServer.Channels = null;
				ParentServer.BotCommands = null;
				ParentServer.ConnectionWatchdog.Destroy();
				ParentServer.ConnectionWatchdog = null;
				AppLog.WriteLine(5, "CONN", "Everything cleaned up, sleeping for 5 sec until reconnect attempt...");
				Thread.Sleep(5000);
				ParentServer.Connect();
			} catch (Exception) { }
		}
	}
}
