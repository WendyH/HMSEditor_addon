using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using FastColoredTextBoxNS;
using whYamlParser;
using System.Text;
using System.Text.RegularExpressions;

namespace HMSEditorNS {
	public class Theme {
		public string Author;
		public string Name;

		public Color Background    = Color.White;
		public Color Caret         = Color.Black;
		public Color Foreground    = Color.Black;
		public Color Invisibles    = Color.Gray;
		public Color LineHighlight = Color.FromArgb(100, 210, 210, 255);
		public Color ChangedLines  = Color.FromArgb(255, 152, 251, 152);
		public Color Selection     = Color.FromArgb(60, 0, 0, 255);
		
		public Color IndentBackColor;
		public Color LineNumberColor;
		public Color PaddingBackColor;
		public Color BreakpointLineColor = Color.FromArgb(255, 255, 128, 128);

		public Style StringStyle;
		public Style CommentStyle;
		public Style NumberStyle;
		public Style AttributeStyle;
		public Style ClassNameStyle;
		public Style KeywordStyle;
		public Style CommentTagStyle; // only csharp
		public Style DeclFunctionStyle;
		
		public Style TagBracketStyle; // only YAML HTML
		public Style TagNameStyle;        // HTML
		public Style AttributeValueStyle; // HTML
		public Style HtmlEntityStyle;     // HTML

		public Style XmlTagBracketStyle;     // XML
		public Style XmlTagNameStyle;        // XML
		public Style XmlAttributeStyle;      // XML
		public Style XmlAttributeValueStyle; // XML
		public Style XmlEntityStyle;         // XML
		public Style XmlCDataStyle;          // XML

		public Style FunctionsStyle;         // Lua SQL
		public Style VariableStyle;          // PHP SQL
		public Style StatementsStyle;        // SQL
		public Style TypesStyle;             // PHP SQL
		public Style KeywordStyle2;          // PHP
		public Style KeywordStyle3;          // PHP
		
		public Style InvisibleStyle;
	}

	static class Themes  {
		public static Dictionary<string, Theme> Dict = new Dictionary<string, Theme>();

		public static void LoadFromXml(string file) {
			Theme t = new Theme();

			YamlObject plist = PlistParser.LoadFromFile(file);
			string themeName = plist["name"];
			if (themeName == "") return;

			YamlObject list = plist.GetObject("settings");
			foreach (YamlObject item in list) {
				string     name     = item["name" ];
				string[]   scope    = item["scope"].Split(',');
				for (int i = 0; i < scope.Length; i++) scope[i] = scope[i].ToLower().Trim();
                YamlObject settings = item.GetObject("settings");
				if ((name=="") && (item["scope"]=="") && (settings.Count > 0)) {
					t.Background     = ToColor(settings["background"   ]);
					t.Caret          = ToColor(settings["caret"        ]);
					t.Foreground     = ToColor(settings["foreground"   ]);
					t.Invisibles     = ToColor(settings["invisibles"   ]);
					t.LineHighlight  = ToColor(settings["lineHighlight"]);
					t.Selection      = ToColor(settings["selection"    ]);
					t.InvisibleStyle = ToStyle(settings["invisibles"   ]);
					continue;
				}

				if      (InScope(scope, "comment"          )) t.CommentStyle    = ToStyleFromSettings(settings);
				else if (InScope(scope, "string"           )) t.StringStyle     = ToStyleFromSettings(settings);
				else if (InScope(scope, "constant.numeric" )) t.NumberStyle     = ToStyleFromSettings(settings);
				else if (InScope(scope, "keyword"          )) t.KeywordStyle    = ToStyleFromSettings(settings);
				else if (InScope(scope, "entity.name.class")) t.ClassNameStyle  = ToStyleFromSettings(settings);
				else if (InScope(scope, "declaration.class")) t.ClassNameStyle  = ToStyleFromSettings(settings);
				else if (InScope(scope, "support.class"    )) t.ClassNameStyle  = ToStyleFromSettings(settings);
				else if (InScope(scope, "meta.tag"         )) t.TagBracketStyle = ToStyleFromSettings(settings);
				else if (InScope(scope, "entity.name.tag"  )) t.TagNameStyle    = ToStyleFromSettings(settings);
				else if (InScope(scope, "variable"         )) t.VariableStyle   = ToStyleFromSettings(settings);
				else if (InScope(scope, "attribute-name"   )) t.AttributeStyle  = ToStyleFromSettings(settings);
				else if (InScope(scope, "support"          )) t.FunctionsStyle  = ToStyleFromSettings(settings);
				else if (InScope(scope, "support.function" )) t.FunctionsStyle  = ToStyleFromSettings(settings);
				else if (InScope(scope, "entity.name.function")) t.DeclFunctionStyle = ToStyleFromSettings(settings);

			}
			Dict[themeName] = t;
		}

		private static bool InScope(string[] arr, string key) { return Array.IndexOf(arr, key) >= 0; }

