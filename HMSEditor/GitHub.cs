using System;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Text;
using whYamlParser;

namespace HMSEditorNS {
	public static class GitHub {
		private static string      giturl = "https://api.github.com/repos/";
		private static Regex regexUpdDate = new Regex(@"""pushed_at""\s*?:\s*?""(.*?)""" , RegexOptions.Compiled);
		private static Regex regexVersion = new Regex(@"""tag_name""\s*?:\s*?""(.*?)"""  , RegexOptions.Compiled);
		private static Regex regexRelease = new Regex(@"""browser_download_url""\s*?:\s*?""([^""]+HMSEditor.exe)""", RegexOptions.Compiled);
		private static string  ReleaseUrl = "";

		public static bool IsWinVistaOrHigher() {
			OperatingSystem OS = Environment.OSVersion;
			return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6);
		}

		private static HttpWebRequest CreateRequest(string url) {
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36";
			request.Accept    = "application/vnd.github.v3+json";
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
			return request;
		}

		private static string DownloadString(string url) {
			string body = "";
			WebResponse  response   = null;
			StreamReader readStream = null;
			try {
				HttpWebRequest  request = CreateRequest(url);
				response = request.GetResponse();
				Stream    receiveStream = response.GetResponseStream();
				readStream = new StreamReader(receiveStream, Encoding.UTF8);
				body = readStream.ReadToEnd();
			} catch (WebException e) {
				HMS.LogError("Ошибка получения JSON данных с GitHub: " + url);
				HMS.LogError("Сообщение: " + e.Message+ " Status: " + e.Status.ToString());
				if (e.Response != null) {
					string resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
					HMS.LogError(resp);
				}

			} catch (Exception e) {
				HMS.LogError(e.ToString());

			} finally {
				if (response   != null) response  .Close();
				if (readStream != null) readStream.Close();
			}
			return body;
		}

		private static int DownloadFile(string url, string file) {
			int   bytesProcessed = 0;
			Stream  remoteStream = null;
			Stream   localStream = null;
			WebResponse response = null;
			CreateFoldersOfFilePath(file);
			try {
				HttpWebRequest request = CreateRequest(url);
				if (request != null) {
					response = request.GetResponse();
					if (response != null) {
						remoteStream  = response.GetResponseStream();
						localStream   = File.Create(file);
						byte[] buffer = new byte[1024];
						int bytesRead;
						do {
							bytesRead = remoteStream.Read(buffer, 0, buffer.Length);
							localStream.Write(buffer, 0, bytesRead);
							bytesProcessed += bytesRead;
						} while (bytesRead > 0);
					}
				}

			} catch (WebException e) {
				HMS.LogError("Ошибка получения файла с GitHub: " + url);
				HMS.LogError(e.Message + " Status: " + e.Status.ToString());
				if (e.Response != null) {
					string resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
					HMS.LogError(resp);
				}

			} catch (Exception e) {
				HMS.LogError(e.ToString());

			} finally {
				if (response     != null) response    .Close();
				if (remoteStream != null) remoteStream.Close();
				if (localStream  != null) localStream .Close();
			}
			return bytesProcessed;
		}

		public static string NormalizeDate(string date) {
			return date.Replace('T', ' ').Replace("Z", "").Replace('-', '.');
		}

		public static string GetRepoUpdatedDate(string userRepo, out string info) {
			string lastDate = ""; info = ""; StringBuilder sb = new StringBuilder();
			string jsonInfo = DownloadString(giturl + userRepo);
			lastDate = regexUpdDate.Match(jsonInfo).Groups[1].Value;
			string jsonCommitsData = DownloadString(giturl + userRepo + "/commits");
			YamlObject Commits = YamlParser.Parse(jsonCommitsData);
			foreach (var commit in Commits.ChildItems) {
				string date = NormalizeDate(commit[@"commit\author\date"]);
				string msg  = commit[@"commit\message"];
				if (date.StartsWith("2015.10")) break;
				if ((msg.IndexOf("README.md") > 0) || (msg.IndexOf("gitignore") > 0)) continue;
				sb.AppendLine("### "+date + "  \r\n" + msg + "\r\n");
			}
			info = DownloadString("https://raw.githubusercontent.com/"+userRepo+"/master/README.md");
			info+= "  \r\n## История обновлений и исправлений  \r\n" + sb.ToString();
			info = MarkdownToHtml(info);
			return lastDate;
		}

