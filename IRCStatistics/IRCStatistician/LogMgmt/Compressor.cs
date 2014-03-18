using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using IRCShared;
using Ionic.Zip;

namespace IRCStatistician.LogMgmt {
	public static class Compressor {

		public static void Compress() {
			AppLog.WriteLine(5, "STATUS", "Entered IRCStatistician.Compressor.Compress().");
			string[] Files = Directory.GetFiles(@"chatlogs/", "*_*_????????-??.log"); 
			Dictionary<string, List<string>> NetworkFiles = new Dictionary<string, List<string>>();
			// Get the list of Network filename prefixes
			foreach (string CurFile in Files) {
				string NetworkChunk = GetNetworkChunk(CurFile);
				if (!NetworkFiles.ContainsKey(NetworkChunk)) {
					NetworkFiles.Add(NetworkChunk, new List<string>());
				}
				NetworkFiles[NetworkChunk].Add(CurFile);
			}
			foreach (KeyValuePair<string, List<string>> CurNetwork in NetworkFiles) {
				CompressNetwork(CurNetwork.Key, CurNetwork.Value);
			}
		}


		private static void CompressNetwork(string Prefix, List<string> FileList) {
			List<string> CompressList = new List<string>();
			foreach (string CurFile in FileList) {
				string DateText = CurFile.Substring(CurFile.Length - 15, 8);
				if (!CompressList.Contains(DateText)) {
					CompressList.Add(DateText);
				}
			}
			if (CompressList.Count > 1) {
				// We don't want to compress todays, only past dates
				CompressList.RemoveAt(CompressList.Count - 1);
				foreach (string DateToCompress in CompressList) {
					CompressDate(Prefix, DateToCompress, FileList);
				}
			} else {
				AppLog.WriteLine(5, "STATUS", "Nothing to compress...");
			}
		}

		private static void CompressDate(string Prefix, string Date, List<string> FileList) {
			string MatchRegexStr = @"chatlogs/" + Prefix + "_" + Date + @"-[0-2][0-9]\.log$";
			string NewZipFile = @"chatlogs/" + Prefix + "_" + Date + ".zip";
			AppLog.WriteLine(5, "STATUS", "Compressing Files: " + MatchRegexStr + " => " + NewZipFile);
			using (ZipFile NewDateZipFile = new ZipFile()) {
				foreach (string CurFile in FileList) {
					Match Match = Regex.Match(CurFile, MatchRegexStr);
					if (Match.Success) {
						// If matches the date we're compressing add it to the date's zip
						NewDateZipFile.AddFile(CurFile, "");
						// Save the zip after adding it. This may be inefficient but we want to be sure we don't lose data.
						// If this becomes a big time sink then we can move the delete to it's own loop at the end of this
						// function.
						NewDateZipFile.Save(NewZipFile);
						// ...and delete it from the filesystem.
						File.Delete(CurFile);
					}
				}
			}
		}


		private static string GetNetworkChunk(string InputFilename) {
			string TempStr = InputFilename.Remove(InputFilename.Length - 16);
			return TempStr.Substring(TempStr.IndexOf('/') + 1);
		}

	}
}