		private static Style ToStyleFromSettings(YamlObject yo) {
			string fore = yo["foreground"];
			string back = yo["background"];
			string font = yo["fontStyle" ];
			bool   bold = (font.IndexOf("bold"     ) >= 0);
			bool   ital = (font.IndexOf("italic"   ) >= 0);
			bool   undl = (font.IndexOf("underline") >= 0);
			return ToStyle(fore, back, bold, ital, undl);
        }

		public static void SetTheme(HMSEditor editor, string name) {
			if (Dict.ContainsKey(name)) {
				Theme t = Dict[name];
				editor.Editor.BackColor  = t.Background;
				editor.Editor.CaretColor = t.Caret;
				editor.Editor.ForeColor  = t.Foreground;

				editor.ColorCurrentLine       = t.LineHighlight;
				editor.ColorChangedLine       = t.ChangedLines;
				editor.Editor.SelectionColor  = t.Selection;
				editor.Editor.PaddingBackColor= t.Background;
				//editor.Editor.BreakpointLineColor = 

				editor.Editor.IndentBackColor  = (t.IndentBackColor .Name != "0") ? t.IndentBackColor  : editor.Editor.BackColor;
				editor.Editor.LineNumberColor  = (t.LineNumberColor .Name != "0") ? t.LineNumberColor  : Color.FromArgb(150, editor.Editor.ForeColor);
				editor.Editor.PaddingBackColor = (t.PaddingBackColor.Name != "0") ? t.PaddingBackColor : Color.FromArgb(150, editor.Editor.BackColor);

				uint icol = (uint)editor.Editor.IndentBackColor.ToArgb();
				if (icol < 0xFF808080) {
					editor.ColorChangedLine = ToColor("#024A02");
				}
				editor.btnMarkChangedLines_Click(null, EventArgs.Empty);

				editor.Editor.SyntaxHighlighter.StyleTheme = t;
				editor.Editor.RefreshTheme();

			}
		}

		public static void Init() {
			string data = HMS.ReadTextFromResource("ColorThemes.txt");
			LoadThemesFromString(data);
			
			// И сканируем файлы шаблонов в директории
			if (Directory.Exists(HMS.ThemesDir)) {
				string[] files = Directory.GetFiles(HMS.ThemesDir);
				foreach(string file in files) {
					LoadFromXml(file);
				}
			}

		}

		public static void LoadThemesFromFile(string file) {
			if (!File.Exists(file))
				file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(file));

			if (File.Exists(file)) {
				string data = File.ReadAllText(file);
				LoadThemesFromString(data);
            }
		}

		public static void LoadThemesFromString(string data) {
			INI tini = new INI();
			tini.NoComments = true;
            tini.Text = data;
			Dict.Clear();
			foreach(string section in tini.Dict.Keys) {
				Theme tt = new Theme();
				tt.Name = section;
				tt.Background      = ToColor(tini.Get("Background"     , section, ""));
				tt.Caret           = ToColor(tini.Get("Caret"          , section, ""));
				tt.Foreground      = ToColor(tini.Get("Foreground"     , section, ""));
				tt.Invisibles      = ToColor(tini.Get("Invisibles"     , section, ""));
				tt.LineHighlight   = ToColor(tini.Get("LineHighlight"  , section, ""));
				tt.ChangedLines    = ToColor(tini.Get("ChangedLines"   , section, ""));
				tt.Selection       = ToColor(tini.Get("Selection"      , section, ""));
				tt.LineNumberColor = ToColor(tini.Get("LineNumberColor", section, ""));
				tt.IndentBackColor = ToColor(tini.Get("IndentBackColor", section, ""));

				tt.StringStyle       = ToStyle2(tini.Get("StringStyle"      , section, ""));
				tt.CommentStyle      = ToStyle2(tini.Get("CommentStyle"     , section, ""));
				tt.NumberStyle       = ToStyle2(tini.Get("NumberStyle"      , section, ""));
				tt.AttributeStyle    = ToStyle2(tini.Get("AttributeStyle"   , section, ""));
				tt.ClassNameStyle    = ToStyle2(tini.Get("ClassNameStyle"   , section, ""));
				tt.KeywordStyle      = ToStyle2(tini.Get("KeywordStyle"     , section, ""));
				tt.CommentTagStyle   = ToStyle2(tini.Get("CommentTagStyle"  , section, ""));
				tt.TagBracketStyle   = ToStyle2(tini.Get("TagBracketStyle"  , section, ""));
				tt.FunctionsStyle    = ToStyle2(tini.Get("FunctionsStyle"   , section, ""));
				tt.VariableStyle     = ToStyle2(tini.Get("VariableStyle"    , section, ""));
				tt.DeclFunctionStyle = ToStyle2(tini.Get("DeclFunctionStyle", section, ""));
				tt.InvisibleStyle    = ToStyle (tini.Get("Invisibles"       , section, ""));
				Dict.Add(section, tt);
			}

		}

