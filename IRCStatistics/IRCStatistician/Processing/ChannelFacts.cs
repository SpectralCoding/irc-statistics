using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRCStatistician.Support;
using IRCShared;

namespace IRCStatistician.Processing {
	class ChannelFacts {
		public uint Joins;
		public uint Parts;
		public uint Kicks;
		public uint Quits;
		public uint Messages;
		public uint Actions;
		public uint NickChanges;
		public uint TopicsSet;
		public uint ModesSet;
	}
}
