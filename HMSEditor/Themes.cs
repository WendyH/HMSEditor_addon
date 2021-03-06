﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using FastColoredTextBoxNS;
using whYamlParser;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

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
        public Color Selection       = Color.FromArgb(60, 0, 0, 255);
        public Color SelectionBorder = Color.FromArgb(60, 0, 0, 255);
        public Color SelectionForegr = Color.Transparent;

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
        public Style ConstantsStyle;
        public Style CommentTagStyle; // only csharp
        public Style DeclFunctionStyle;
        public Style PunctuationStyle;

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
            //string themeName = plist["name"];
            //if (themeName == "") return;
            string themeName = Path.GetFileNameWithoutExtension(file);
            YamlObject list = plist.GetObject("settings");
            foreach (YamlObject item in list) {
                string     name     = item["name" ];
                string[]   scope    = item["scope"].Split(',');
                for (int i = 0; i < scope.Length; i++) scope[i] = scope[i].ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim();
                YamlObject settings = item.GetObject("settings");
                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(item["scope"]) && (settings.Count > 0)) {
                    t.Background      = ToColor(settings["background"     ]);
                    t.Caret           = ToColor(settings["caret"          ]);
                    t.Foreground      = ToColor(settings["foreground"     ]);
                    t.Invisibles      = ToColor(settings["invisibles"     ]);
                    t.LineHighlight   = ToColor(settings["lineHighlight"  ]);
                    t.InvisibleStyle  = ToStyle(settings["invisibles"     ]);
                    t.Selection       = ToColor(settings["selection"      ]);
                    t.SelectionBorder = ToColor(settings["selectionBorder"]);
                    t.SelectionForegr = ToColor(settings["selectionForeground"]);
                    if (t.Selection == Color.Transparent) t.Selection = ToColor(settings["selectionBackground"]);
                    continue;
                }

                if (InScope(scope, "comment"          )) t.CommentStyle    = ToStyleFromSettings(settings);
                if (InScope(scope, "string"           )) t.StringStyle     = ToStyleFromSettings(settings);
                if (InScope(scope, "constant.numeric" )) t.NumberStyle     = ToStyleFromSettings(settings);
                if (InScope(scope, "keyword"          )) t.KeywordStyle    = ToStyleFromSettings(settings);
                if (InScope(scope, "storage.type"     )) t.TypesStyle      = ToStyleFromSettings(settings);
                if (InScope(scope, "entity.name.class")) t.ClassNameStyle  = ToStyleFromSettings(settings);
                if (InScope(scope, "declaration.class")) t.ClassNameStyle  = ToStyleFromSettings(settings);
                if (InScope(scope, "support.class"    )) t.ClassNameStyle  = ToStyleFromSettings(settings);
                if (InScope(scope, "meta.tag"         )) t.TagBracketStyle = ToStyleFromSettings(settings);
                if (InScope(scope, "entity.name.tag"  )) t.TagNameStyle    = ToStyleFromSettings(settings);
                if (InScope(scope, "variable"         )) t.VariableStyle   = ToStyleFromSettings(settings);
                if (InScope(scope, "attribute-name"   )) t.AttributeStyle  = ToStyleFromSettings(settings);
                if (InScope(scope, "support.function" )) t.FunctionsStyle  = ToStyleFromSettings(settings);
                if (InScope(scope, "support.constant" )) t.ConstantsStyle  = ToStyleFromSettings(settings);
                if (InScope(scope, "entity.name.function")) t.DeclFunctionStyle = ToStyleFromSettings(settings);
                if (InScope(scope, "punctuation"         )) t.PunctuationStyle  = ToStyleFromSettings(settings);
                if (t.KeywordStyle == null) if(InScope(scope, "storage")) t.KeywordStyle = ToStyleFromSettings(settings);
            }
            Dict[themeName] = t;
        }

        private static bool InScope(string[] arr, string key) { return Array.IndexOf(arr, key) >= 0; }

        private static Style ToStyleFromSettings(YamlObject yo) {
            string fore = yo["foreground"];
            string back = yo["background"];
            string font = yo["fontStyle" ];
            bool   bold = font.IndexOf("bold"     , StringComparison.Ordinal) >= 0;
            bool   ital = font.IndexOf("italic"   , StringComparison.Ordinal) >= 0;
            bool   undl = font.IndexOf("underline", StringComparison.Ordinal) >= 0;
            if ((fore.Length + back.Length == 0) && !bold && !ital && !undl) return null;
            return ToStyle(fore, back, bold, ital, undl);
        }

        public static void SetTheme(FastColoredTextBox tb, string name) {
            if (Dict.ContainsKey(name)) {
                Theme t = Dict[name];
                if ((t.ConstantsStyle == null) && (t.NumberStyle != null) && (name!="Стандартная"))
                    t.ConstantsStyle = ((TextStyle)t.NumberStyle).Clone(30);

                tb.BackColor  = t.Background;
                tb.CaretColor = t.Caret;
                tb.ForeColor  = t.Foreground;
                tb.CaretCreated = false;

                tb.SelectionStyle   = new SelectionStyle(t.Selection, t.SelectionForegr, t.SelectionBorder);
                tb.PaddingBackColor = t.Background;
                if (t.SelectionForegr!=Color.Transparent)
                    tb.InvisibleCharsStyle = new InvisibleCharsRenderer(new Pen(Color.FromArgb(128, t.SelectionForegr), 2));
                else
                    tb.InvisibleCharsStyle = new InvisibleCharsRenderer(new Pen(Color.FromArgb(75, t.Foreground), 2));
                //editor.BreakpointLineColor = 

                tb.IndentBackColor  = (t.IndentBackColor .Name != "0") ? t.IndentBackColor  : tb.BackColor;
                tb.LineNumberColor  = (t.LineNumberColor .Name != "0") ? t.LineNumberColor  : Color.FromArgb(150, tb.ForeColor);
                tb.PaddingBackColor = (t.PaddingBackColor.Name != "0") ? t.PaddingBackColor : Color.FromArgb(150, tb.BackColor);
                if (t.LineHighlight.Name != "0")
                    tb.ChangedLineColor = t.LineHighlight;

                tb.SyntaxHighlighter.StyleTheme = t;
                tb.RefreshTheme();
            }
        }

        public static Color MixColor(double balance, Color c1, Color c2) {
            int R = (int)Math.Round(c1.R * balance + c2.R * (1 - balance));
            int G = (int)Math.Round(c1.G * balance + c2.G * (1 - balance));
            int B = (int)Math.Round(c1.B * balance + c2.B * (1 - balance));
            return Color.FromArgb(R, G, B);
        }

        public static Color MediaColor(int top, Color color) {
            int maxVal = 0;
            maxVal = Math.Max(maxVal, color.R);
            maxVal = Math.Max(maxVal, color.G);
            maxVal = Math.Max(maxVal, color.B);
            int dx = top - maxVal;
            int R = color.R + dx;
            int G = color.G + dx;
            int B = color.B + dx;
            R = Math.Max(0, R); R = Math.Min(255, R);
            G = Math.Max(0, G); G = Math.Min(255, G);
            B = Math.Max(0, B); B = Math.Min(255, B);
            return Color.FromArgb(255, R, G, B);
        }

        public static void SetTheme(HMSEditor editor, string name, System.Windows.Forms.ToolStripItemCollection MenuItems) {
            if (Dict.ContainsKey(name)) {
                SetTheme(editor.TB, name);
                Theme t = Dict[name];
                editor.TB.CurrentLineColor = t.LineHighlight;
                //editor.Editor.ChangedLineColor = t.ChangedLines;

                //Color c1 = MixColor(0.5, t.KeywordStyle.GetRTF().ForeColor, t.StringStyle.GetRTF().ForeColor);
                HmsToolTip.ColorBackgrnd = MediaColor(0xF0, t.Background);

                //editor.PopupMenu.BackColor = t.Background;
                //editor.PopupMenu.ForeColor = t.Foreground;

                // Для тёмных тем цвет изменённых строк меняем тоже на более тёмный
                //if (editor.Editor.IndentBackColor.GetBrightness() < 0.5) editor.Editor.ChangedLineColor = ToColor("#024A02");

                foreach (var i in MenuItems) {
                    var b = i as System.Windows.Forms.ToolStripMenuItem;
                    if (b != null) b.Checked = b.Text == name;
                }
                editor.btnMarkChangedLines_Click(null, EventArgs.Empty);
            }
        }

        public static int Init() {
            string data = HMS.ReadTextFromResource("ColorThemes.txt");
            LoadThemesFromString(data);
            int buildinThemes = Dict.Count;
            // И сканируем файлы шаблонов в директории
            if (Directory.Exists(HMS.ThemesDir)) {
                string[] files = Directory.GetFiles(HMS.ThemesDir);
                foreach(string file in files) {
                    LoadFromXml(file);
                }
            }
            //SaveThemesToFile(@"D:\ColorThemes.txt");
            return buildinThemes;
        }

        static public Color GetPixelColor(IntPtr hwnd, int x, int y)
        {
            IntPtr hdc = NativeMethods.GetDC(hwnd);
            uint pixel = NativeMethods.GetPixel(hdc, x, y);
            NativeMethods.ReleaseDC(hwnd, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                            (int)(pixel & 0x0000FF00) >> 8,
                            (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        public static void LoadThemesFromFile(string file) {
            if (!File.Exists(file)) {
                var name = Path.GetFileName(file);
                if (name != null) file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
            }

            if (File.Exists(file)) {
                string data = File.ReadAllText(file);
                LoadThemesFromString(data);
            }
        }

        public static void LoadThemesFromString(string data) {
            INI tini = new INI {
                NoComments = true,
                Text       = data
            };
            Dict.Clear();
            foreach(string section in tini.Dict.Keys) {
                // ReSharper disable once UseObjectOrCollectionInitializer
                Theme tt = new Theme();
                tt.Name = section;
                tt.Background      = ToColor(tini.Get("Background"     , section, ""));
                tt.Caret           = ToColor(tini.Get("Caret"          , section, ""));
                tt.Foreground      = ToColor(tini.Get("Foreground"     , section, ""));
                tt.Invisibles      = ToColor(tini.Get("Invisibles"     , section, ""));
                tt.LineHighlight   = ToColor(tini.Get("LineHighlight"  , section, ""));
                tt.ChangedLines    = ToColor(tini.Get("ChangedLines"   , section, ""));
                tt.Selection       = ToColor(tini.Get("Selection"      , section, ""));
                tt.SelectionBorder = ToColor(tini.Get("SelectionBorder", section, ""));
                tt.SelectionForegr = ToColor(tini.Get("SelectionForegr", section, ""));
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
                tt.PunctuationStyle  = ToStyle2(tini.Get("PunctuationStyle" , section, ""));
                tt.InvisibleStyle    = ToStyle (tini.Get("Invisibles"       , section, ""));
                tt.TypesStyle        = ToStyle2(tini.Get("TypesStyle"       , section, ""));
                Dict.Add(section, tt);
            }

        }

        public static void SaveThemesToFile(string file) {
            StringBuilder sb = new StringBuilder();

            foreach (var name in Dict.Keys) {
                Theme tt = Dict[name];
                sb.AppendLine("[" + name + "]");
                sb.AppendLine("Background="        + ColorToString(tt.Background));
                sb.AppendLine("Caret="             + ColorToString(tt.Caret));
                sb.AppendLine("Foreground="        + ColorToString(tt.Foreground));
                sb.AppendLine("Invisibles="        + ColorToString(tt.Invisibles));
                sb.AppendLine("LineHighlight="     + ColorToString(tt.LineHighlight));
                sb.AppendLine("ChangedLines="      + ColorToString(tt.ChangedLines));
                sb.AppendLine("Selection="         + ColorToString(tt.Selection));
                sb.AppendLine("SelectionBorder="   + ColorToString(tt.SelectionBorder));
                sb.AppendLine("SelectionForegr="   + ColorToString(tt.SelectionForegr));
                sb.AppendLine("LineNumberColor="   + ColorToString(tt.LineNumberColor));
                sb.AppendLine("IndentBackColor="   + ColorToString(tt.IndentBackColor));

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
                sb.AppendLine("PunctuationStyle="  + StyleToString(tt.PunctuationStyle));
                sb.AppendLine("TypesStyle="        + StyleToString(tt.TypesStyle));
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

            var font = s.FontStyle.ToString();

            string result = "{";
            result += (bf == null) ? "" : "fore:"+ColorToString(bf.Color)+";";
            result += (bb == null) ? "" : "back:"+ColorToString(bb.Color)+";";
            result += string.IsNullOrEmpty(font) ? "" : "font:"+ font + ";";
            return result+"}";
        }

        private static Style ToStyle2(string styleString) {
            //{fore:#8ABBE7;fore:#FFFFFF;font:Bold, Italic;}
            string fore    = Regex.Match(styleString, "fore:(.*?);").Groups[1].Value;
            string back    = Regex.Match(styleString, "back:(.*?);").Groups[1].Value;
            bool bold      = Regex.IsMatch(styleString, "font:.*?Bold"     , RegexOptions.IgnoreCase);
            bool italic    = Regex.IsMatch(styleString, "font:.*?Italic"   , RegexOptions.IgnoreCase);
            bool underline = Regex.IsMatch(styleString, "font:.*?Underline", RegexOptions.IgnoreCase);
            if ((fore.Length + back.Length == 0) && !bold && !italic && !underline) return null;
            return ToStyle(fore, back, bold, italic, underline);
        }

        private static Style ToStyle(string foreVal, string backVal = "", bool bold = false, bool italic = false, bool underline = false) {
            FontStyle fs = FontStyle.Regular;
            if (italic   ) fs |= FontStyle.Italic;
            if (bold     ) fs |= FontStyle.Bold;
            if (underline) fs |= FontStyle.Underline;
            Brush backBrush = null;
            Brush foreBrush = null;
            if (!string.IsNullOrEmpty(foreVal))
                foreBrush = new SolidBrush(ToColor(foreVal));
            if (!string.IsNullOrEmpty(backVal))
                backBrush = new SolidBrush(ToColor(backVal));
            Style s = new TextStyle(foreBrush, backBrush, fs);
            return s;
        }

        public static Color ToColor(string val) {
            if (string.IsNullOrEmpty(val) || val == "null") return Color.Transparent;
            if (val.Length == 9) {
                string aval = val.Substring(7, 2);
                val = "#" + aval + val.Substring(1, 6);
            }
            ColorConverter cc = new ColorConverter();
            var fromString = cc.ConvertFromString(val);
            if (fromString != null) {
                return (Color)fromString;
            }
            return Color.Transparent;
        }

    }
}
