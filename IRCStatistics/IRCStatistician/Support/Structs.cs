using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRCStatistician.Support {
	public struct LogFileRow {
		public int Id;
		public string Filename;
		public string ProductVer;
		public long LastReadSize;
	}
}
