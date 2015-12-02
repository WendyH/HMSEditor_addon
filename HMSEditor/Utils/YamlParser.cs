using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// By WendyH. Worked with broken format without exceptions. Sorry 4 code. Magic code.
/// </summary>
namespace whYamlParser {
	public enum YamlObjectType { Scalar, Mapping, Sequence }

	/// <summary>
	/// YamlObjects collection
	/// </summary>
	public class YamlObjects: List<YamlObject> {
		public bool ContainsName(string name) {
			foreach (YamlObject o in this) if (o.Name == name) return true;
			return false;
		}

		public YamlObject this[string name] {
			get {
				foreach (var o in this) if (o.Name == name) return o;
				return new YamlObject();
			}
		}
	}

	/// <summary>
	/// The item of yaml (json) colletion
	/// </summary>
	public class YamlObject : IEnumerable {
		public string         Name   = "";
		public string         Value  = "";
		public int            Indent = 0;
		public YamlObject     Parent = null;
		public int            Line   = 0;
		public int            Char   = 0;
		public YamlObjectType Type   = YamlObjectType.Scalar;
		public bool           Closed = true;
		public bool           Inline = false;
		public string         Tag    = "";
		public YamlObjects    ChildItems = new YamlObjects();

		// Constructors
		public YamlObject() {

		}

		public YamlObject(string name) {
			Name = name;
		}

		public YamlObject(string name, string value) {
			Name  = name;
			Value = value;
		}

		public int Count { get { return ChildItems.Count; } }

		public string this[string name] {
			get {
				return GetObject(name).Value;
			}
		}

		public YamlObject GetObject(string name) {
			string[] names = name.Split('\\');
			YamlObjects curList = ChildItems;
			YamlObject  result  = null;
			foreach (string part in names) {
				for (int i = 0; i < curList.Count; i++) {
					YamlObject o = curList[i];
					string nameInList = o.Name;
					if (nameInList.Length == 0)
						nameInList = i.ToString();
					if (nameInList == part) { result = o; curList = o.ChildItems; break; }
				}
			}
			if (result == null)
				result = new YamlObject();
			return result;
		}

		public IEnumerator GetEnumerator() {
			return ((IEnumerable)ChildItems).GetEnumerator();
		}

		public new string ToString() {
			return Name + ": " + Value;
		}
	}

	/// <summary>
	/// Parser for YAML or JSON farmat string
	/// </summary>
	public static class YamlParser {
		public static bool CheckCloseObjects  = true;
		public static bool LeaveValuesEscaped = false;
		public static string yaml { get; set; }
		public static string json { get; internal set; }
		public static List<string> Errors = new List<string>();

		private static Dictionary<string, YamlObject> aliases = new Dictionary<string, YamlObject>();
		private static Regex reComment        = new Regex(@"^\s*?($|#|%YAML|\.\.\.)");
		private static Regex reKeyValuePair   = new Regex(@"^\s*(""\\[\s\S]|[^""]*"")\s*:(.*)|^\s*('\\[\s\S]|[^']*')\s*:(.*)|^\s*([^:\{\[]+)\s*:(.*)");
		private static Regex reQuotedString   = new Regex(@"^\s*""(.*)""\s*$|^\s*'(.*)'\s*$");
		private static Regex reObjectsEnds    = new Regex(@"(.*)([\}\]\{\[,>\|])\s*$");
		private static Regex reAlias          = new Regex(@"^\s*?&([\w-_]+)(.*)");
		private static Regex reRefer          = new Regex(@"^\s*?\*([\w-_]+)");
		private static Regex reStringsAndComments = new Regex(@"""(\\[\s\S]|[^""\n])*""|'(\\[\s\S]|[^'\n])*'|(#.*|\/\*[\s\S]*?\*\/)");
		private static Regex reTags           = new Regex(@"^\s*(!!\w+|!\w+)(.*)");
		private static Regex reSequenceEntry  = new Regex(@"^\s*(- |-$|--- |---$)(.*)");
		private static Regex reBeginInlineObj = new Regex(@"^\s*(\{|\[)(.*)");
		private static Regex reInlinePart     = new Regex(@"""(\\[\s\S]|[^""]*)""|'(\\[\s\S]|[^']*)'|([,\]\}])");

		public static void SetTypeYamlObject(YamlObject curObject, YamlObjectType type, string errKey) {
			if (curObject.Type == YamlObjectType.Scalar) curObject.Type = type;
			else if (curObject.Type != type) Errors.Add(YamlErrors.Msg(errKey, curObject.Line, curObject.Char));
		}

