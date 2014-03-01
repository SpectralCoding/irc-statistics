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
	public class IRCComm {
		public Socket WorkSocket = null;
		public const int BufferSize = 1024;
		public byte[] Buffer = new byte[BufferSize];
		public MemoryStream MemoryStream = new MemoryStream();
		public BinaryWriter BinaryWriter;
		public string StringBuffer;
		public ServerComm ParentServerComm;

		public IRCComm() {
			BinaryWriter = new BinaryWriter(MemoryStream);
		}

		public void ResetBuffer() {
			StringBuffer = String.Empty;
			Buffer = new byte[BufferSize];
			MemoryStream = new MemoryStream();
			BinaryWriter = new BinaryWriter(MemoryStream);
		}

	}
}
