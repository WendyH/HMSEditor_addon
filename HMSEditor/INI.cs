/* This code is released under WTFPL Version 2 (http://www.wtfpl.net/) * Created by WendyH. Copyleft. */
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HMSEditorNS {
	/// <summary>
	/// INI class for working with the .ini or .conf text files.
	/// </summary>
	public class INI {
		/// <summary>
		/// Path and name to settings ini file.
		/// </summary>
		public string File = "";

		public bool NoComments = false;

		/// <summary>
		/// Data of settings ini file as Dictionary
		/// </summary>
		public Dictionary<string, Dictionary<string, string>> Dict = new Dictionary<string, Dictionary<string, string>>();

		/// <summary>
		/// Encoding of the ini file. Default is UTF8.
		/// </summary>
		public Encoding FileEncoding = Encoding.UTF8;

		/// <summary>
		/// Lines of the text file
		/// </summary>
		private List<string> lines = new List<string>();

		private static Regex RegexComment = new Regex(@"(#.*)"        , RegexOptions.Multiline | RegexOptions.Compiled);
		private static Regex RegexSection = new Regex(@"^\[(.*)\]\r?$", RegexOptions.Multiline | RegexOptions.Compiled);

		/// <summary>
		/// Data of settings ini file as text
		/// </summary>
		public string Text {
			get {
				string text = "";
				foreach (string line in lines) text += "\n" + line;
				return text.Substring(1);
			}
			set {
				lines.Clear();
				foreach (string line in value.Split('\n'))
					lines.Add(line);
				UpdateDictionaryData();
			}
		}

		// CONSTRUCTOR
		public INI(string fileName = "") {
			if (fileName.Length > 0) {
				File = fileName;
				Load();
			}
		}

		/// <summary>
		/// Load the data of ini file
		/// </summary>
		public void Load() {
			if (System.IO.File.Exists(this.File))
				Text = System.IO.File.ReadAllText(this.File, FileEncoding);
		}

		/// <summary>
		/// Save ini text data to file
		/// </summary>
		public void Save() {
			System.IO.File.WriteAllText(this.File, Text, FileEncoding);
		}

		/// <summary>
		/// Update the Dictionary of the settings data
		/// </summary>
		private void UpdateDictionaryData() {
			Dict.Clear();
			string section = "", key = "", val = "";
			for (int i = 0; i < lines.Count; i++) {
				string line;
				if (NoComments) line = lines[i];
				else line = RegexComment.Replace(lines[i], "");
				if (line.Trim().Length == 0) continue;   // Skip empty lines and comments
				// if current line is section - get current section name and continue
				if (RegexSection.IsMatch(line)) {
					section = RegexSection.Match(line).Groups[1].Value;
					if (!Dict.ContainsKey(section)) Dict[section] = new Dictionary<string, string>();
					continue;
				}
				int n = line.IndexOf('=');
				if (n < 1) {
					key = line.Trim();
					val = "";
				} else {
					key = line.Substring(0, n).Trim();
					val = line.Substring(n + 1).Trim();
				}
				if (!Dict.ContainsKey(section)) Dict[section] = new Dictionary<string, string>();
				Dict[section][key] = val;
			}
		}

		/// <summary>
		/// Sets the value of the settings
		/// </summary>
		/// <param name="name">Name of the settings</param>
		/// <param name="value">Value of the settings</param>
		/// <param name="section">Section name (optional)</param>
		public void Set(string name, bool value, string section = "") {
			Set(name, value ? "1" : "0", section);
		}

		public void Set(string name, object value, string section = "") {
			Set(name, value.ToString(), section);
		}
		
		/// <summary>
		/// Sets the value of the settings
		/// </summary>
		/// <param name="name">Name of the settings</param>
		/// <param name="value">Value of the settings</param>
		/// <param name="section">Section name (optional)</param>
		public void Set(string name, string value, string section = "") {
			bool isFoundSection = false;
			bool isFoundKey = false;
			string currentSection = "";
			int lastLineOfSection = 0;
			for (int i = 0; i < lines.Count; i++) {
				string line = RegexComment.Replace(lines[i], "");
				if (line.Trim().Length == 0) continue;   // Skip empty lines and comments
														 // if current line is section - get current section name and continue
				if (RegexSection.IsMatch(line)) { currentSection = RegexSection.Match(line).Groups[1].Value; continue; }
				if (currentSection != section) continue; // Skip is not our sections

				isFoundSection    = true;
				lastLineOfSection = i;

				Match m = Regex.Match(line, @"^(.*?)=");
				if (!m.Success) continue;
				string key = m.Groups[1].Value.Trim();
				if (key == name) {
					isFoundKey = true;
					lines[i]   = m.Value + " " + value;
					break;
				}
			}

			if (!isFoundSection) {
				if (section.Length > 0) {
					lines.Add("");
					lines.Add("[" + section + "]");
				}
				lastLineOfSection = lines.Count - 1;
			}

			if (!isFoundKey) {
				lines.Insert(lastLineOfSection + 1, name + " = " + value);
			}

			if (!Dict.ContainsKey(section)) Dict[section] = new Dictionary<string, string>();
			Dict[section][name] = value;
		}

		/// <summary>
		/// Get the value of the settings by name
		/// </summary>
		/// <param name="name">Name of the settings</param>
		/// <param name="section">Section name (optional)</param>
		/// <param name="defaultValue">Default value. Returned if there is no settings with such section and name</param>
		/// <returns>Returns value of the settings</returns>
		public string Get(string name, string section = "", string defaultValue = "") {
			string value = defaultValue;
			if (Dict.ContainsKey(section)) {
				if (Dict[section].ContainsKey(name))
					value = Dict[section][name];
			}
			return value;
		}

		/// <summary>
		/// Check is set value to "1", "Yes" or "Y"
		/// </summary>
		/// <param name="name">Name of the settings</param>
		/// <param name="section">Section name (optional)</param>
		/// <param name="defaultValue">Default value. Returned if there is no settings with such section and name</param>
		/// <returns>Returns the true if the setting value is "1", "Yes" or "Y". Else returns false.</returns>
		public bool Get(string name, string section = "", bool defaultValue = false) {
			bool retValue = defaultValue;
			if (Dict.ContainsKey(section)) {
				if (Dict[section].ContainsKey(name)) {
					string value = Dict[section][name].Trim();
					retValue = ((value == "1") || value.ToLower().StartsWith("y"));
				}
			}
			return retValue;
		}

		/// <summary>
		/// Gets all lines of section as list of strings
		/// </summary>
		/// <param name="section">Section name (optional)</param>
		/// <param name="excludeComments">If true - exclude empty lines and comments</param>
		/// <returns>Return List of the lines</returns>
		public List<string> GetLines(string section = "", bool excludeComments = false) {
			List<string> sectLines = new List<string>();
			string currentSection = "", line;
			for (int i = 0; i < lines.Count; i++) {
				line = lines[i];
				if (excludeComments) {   // Skip empty lines and comments
					line = RegexComment.Replace(line, "");
					if (line.Trim().Length == 0) continue;
				}
				// if current line is section - get current section name and continue
				if (RegexSection.IsMatch(line)) { currentSection = RegexSection.Match(line).Groups[1].Value; continue; }
				if (currentSection != section) continue; // Skip is not our sections
				sectLines.Add(line);
			}
			return sectLines;
		}

		/// <summary>
		/// Get contents of the section
		/// </summary>
		/// <param name="section">Section name</param>
		/// <param name="excludeComments">If true - exclude empty lines and comments</param>
		/// <returns>Returns content of section as string</returns>
		public string GetSectionText(string section, bool excludeComments = false) {
			List<string> t = GetLines(section, excludeComments);
			string text = "";
			foreach (string line in t) text += line + "\n";
			return text.Substring(0, text.Length - 1);
		}
	}
}