		public static string MarkdownToHtml(string mdText) {
			string body = "";
			string url  = "https://api.github.com/markdown";
			WebResponse  response   = null;
			StreamReader readStream = null;
			Stream       dataStream = null;
			try {
				HttpWebRequest  request = CreateRequest(url);
				request.Method = "POST";
				string postData = "{\"text\":\""+ YamlParser.EscapeSymbols(mdText) + "\",\"mode\":\"gfm\",\"context\":\"github/gollum\"}";
				byte[] byteArray = Encoding.UTF8.GetBytes(postData);
				request.ContentType   = "application/x-www-form-urlencoded";
				request.ContentLength = byteArray.Length;
				dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);

				response = request.GetResponse();
				Stream    receiveStream = response.GetResponseStream();
				readStream = new StreamReader(receiveStream, Encoding.UTF8);
				body = readStream.ReadToEnd();

			} catch (WebException e) {
				HMS.LogError("Ошибка получения Markdown данных с GitHub: " + url);
				HMS.LogError("Сообщение: " + e.Message+ " Status: " + e.Status.ToString());
				if (e.Response != null) {
					string resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
					HMS.LogError(resp);
				}

			} catch (Exception e) {
				HMS.LogError(e.ToString());

			} finally {
				if (response   != null) dataStream.Close();
				if (response   != null) response  .Close();
				if (readStream != null) readStream.Close();
			}
			return body;
		}

		public static string GetLatestReleaseVersion(string userRepo, out string updateInfo) {
			string urlInfo = giturl + userRepo + "/releases";
			string version = "";
			updateInfo = "";
			ReleaseUrl = "";
			StringBuilder sbVersionHistory = new StringBuilder();
			string jsonData = DownloadString(urlInfo);
			var    jsonINFO = YamlParser.Parse(jsonData);
			foreach (var releaseInfo in jsonINFO.ChildItems) {
				var assets = releaseInfo.GetObject("assets");
				foreach (var assetInfo in assets.ChildItems) {
					string url = assetInfo["browser_download_url"];
					if ((ReleaseUrl.Length == 0) && url.EndsWith("HMSEditor.exe")) {
						version    = releaseInfo["tag_name"];
						ReleaseUrl = url;
					}
				}
				sbVersionHistory.AppendLine("### v" + releaseInfo["tag_name"]);
				sbVersionHistory.AppendLine(releaseInfo["body"] + "\r\n");
			}
			updateInfo = sbVersionHistory.ToString();
			updateInfo = MarkdownToHtml(updateInfo);
			return version;
		}

		public static void DownloadLegacyArchive(string userRepo, string tmpFile) {
			DownloadFile("https://codeload.github.com/" + userRepo + "/legacy.zip/master", tmpFile);
		}

		public static event EventHandler DownloadFileCompleted;
		public static event EventHandler DownloadProgressChanged;
		public static ManualResetEvent allDone = new ManualResetEvent(false);
		const int BUFFER_SIZE = 1024;
		const int DefaultTimeout = 2 * 60 * 1000; // 2 minutes timeout

		// Abort the request if the timer fires.
		private static void TimeoutCallback(object state, bool timedOut) {
			if (timedOut) {
				HttpWebRequest request = state as HttpWebRequest;
				if (request != null) {
					request.Abort();
				}
			}
		}

		private static void StartAsyncRequest(string url, RequestState requestState) {
			IAsyncResult result = null;
			try {
				requestState.StreamDst = File.OpenWrite(requestState.File);
				if (!requestState.StreamDst.CanWrite) {
					requestState.Close();
					return;
				}
				HttpWebRequest request = CreateRequest(url);
				requestState.request = request;
				result = request.BeginGetResponse(new AsyncCallback(AsyncRespCallback), requestState);

				if (result != null)
					ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), request, DefaultTimeout, true);

				//allDone.WaitOne();

			} catch (WebException e) {
				requestState.Close();
				HMS.LogError("Ошибка асинхронного запроса на GitHub: " + url);
				HMS.LogError(e.Message + " Status: " + e.Status.ToString());
				if (e.Response != null) {
					string resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
					HMS.LogError(resp);
				}

			} catch (Exception e) {
				requestState.Close();
				HMS.LogError(e.ToString());

			}
		}

		private static void AsyncRespCallback(IAsyncResult asyncResult) {
			RequestState requestState = ((RequestState)(asyncResult.AsyncState));
			try {
				HttpWebResponse response = ((HttpWebResponse)(requestState.request.EndGetResponse(asyncResult)));
				if (requestState.response == null)
					requestState.response = response;
				requestState.TotalBytes = response.ContentLength;
				if (DownloadProgressChanged != null)
					DownloadProgressChanged(requestState, EventArgs.Empty);
				Stream stream = requestState.response.GetResponseStream();
				stream.BeginRead(requestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(AsyncReadCallback), requestState);

			} catch (WebException e) {
				HMS.LogError("Ошибка асинхронного получения данных с GitHub");
				HMS.LogError(e.Message + " Status: " + e.Status.ToString());
				if (e.Response != null) {
					string resp = new StreamReader(e.Response.GetResponseStream()).ReadToEnd();
					HMS.LogError(resp);
				}
				requestState.Close();

			} catch (Exception e) {
				HMS.LogError(e.ToString());
				requestState.Close();
			}
			//allDone.Set();
		}

		private static void AsyncReadCallback(IAsyncResult asyncResult) {
			RequestState state = ((RequestState)(asyncResult.AsyncState));
			int bytesRead = 0;
			try {
				Stream stream = state.response.GetResponseStream();
				if (stream.CanRead)
					bytesRead = stream.EndRead(asyncResult);

				if (bytesRead > 0) {
					state.BytesRead += bytesRead;
					state.StreamDst.Write(state.BufferRead, 0, bytesRead);
					state.StreamDst.Flush();
					stream.BeginRead(state.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(AsyncReadCallback), state);
					if (DownloadProgressChanged != null)
						DownloadProgressChanged(state, EventArgs.Empty);
				} else {
					state.Close();
				}
			} catch {
				state.Close();
			}
			//allDone.Set();
		}

		public static void DownloadLatestReleaseAsync(string tmpFile) {
			DownloadFileAsync(ReleaseUrl, tmpFile);
		}

		public static void DownloadFileAsync(string url, string tmpFile) {
			RequestState requestState = new RequestState();
			requestState.File = tmpFile;
			StartAsyncRequest(url, requestState);
		}

		internal static Regex extractOnlyVersion = new Regex(@"[\d\.,\s]+", RegexOptions.Compiled);

		/// <summary>
		/// Create all folders and subfolders in the file path
		/// </summary>
		/// <param name="path">Filename with path</param>
		private static void CreateFoldersOfFilePath(string path) {
			try {
				string directory = "";
				foreach (string dir in Path.GetDirectoryName(path).Split(Path.DirectorySeparatorChar)) {
					directory += dir + Path.DirectorySeparatorChar;
					if (!Directory.Exists(directory))
						Directory.CreateDirectory(directory);
				}
			} catch {
			}
		}

		/// <summary>
		/// Compare versions of form "1,2,3,4" or "1.2.3.4". Throws FormatException
		/// in case of invalid version.
		/// </summary>
		/// <param name="strA">the first version</param>
		/// <param name="strB">the second version</param>
		/// <returns>less than zero if strA is less than strB, equal to zero if
		/// strA equals strB, and greater than zero if strA is greater than strB</returns>
		public static int CompareVersions(string strA, string strB) {
			strA = extractOnlyVersion.Match(strA).Value.Replace(" ", "").Trim();
			strB = extractOnlyVersion.Match(strB).Value.Replace(" ", "").Trim();
			if (strA.Length == 0) return -1;
			if (strB.Length == 0) return  1;
			int result = 0;
			try {
				Version vA = new Version(strA.Replace(",", "."));
				Version vB = new Version(strB.Replace(",", "."));
				return vA.CompareTo(vB);
			} catch (Exception e) {
				HMS.LogError("Ошибка сравнений версий " + strA + " и " + strB);
				HMS.LogError(e.ToString());
			}
			return result;
		}

		public class RequestState {
			const int BUFFER_SIZE = 1024;
			public StringBuilder   requestData = new StringBuilder("");
			public byte[]          BufferRead  = new byte[BUFFER_SIZE];
			public HttpWebRequest  request     = null;
			public HttpWebResponse response    = null;
			public Stream          StreamDst   = null;
			public long            TotalBytes  = 0;
			public long            BytesRead   = 0;
			public string          File        = "";

			public void Close() {
				if (StreamDst != null) StreamDst.Close();
				if (response != null) {
					response.Close();
					if (DownloadProgressChanged != null)
						DownloadFileCompleted(this, EventArgs.Empty);
				}
			}
		}
	}


}
