using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRCStatistician.Processing;
using IRCStatistician.Support;
using IRCShared;

namespace IRCStatistician.IRC {
	class Channel {
		private string m_Name;
		public SortedDictionary<DateTime, Topic> TopicHistory;
		public SortedDictionary<uint, ChannelFacts> ChannelFacts;
		public Dictionary<Guid, User> Users;

		public string Name { get { return m_Name; } set { m_Name = value; } }

		public Channel(string ChanName) {
			m_Name = ChanName;
			TopicHistory = new SortedDictionary<DateTime, Topic>();
			ChannelFacts = new SortedDictionary<uint, ChannelFacts>();
			Users = new Dictionary<Guid, User>();
		}


		public void Join(DateTime Timestamp, uint TimeframeID, Sender Sender) {
			ChannelFacts CurCF = GetChannelFactFromTimeframe(TimeframeID);
			CurCF.Joins++;
			Guid CurGuid = GetGuid(Sender);
			Users[CurGuid].AddJoin(Timestamp, TimeframeID, Sender);
		}

		public void Part(DateTime Timestamp, uint TimeframeID, Sender Sender, string PartMsg) {
			ChannelFacts CurCF = GetChannelFactFromTimeframe(TimeframeID);
			CurCF.Parts++;
			Guid CurGuid = GetGuid(Sender);
			Users[CurGuid].AddPart(Timestamp, TimeframeID, Sender, PartMsg);
		}

		public void Kick(DateTime Timestamp, uint TimeframeID, Sender Sender, string PersonKicked, string KickMsg) {
			ChannelFacts CurCF = GetChannelFactFromTimeframe(TimeframeID);
			CurCF.Kicks++;
			Guid CurGuid = GetGuid(Sender);
			Users[CurGuid].AddKick(Timestamp, TimeframeID, Sender, PersonKicked, KickMsg);
		}

		public void Quit(DateTime Timestamp, uint TimeframeID, Sender Sender, string QuitMsg) {
			ChannelFacts CurCF = GetChannelFactFromTimeframe(TimeframeID);
			CurCF.Quits++;
			Guid CurGuid = GetGuid(Sender);
			Users[CurGuid].AddQuit(Timestamp, TimeframeID, Sender, QuitMsg);
		}

		public void Message(DateTime Timestamp, uint TimeframeID, Sender Sender, string PrivMsg) {
			ChannelFacts CurCF = GetChannelFactFromTimeframe(TimeframeID);
			CurCF.Messages++;
			Guid CurGuid = GetGuid(Sender);
			Users[CurGuid].AddMessage(Timestamp, TimeframeID, Sender, PrivMsg);
		}

		public void Action(DateTime Timestamp, uint TimeframeID, Sender Sender, string ActionMsg) {
			ChannelFacts CurCF = GetChannelFactFromTimeframe(TimeframeID);
			CurCF.Actions++;
			Guid CurGuid = GetGuid(Sender);
			Users[CurGuid].AddAction(Timestamp, TimeframeID, Sender, ActionMsg);
		}

		public void Topic(DateTime Timestamp, uint TimeframeID, Sender Sender, string TopicText) {
			ChannelFacts CurCF = GetChannelFactFromTimeframe(TimeframeID);
			CurCF.TopicsSet++;
			Guid CurGuid = GetGuid(Sender);
			Users[CurGuid].AddTopic(Timestamp, TimeframeID, Sender, TopicText);
			// Don't add duplicate back-to-back TOPICs as they're likely just from rejoining.
			if (TopicHistory.Count > 0) { if (TopicHistory.Values.Last().Text == TopicText) { return; } }
			Topic Temp = new Topic();
			Temp.Changed = Timestamp;
			Temp.Sender = Sender;
			Temp.Text = TopicText;
			while (TopicHistory.ContainsKey(Timestamp)) { Timestamp = Timestamp.AddMilliseconds(1); }
			TopicHistory.Add(Timestamp, Temp);
		}

		public void Mode(DateTime Timestamp, uint TimeframeID, Sender Sender, string ModeText) {
			ChannelFacts CurCF = GetChannelFactFromTimeframe(TimeframeID);
			CurCF.ModesSet++;
			Guid CurGuid = GetGuid(Sender);
			Users[CurGuid].AddMode(Timestamp, TimeframeID, Sender, ModeText);
		}
		public void Nick(DateTime Timestamp, uint TimeframeID, Sender Sender, string NewNick) {
			ChannelFacts CurCF = GetChannelFactFromTimeframe(TimeframeID);
			CurCF.NickChanges++;
			Guid CurGuid = GetGuid(Sender);
			Users[CurGuid].ChangeNick(Timestamp, TimeframeID, Sender, NewNick);
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

		private Guid GetGuid(Sender Sender) {
			// Will need to add logic for fuzzy matching nicks or something... hmmm.
			// This will definitely be one of the most important functions in the app.
			foreach (KeyValuePair<Guid, User> CurKVP in Users) {
				if (CurKVP.Value.SenderHistory.Count != 0) {
					if (CurKVP.Value.SenderHistory.Values.Last().SenderStr == Sender.SenderStr) {
						return CurKVP.Key;
					}
				}
			}
			// If we're not returning a Guid already we can't find it, so create a user and return it's new Guid.
			Guid TempGuid = Guid.NewGuid();
			Users.Add(TempGuid, new User());
			return TempGuid;
		}

		private ChannelFacts GetChannelFactFromTimeframe(uint TimeframeID) {
			// Good candidate for optimization maybe
			if (ChannelFacts.ContainsKey(TimeframeID)) {
				return ChannelFacts[TimeframeID];
			} else {
				ChannelFacts.Add(TimeframeID, new ChannelFacts());
				return ChannelFacts[TimeframeID];
			}
		}


	}
}