		public static void SaveThemesToFile(string file) {
			StringBuilder sb = new StringBuilder();

			foreach (var name in Dict.Keys) {
				Theme tt = Dict[name];
				sb.AppendLine("[" + name + "]");
				sb.AppendLine("Background="      + ColorToString(tt.Background));
				sb.AppendLine("Caret="           + ColorToString(tt.Caret));
				sb.AppendLine("Foreground="      + ColorToString(tt.Foreground));
				sb.AppendLine("Invisibles="      + ColorToString(tt.Invisibles));
				sb.AppendLine("LineHighlight="   + ColorToString(tt.LineHighlight));
				sb.AppendLine("ChangedLines="    + ColorToString(tt.ChangedLines));
				sb.AppendLine("Selection="       + ColorToString(tt.Selection));
				sb.AppendLine("LineNumberColor=" + ColorToString(tt.LineNumberColor));
				sb.AppendLine("IndentBackColor=" + ColorToString(tt.IndentBackColor));

				// Должно быть всё, что в SyntaxHighlighter.InitStyleSchema()
				sb.AppendLine("StringStyle="       + StyleToString(tt.StringStyle));
				sb.AppendLine("CommentStyle="      + StyleToString(tt.CommentStyle));
				sb.AppendLine("NumberStyle="       + StyleToString(tt.NumberStyle));
				sb.AppendLine("AttributeStyle="    + StyleToString(tt.AttributeStyle));
				sb.AppendLine("ClassNameStyle="    + StyleToString(tt.ClassNameStyle));
				sb.AppendLine("KeywordStyle="      + StyleToString(tt.KeywordStyle));
				sb.AppendLine("CommentTagStyle="   + StyleToString(tt.CommentTagStyle));
				sb.AppendLine("TagBracketStyle="   + StyleToString(tt.TagBracketStyle));
				sb.AppendLine("FunctionsStyle="    + StyleToString(tt.FunctionsStyle));
				sb.AppendLine("VariableStyle="     + StyleToString(tt.VariableStyle));
				sb.AppendLine("DeclFunctionStyle=" + StyleToString(tt.DeclFunctionStyle));
				sb.AppendLine();
            }
			File.WriteAllText(file, sb.ToString());
		}

		private static string ColorToString(Color c) {
			string result = "#";
			result += c.R.ToString("X2")+c.G.ToString("X2")+c.B.ToString("X2");
            if (c.A!=255) result += c.A.ToString("X2");
			return result;
		}

		private static string StyleToString(Style style) {
			TextStyle  s  = (TextStyle )style;
			if (s == null) return "null";
			SolidBrush bf = (SolidBrush)s.ForeBrush;
			SolidBrush bb = (SolidBrush)s.BackgroundBrush;

			string font = "";
			font = s.FontStyle.ToString();

			string result = "{";
			result += (bf == null) ? "" : "fore:"+ColorToString(bf.Color)+";";
			result += (bb == null) ? "" : "back:"+ColorToString(bb.Color)+";";
			result += (font == "") ? "" : "font:"+ font + ";";
			return result+"}";
		}

		private static Style ToStyle2(string styleString) {
			//{fore:#8ABBE7;fore:#FFFFFF;font:Bold, Italic;}
			string fore    = Regex.Match(styleString, "fore:(.*?);").Groups[1].Value;
			string back    = Regex.Match(styleString, "back:(.*?);").Groups[1].Value;
			bool bold      = Regex.IsMatch(styleString, "font:.*?Bold"     , RegexOptions.IgnoreCase);
			bool italic    = Regex.IsMatch(styleString, "font:.*?Italic"   , RegexOptions.IgnoreCase);
			bool underline = Regex.IsMatch(styleString, "font:.*?Underline", RegexOptions.IgnoreCase);
			return ToStyle(fore, back, bold, italic, underline);
		}

		private static Style ToStyle(string foreVal, string backVal = "", bool bold = false, bool italic = false, bool underline = false) {
			FontStyle fs = FontStyle.Regular;
			if (italic   ) fs |= FontStyle.Italic;
			if (bold     ) fs |= FontStyle.Bold;
			if (underline) fs |= FontStyle.Underline;
			Brush backBrush = null;
			Brush foreBrush = null;
            if (foreVal!="")
				foreBrush = new SolidBrush(ToColor(foreVal));
			if (backVal!="")
				backBrush = new SolidBrush(ToColor(backVal));
			Style s = new TextStyle(foreBrush, backBrush, fs);
			return s;
		}

		private static Color ToColor(string val) {
			if (string.IsNullOrEmpty(val)) return Color.Transparent;

			if (val.Length == 9) {
				string aval = val.Substring(7, 2);
				val = "#" + aval + val.Substring(1, 6);
			}
			ColorConverter cc = new ColorConverter();
			Color c = (Color)cc.ConvertFromString(val);
			return c;
		}

	}
}
