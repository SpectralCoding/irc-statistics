using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IRCStatistician.Support;
using IRCShared;

namespace IRCStatistician.Processing {
	class User {
		public SortedDictionary<DateTime, Sender> SenderHistory;
		public SortedDictionary<uint, UserFacts> UserFacts;


		public User() {
			SenderHistory = new SortedDictionary<DateTime, Sender>();
			UserFacts = new SortedDictionary<uint, UserFacts>();
		}

		public void AddJoin(DateTime Timestamp, uint TimeframeID, Sender Sender) {
			RegisterSender(Timestamp, Sender);
			UserFacts CurUF = GetUserFactFromTimeframe(TimeframeID);
			CurUF.Joins++;
		}
		public void AddPart(DateTime Timestamp, uint TimeframeID, Sender Sender, string PartMsg) {
			RegisterSender(Timestamp, Sender);
			UserFacts CurUF = GetUserFactFromTimeframe(TimeframeID);
			CurUF.Parts++;
		}
		public void AddKick(DateTime Timestamp, uint TimeframeID, Sender Sender, string PersonKicked, string KickMsg) {
			RegisterSender(Timestamp, Sender);
			UserFacts CurUF = GetUserFactFromTimeframe(TimeframeID);
			CurUF.Kicks++;
		}
		public void AddQuit(DateTime Timestamp, uint TimeframeID, Sender Sender, string KickMsg) {
			RegisterSender(Timestamp, Sender);
			UserFacts CurUF = GetUserFactFromTimeframe(TimeframeID);
			CurUF.Quits++;
		}
		public void AddMessage(DateTime Timestamp, uint TimeframeID, Sender Sender, string PrivMsg) {
			RegisterSender(Timestamp, Sender);
			UserFacts CurUF = GetUserFactFromTimeframe(TimeframeID);
			CurUF.Messages++;
		}
		public void AddAction(DateTime Timestamp, uint TimeframeID, Sender Sender, string ActionMsg) {
			RegisterSender(Timestamp, Sender);
			UserFacts CurUF = GetUserFactFromTimeframe(TimeframeID);
			CurUF.Actions++;
		}
		public void AddTopic(DateTime Timestamp, uint TimeframeID, Sender Sender, string TopicText) {
			RegisterSender(Timestamp, Sender);
			UserFacts CurUF = GetUserFactFromTimeframe(TimeframeID);
			CurUF.TopicsSet++;
		}
		public void AddMode(DateTime Timestamp, uint TimeframeID, Sender Sender, string ModeText) {
			RegisterSender(Timestamp, Sender);
			UserFacts CurUF = GetUserFactFromTimeframe(TimeframeID);
			CurUF.ModesSet++;
		}
		public void ChangeNick(DateTime Timestamp, uint TimeframeID, Sender Sender, string NewNick) {
			Sender NewSender = Sender;
			NewSender.Nick = NewNick;
			NewSender.SenderStr = NewSender.SenderStr.Replace(Sender.Nick + '!', NewNick + '!');
			RegisterSender(Timestamp, NewSender);
			UserFacts CurUF = GetUserFactFromTimeframe(TimeframeID);
			CurUF.NickChanges++;
		}

		public void RegisterSender(DateTime Timestamp, Sender Sender) {
			// Good candidate for optimization
			if ((SenderHistory.Count == 0) || (SenderHistory.Values.Last().SenderStr != Sender.SenderStr)) {
				while (SenderHistory.ContainsKey(Timestamp)) {
					Timestamp = Timestamp.AddMilliseconds(1);
				}
				SenderHistory.Add(Timestamp, Sender);
			}
		}

		private UserFacts GetUserFactFromTimeframe(uint TimeframeID) {
			// Good candidate for optimization maybe
			if (UserFacts.ContainsKey(TimeframeID)) {
				return UserFacts[TimeframeID];
			} else {
				UserFacts.Add(TimeframeID, new UserFacts());
				return UserFacts[TimeframeID];
			}
		}

	}
}