		private static string ReplaceScreeningSymbols(string text) {
			if (LeaveValuesEscaped) return text;
			text = text.Replace(@"\""", @"""").Replace(@"\/", "/").Replace(@"\b", "\b").Replace(@"\t", "\t");
			return text.Replace(@"\f", "\f").Replace(@"\n", "\n").Replace(@"\r", "\r").Replace(@"\\", @"\");
		}

		public static string EscapeSymbols(string text) {
			text = text.Replace(@"\", @"\\").Replace(@"""", @"\""").Replace("/", @"\/").Replace("\b", @"\b");
			return text.Replace("\f", @"\f").Replace("\n", @"\n").Replace("\r", @"\r").Replace("\t", @"\t");
		}

		private static void _CheckYamlValue(int iLine, ref int iChar, int indent, string sVal, ref YamlObject curObject, string sKey = "") {
			Match m; bool catched = false;
			//if (sVal.Trim().Length == 0) return;
			m = reTags.Match(sVal); // Tags
			if (m.Success) { curObject.Tag = m.Groups[1].Value; sVal = m.Groups[2].Value; iChar += m.Groups[2].Index; }
			if (!catched && !curObject.Inline) {
				m = reSequenceEntry.Match(sVal); // Sequence entry
				if (m.Success) {
					iChar += m.Groups[2].Index;
					sVal = m.Groups[2].Value.Trim();
					sKey = string.Empty;
					SetTypeYamlObject(curObject, YamlObjectType.Sequence, "YE001");
					curObject = _AddChild(iLine, iChar, sKey, string.Empty, indent, curObject);
					indent += m.Groups[2].Index; catched = true;
				}
			}
			if (!catched) {
				m = reBeginInlineObj.Match(sVal); // Begin inline object
				if (m.Success) {
					iChar += m.Groups[2].Index;
					sVal = m.Groups[2].Value;
					if (curObject.Inline)
						curObject = _AddChild(iLine, iChar, sKey, string.Empty, indent, curObject);
					curObject.Inline = true;
					curObject.Closed = false;
					curObject.Type = (m.Groups[1].Value == "{") ? YamlObjectType.Mapping : YamlObjectType.Sequence;
					catched = true;
					if (m.Groups[1].Value == "[") sKey = string.Empty; if (curObject.Inline) sKey = string.Empty;
				}
			}
			if (!catched && curObject.Inline) {
				string line = sVal;
				MatchCollection mc = reInlinePart.Matches(line); // Find inline part of values from begin to "," or "]" or "}" out of strings
				foreach (Match match in mc) {
					string ch = match.Groups[3].Value;
					if (ch.Length == 0) continue;
					if (!CheckCloseObjects) {
						if ((ch == "]") && (curObject.Type != YamlObjectType.Sequence)) continue;
						if ((ch == "}") && (curObject.Type != YamlObjectType.Mapping )) continue;
					}
					sVal = (match.Index > 0) ? line.Substring(0, match.Index) : string.Empty; iChar += match.Index;
					line = (line.Length > match.Groups[3].Index) ? line.Substring(match.Groups[3].Index + 1) : string.Empty;
					if (sVal.Length > 0) _CheckYamlValue(iLine, ref iChar, indent, sVal, ref curObject, sKey);
					if ((ch == "]") || (ch == "}")) {
						if (ch == "]") SetTypeYamlObject(curObject, YamlObjectType.Sequence, "YE101");
						else SetTypeYamlObject(curObject, YamlObjectType.Mapping, "YE102");
						curObject.Closed = true;
						curObject = curObject.Parent;
						sKey = string.Empty;
					}
					sVal = line; catched = true; break;
				}
			}
			if (!catched) {
				m = reKeyValuePair.Match(sVal); // Map entry
				if (m.Success) {
					iChar += m.Groups[2].Index + m.Groups[4].Index + m.Groups[6].Index;
					sKey =  (m.Groups[1].Value + m.Groups[3].Value + m.Groups[5].Value.Trim());
					sVal =   m.Groups[2].Value + m.Groups[4].Value + m.Groups[6].Value;
					SetTypeYamlObject(curObject, YamlObjectType.Mapping, "YE002");
					if (!curObject.Inline) curObject = _AddChild(iLine, iChar, sKey, sVal, indent, curObject);
					m = reAlias.Match(sVal); if (m.Success) sVal = m.Groups[2].Value.TrimStart();
					catched = reBeginInlineObj.IsMatch(sVal);
				}
			}
			if (catched) {
				if ((sVal + sKey).Length > 0) _CheckYamlValue(iLine, ref iChar, indent, sVal, ref curObject, sKey);
			} else {
				if (curObject.Inline)
					_AddChild(iLine, iChar, sKey, sVal, indent, curObject);
				else {
					m = reObjectsEnds.Match(sVal); if (m.Success) sVal = m.Groups[1].Value.Trim();
					m = reAlias.Match(sVal); if (m.Success) { sVal = m.Groups[2].Value.TrimStart(); aliases[m.Groups[1].Value] = curObject; }
					m = reRefer.Match(sVal); if (m.Success) {
						sVal = m.Groups[2].Value.TrimStart();
						if (aliases.ContainsKey(m.Groups[1].Value)) {
							sVal = aliases[m.Groups[1].Value].Value;
							_AddChild(iLine, iChar, aliases[m.Groups[1].Value].Name, sVal, indent, curObject, aliases[m.Groups[1].Value]);
						}
					}
					m = reQuotedString.Match(sVal); if (m.Success) sVal = m.Groups[1].Value + m.Groups[2].Value; else sVal = sVal.Trim();
					curObject.Value = sVal;
				}
			}
		}

		public static YamlObject Parse(string yamlText) {
			YamlObject resultObject = new YamlObject();
			LoadFromString(yamlText, resultObject);
			return resultObject;
		}

		public static bool LoadFromString(string yamlText, YamlObject yoResult) {
			bool bSuccess = true; string line = string.Empty, lastchar, mchar = string.Empty;
			int mulindent = int.MaxValue, mulindent1 = -1, iChar, indent;
			YamlObject curObject = yoResult; aliases.Clear(); Errors.Clear();
			// Remove all comments out the strings
			yaml = reStringsAndComments.Replace(yamlText,
						delegate (Match mr) { if (mr.Value.StartsWith("#")) return string.Empty; return mr.Value; });

			string[] Lines = yaml.Split('\n');
			for (int i = 0; i < Lines.Length; i++) {
				line   = Lines[i]; if (reComment.IsMatch(line)) continue;
				indent = line.Length - line.TrimStart().Length;
				line   = line.Trim();
				if (!curObject.Inline) {
					if (indent > mulindent) {   // Multiline scalar
						if (mulindent1 < 0) mulindent1 = indent;                  // mulindent1 - indent first line of multiline text
						if (curObject.Value.Length > 0) curObject.Value += mchar; // Skip for first line (mchar - "\r" or " ")
						curObject.Value += line.PadLeft(line.Length + indent - mulindent1); // Padding left from indent first line
						continue;
					}
					mulindent = int.MaxValue; mulindent1 = -1;
					// Get parent by indent
					while ((curObject.Parent != null) && (indent <= curObject.Indent) && (curObject != curObject.Parent)) curObject = curObject.Parent;
				}
				iChar = indent;
				_CheckYamlValue(i, ref iChar, indent, line, ref curObject);
				if (curObject!=null && !curObject.Inline) {
					lastchar = (line.Length > 0) ? line.Substring(line.Length - 1) : string.Empty;
					if ((lastchar == ">") || (lastchar == "|")) { mulindent = indent; mchar = (lastchar == ">") ? " " : "\n"; }
				}
			}
			aliases.Clear();
			return bSuccess;
		}

		private static YamlObject _AddChild(int iLine, int iChar, string name, string val, int indent,
											YamlObject parent, YamlObject copyObject = null, int level = 0) {
			Match m; YamlObject childObject; string alias = string.Empty, refer = string.Empty;
			m = reAlias.Match(name); if (m.Success) name = m.Groups[2].Value.TrimStart();
			m = reAlias.Match(val ); if (m.Success) { val = m.Groups[2].Value.TrimStart(); alias = m.Groups[1].Value; }
			if (copyObject == null) {
				m = reObjectsEnds .Match(val ); if (m.Success) { val = m.Groups[1].Value.Trim(); }
				m = reQuotedString.Match(name); if (m.Success) name = m.Groups[1].Value + m.Groups[2].Value; else name = name.Trim();
				m = reQuotedString.Match(val ); if (m.Success) val  = m.Groups[1].Value + m.Groups[2].Value; else val  = val .Trim();
			}
			m = reRefer.Match(val); if (m.Success) refer = m.Groups[1].Value;
			if ((refer.Length > 0) && aliases.ContainsKey(refer)) {
				copyObject = aliases[refer];
				val = copyObject.Value;
			}
			if ((copyObject == null) || (level > 0)) { childObject = new YamlObject(); childObject.Parent = parent; } else childObject = parent;
			childObject.Name   = name;
			childObject.Value  = ReplaceScreeningSymbols(val);
			childObject.Indent = indent;
			childObject.Line   = iLine;
			childObject.Char   = iChar;
			childObject.Inline = parent.Inline;
			if ((copyObject == null) || (level > 0)) parent.ChildItems.Add(childObject);

			if (alias.Length > 0) aliases[alias] = childObject;
			if (parent != null) parent.Value = string.Empty;
			if ((copyObject != null) && (level < 30))
				foreach (YamlObject yo in copyObject.ChildItems)
					_AddChild(iLine, iChar, yo.Name, yo.Value, yo.Indent, childObject, yo, level + 1);
			return childObject;
		}

	}

	public static class YamlErrors {
		public static Dictionary<string, string> Messages = new Dictionary<string, string>();

		static YamlErrors() {
			Messages.Add("YE001", "Unexpected sequence entry in non sequence object.");
			Messages.Add("YE002", "Unexpected maping entry in non mapping object.");
			Messages.Add("YE003", "Unexpected scalar entry in mapping object.");
			Messages.Add("YE101", "Unexpected close sequence indicator.");
			Messages.Add("YE102", "Unexpected close maping indicator.");
		}

		public static string Msg(string errKey, int iLine = -1, int iChar = -1) {
			string err = ""; iLine++; iChar++;
			//if ((iLine > 0) && (iChar > 0)) err += "[" + iLine.ToString() + ", " + iChar.ToString() + "] ";
			//else if (iLine > 0) err += "[" + iLine.ToString() + "] ";
			err += "[" + iLine.ToString() + "] ";
			if (Messages.ContainsKey(errKey)) err += Messages[errKey];
			return err;
		}
	}
}

