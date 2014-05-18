using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRCShared;

namespace IRCStatistician.Support {
	public struct LogFileRow {
		public int Id;
		public string Filename;
		public string ProductVer;
		public long LastReadSize;
	}

	public struct Topic {
		public DateTime Changed;
		public Sender Sender;
		public string Text;
	}
	public struct Mode {
		public DateTime Set;
		public Sender Sender;
		public string Modes;
	}

}
