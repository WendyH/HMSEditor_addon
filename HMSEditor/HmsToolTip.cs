//#define debug
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using HMSEditorNS;
using System.Collections.Generic;
using System;
// ReSharper disable InconsistentNaming

// ReSharper disable once CheckNamespace
namespace FastColoredTextBoxNS {

    /// <summary>
    /// Класс всплывающей подсказки (Tooltip) с отображением дополнительного текста, возможностью разметки и подсветки ключевых слов и классов HMS.
    /// </summary>
    public sealed class HmsToolTip: ToolTip {
        #region Static computed field
        private static Regex regexPhrases        = new Regex(@"(.*?[,;]|.*?$)", RegexOptions.Multiline);
        private static Regex regexWords          = new Regex(@"(<.*?>|[\w-]+|[^\w-<]+)([\.,;]?)");
        private static Regex regexSplitFuncParam = new Regex("[,;]"       );
        private static Regex regexFunctionParams = new Regex(@"\(([^\)]+)");
        private static Size Margin        = new Size(6, 4);
        private static Font FontTitle     = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
        private static Font FontText      = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
        private static Font FontTextBold  = new Font("Segoe UI", 9.75f, FontStyle.Bold   , GraphicsUnit.Point);
        private static Font FontHelp      = new Font("Segoe UI", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
        private static Font FontItalic    = new Font("Segoe UI", 9.75f, FontStyle.Italic , GraphicsUnit.Point);
        private static Font FontValue     = new Font("Arial", 9f);
        private static Color colorText    = Color.Black;
        private static Color colorKeyword = Color.Blue;
        private static Color colorRed     = Color.OrangeRed;
        private static Color colorParam   = Color.DarkOrange;
        private static Color colorClass   = Color.FromArgb(0x2B91AF);
        private static Color colorHelp    = Color.FromArgb(0x247256);
        private static Color colorString  = Color.FromArgb(0xAA5C36);
        private static Color colorValue   = Color.FromArgb(0xAA5C36);
        private static Size  MaxSize      = new Size(590, 500);
        private static TextFormatFlags tf = TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsClipping;
        private static int MaxValueLenght = 100;
        public static Color ColorBackgrnd = Color.FromArgb(0xE4E5F0);
        #endregion Static computed field

        private string  _value   = "";
        public string    Help    = "";
        public Rectangle Bounds;
        public int       iLine   = 0;
        public bool      Visible;
        public Rectangle ParentRect = new Rectangle();
        public HMSItem   HmsItem;
        //private long LastTS = 0;
        private List<WordStyle> OwnWords = new List<WordStyle>();

        public string Value {
            get { return _value; }
            set { _value = value.Length > MaxValueLenght ? value.Substring(0, MaxValueLenght) : value; }
        }

        public HmsToolTip() {
            OwnerDraw = true;
            Popup    += OnPopup;
            Draw     += OnDraw;
            UseFading    = false;
            UseAnimation = false;
            ShowAlways   = false;
            ReshowDelay  = 100000; 
            if (HMS.PFC.Families.Length > 0) {
                FontTitle    = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Regular, GraphicsUnit.Point);
                FontText     = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Regular, GraphicsUnit.Point);
                FontTextBold = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Bold   , GraphicsUnit.Point);
                FontHelp     = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Regular, GraphicsUnit.Point);
                FontItalic   = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Italic , GraphicsUnit.Point);
            }
        }

        protected override void Dispose(bool disposing) {
            HmsItem  = null;
            OwnWords = null;
            base.Dispose(disposing);
        }

        public new void Hide(IWin32Window window) {
            HmsItem = null;
            Visible = false;
            var win = window as Control;
            if (win != null && win.InvokeRequired) {
                win.Invoke((MethodInvoker)delegate { base.Hide(win); });
            } else {
                if (win != null) base.Hide(win);
            }
        }

        public static void PrepareFastDraw(IWin32Window win, HMSItem item) {
            Graphics g = Graphics.FromHwnd(win.Handle);
            PrepareFastDraw(item, g);
        }

        public static string GetTextWithHelp(HMSItem item) {
            string help = "";
            if (!string.IsNullOrEmpty(item.ToolTipTitle)) {
                help = GetText(item);
                if (item.Params.Count > 0) {
                    help += "\n-----------------\nПараметры:\n";
                    foreach (var param in item.Params) {
                        help += Regex.Replace(param, "^(\\w+)", "<p>$1</p>") + "\r\n";
                    }
                }
                if (item.IsClass) {
                    help += "\n";
                    if (item.ClassInfo.StaticItems.Count > 0) {
                        help += "<h>\nСтатические методы (конструктор):\n<t>";
                        foreach (var subitem in item.ClassInfo.StaticItems) {
                            string title = subitem.ToolTipTitle;
                            title = Regex.Replace(title, @"\b(" + subitem.MenuText + @")\b", "<b>$1</b>");
                            help += title + "\n";
                        }
                    }
                    if (item.ClassInfo.MemberItems.Count > 0) {
                        help += "<h>\nСвойства и методы:\n<t>";
                        foreach (var subitem in item.ClassInfo.MemberItems) {
                            string title = subitem.ToolTipTitle;
                            title = Regex.Replace(title, @"\b("+ subitem.MenuText + @")\b", "<b>$1</b>");
                            help += title + "\n";
                        }
                    }
                }
            }
            return help.TrimEnd();
        }

        public static void PrepareFastDraw(HMSItem item, Graphics g) {
            if (item.Words.Count > 0) return;
            /*
            // Prepare for rtf text for help panel
            RichTextBox HelpTextBox = new RichTextBox();
            if (item.Rtf == "") {
                HelpTextBox.Text = "";
                if (!string.IsNullOrEmpty(item.ToolTipTitle)) {
                    string help = GetTextWithHelp(item);
                    HmsToolTip.WriteWords(HelpTextBox, help);
                }
                item.Rtf = HelpTextBox.Rtf;
            }
            */
            // prepeare for tooltip
            float heightCorrection;
            item.ToolTipTitle = CalcPhrasesBreaks(g, item.ToolTipTitle);
            string text  = GetText(item, out heightCorrection);
            //if (text.IndexOf("HmsRegExMatch3") > 0) {
            //    heightCorrection = 1;
            //}
            Size size  = TextRenderer.MeasureText(text, FontText, MaxSize, TextFormatFlags.WordBreak);
            size.Width  += Margin.Width  * 2;
            size.Height += Margin.Height * 2 + (int)heightCorrection;
            item.ToolTipSize = WriteWords(text, new Rectangle(0, 0, size.Width, size.Height), g, item.Words);
        }

        public void Show(HMSItem item, IWin32Window win, Point point, int duration) {
            // Если указан прямоугольник родителя (меню подсказок), то проверяем не перекрываем ли мы его.
            // Отодвигаем вверх, если перекрываем по ширине.
            if (item.Words.Count == 0) PrepareFastDraw(win, item);
            if (ParentRect.Width > 0) {
                Control ctrl = win as Control;
                if (ctrl != null) {
                    Rectangle tipRect = ctrl.RectangleToScreen(new Rectangle(point.X, point.Y, item.ToolTipSize.Width, item.ToolTipSize.Height));
                    Rectangle scrRect = Screen.FromHandle(win.Handle).Bounds;
                    if ((tipRect.X + tipRect.Width) > scrRect.Width) {
                        point.X = ParentRect.X - tipRect.Width - 2;
                    }
                }
            }
            HmsItem = item;
            Show(" ", win, point, duration);
        }

        private void OnPopup(object sender, PopupEventArgs e) // use this event to set the size of the tool tip
        {
            if (HmsItem != null)
                e.ToolTipSize = HmsItem.ToolTipSize;
            else {
                Graphics g = Graphics.FromHwnd(e.AssociatedControl.Handle);
                float heightCorrection;
                string text  = GetText(GetToolTip(e.AssociatedControl), out heightCorrection);
                //Size size    = TextRenderer.MeasureText(g, text, FontText, MaxSize, TextFormatFlags.WordBreak);
                //size.Width  += Margin.Width  * 2;
                //size.Height += Margin.Height * 2 + (int)heightCorrection;
                e.ToolTipSize = WriteWords(text, new Rectangle(0, 0, MaxSize.Width, MaxSize.Height), g, OwnWords, true);
                OwnWords.Clear();
                Bounds = new Rectangle(0, 0, e.ToolTipSize.Width, e.ToolTipSize.Height);
                WriteWords(text, Bounds, g, OwnWords);
            }
        }

        private string GetText(string tooltipText, out float heightCorrection) {
            string s1 = ToolTipTitle.Trim();
            string s2 = tooltipText .Trim();
            string s3 = Help .Trim();
            string s4 = Value.Trim();
            return StructuredText(out heightCorrection, s1, s2, s3, s4);
        }

        public static string GetText(HMSItem item) {
            float heightCorrection;
            return GetText(item, out heightCorrection);
        }

        public static string GetText(HMSItem item, out float heightCorrection) {
            string s1 = item.ToolTipTitle.Trim();
            string s2 = item.ToolTipText .Trim();
            string s3 = item.Help .Trim();
            string s4 = item.Value.Trim();
            return StructuredText(out heightCorrection, s1, s2, s3, s4);
        }


        private static string StructuredText(out float heightCorrection, string s1, string s2, string s3, string s4) {
            heightCorrection = 0;
            string text = "";
            if (s1.Length > 0) { text += "<t>" + s1  + "</b>"; heightCorrection += 3; }
            if (s2.Length > 0) { text += "<id>\n"    + s2;     heightCorrection += 3; }
            if (s3.Length > 0) { text += "<id>\n<h>" + s3;     heightCorrection += 3; }
            if (s4.Length > 0) { text += "<hr>\n<v>" + s4;     heightCorrection += 7; }
            return text.Trim();
        }

        private void OnDraw(object sender, DrawToolTipEventArgs e) // use this event to customise the tool tip
        {
#if debug
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
            //long ts = System.Diagnostics.Stopwatch.GetTimestamp();
            //if (ts - LastTS < 5000) return;
            Bounds = e.Bounds; // Store show Bounds
            Graphics            g = e.Graphics;
            LinearGradientBrush b = new LinearGradientBrush(Bounds, Color.WhiteSmoke, Color.FromArgb(255, ColorBackgrnd), 90f);
            g.FillRectangle(b, Bounds);
            e.DrawBorder();
            g.SmoothingMode = SmoothingMode.HighQuality;
            if (HmsItem != null && HmsItem.Words.Count > 0) {
                DrawFast(g, HmsItem.Words);
            } else if (OwnWords.Count > 0) {
                DrawFast(g, OwnWords);
            } else {
                float i;
                WriteWords(GetText(e.ToolTipText, out i), Bounds, g);
            }
            b.Dispose();
            Visible = true;
            HmsItem = null;
            //LastTS = System.Diagnostics.Stopwatch.GetTimestamp();
#if debug
            sw.Stop();
            System.Console.WriteLine("OnDraw: " + sw.ElapsedMilliseconds);
#endif
        }

        public void ShowFunctionParams(HMSItem item, int nParam, IWin32Window window, Point p) {
            string title      = item.ToolTipTitle;
            string parameters = title;
            string activparam = "";
            string paramtype  = "";
            string paramHelp  = item.Help;
            if ((nParam > 0) && (nParam <= item.Params.Count)) paramHelp = item.Params[nParam - 1];
            Match m = regexFunctionParams.Match(title);
            if (m.Success) parameters = m.Groups[1].Value;
            string[] prmtrs = regexSplitFuncParam.Split(parameters);
            if ((nParam > 0) && (nParam <= prmtrs.Length)) activparam = prmtrs[nParam-1];
            if (activparam.Length > 0) {
                int ind = parameters.IndexOf(activparam, StringComparison.Ordinal);
                paramtype = HMS.GetType(parameters.Substring(ind));
            }
            HMS.CurrentParamType = paramtype.ToLower();
            if (activparam.Length > 0) title = title.Replace(activparam, "<p>" + activparam + "</p>");
            if (paramHelp.Length == 0) paramHelp = " ";
            Help = paramHelp;
            ToolTipTitle = title;
            if (((Control)window).InvokeRequired) {
                ((Control)window).Invoke((MethodInvoker)delegate {
                    //ToolTipTitle = CalcPhrasesBreaks(window, title);
                    if (!Visible) Show(" ", window, p);
                });
            } else {
                //ToolTipTitle = CalcPhrasesBreaks(window, title);
                if (!Visible) Show(" ", window, p);
            }
        }

        private void DrawFast(Graphics g, List<WordStyle> words) {
            int count = words.Count;
            for (int i=0; i< count; i++) {
                WordStyle word = words[i];
                TextRenderer.DrawText(g, word.Text, word.Font, word.Point, word.Color, tf);
            }
        }

        private static string CalcPhrasesBreaks(Graphics g, string text) {
            string newText = "";
            int x = Margin.Width;
            string[] lines = text.Split('\n');
            for (int iline = 0; iline < lines.Length; iline++) {
                MatchCollection mc = regexPhrases.Matches(lines[iline]);
                foreach (Match m in mc) {
                    string word = m.Groups[1].Value;
                    if (word.StartsWith("<")) { newText += word; continue; }
                    var wordSize = TextRenderer.MeasureText(g, Regex.Replace(word, "<.*?>", ""), FontTitle, MaxSize, tf);
                    if (wordSize.Width > (MaxSize.Width - x - Margin.Width * 2 - 12)) { x = Margin.Width; newText += "\n"; word = word.TrimStart(); }
                    x += wordSize.Width;
                    newText += word;
                }
                newText += "\n";
            }
            if (newText.Length > 0) newText = newText.Substring(0, newText.Length - 1);
            return newText;
        }

        public static string GetRtf(string text) {
            MyRichTextBox tb = new MyRichTextBox();
            WriteWords(text, new Rectangle(), null, null, true, tb);
            return tb.Rtf;
        }

        public static Size WriteWords(string text, Rectangle bounds, Graphics g, List<WordStyle> words = null, bool notShow = false, MyRichTextBox tb = null) {
            if (text.Length == 0) return new Size();
            Point  point      = new Point(Margin.Width, Margin.Height);
            Font   font       = FontText;
            Color  color      = colorText;
            Color  prevColor  = colorText;
            Size   wordSize   = new Size();
            int    prevHeight = 0;
            int    maxWidth   = 0;
            string[] lines = text.Split('\n');
            for (int iline = 0; iline < lines.Length; iline++) {
                MatchCollection mc = regexWords.Matches(lines[iline]);
                foreach (Match m in mc) {
                    string word  = m.Groups[1].Value;
                    string word2 = m.Groups[2].Value;
                    if (word == "<t>" ) { font = FontTitle   ; color = colorText  ; continue; }
                    if (word == "<b>" ) { font = FontTextBold; continue; }
                    if (word == "</b>") { font = FontText    ; continue; }
                    if (word == "<h>" ) { font = FontHelp    ; color = colorHelp  ; continue; }
                    if (word == "<v>" ) { font = FontValue   ; color = colorValue ; continue; }
                    if (word == "<i>" ) { font = FontItalic  ; continue; }
                    if (word == "</i>") { font = FontText    ; continue; }
                    if (word == "<s>" ) { prevColor = color  ; color = colorString; continue; }
                    if (word == "</s>") { color = prevColor  ; continue; }
                    if (word == "<r>" ) { prevColor = color  ; color = colorRed   ; continue; }
                    if (word == "</r>") { color = prevColor  ; continue; }
                    if (word == "<p>" ) { prevColor = color  ; font = FontTextBold; color = colorParam; continue; }
                    if (word == "</p>") { color = prevColor  ; font = FontTitle   ; continue; }
                    if (word == "<id>") { point.Y += 3       ; continue; }
                    bool notColored;
                    if (tb!=null) {
                        notColored = (color == colorString) || (color == colorValue);
                        if      (!notColored && isKeyWord(word)) tb.AppendText(word, colorKeyword, font);
                        else if (!notColored && isClass  (word)) tb.AppendText(word, colorClass  , font);
                        else                                     tb.AppendText(word, color       , font);
                        tb.AppendText(word2, color, font);
                        continue;
                    }
                    wordSize = TextRenderer.MeasureText(g, word+word2, font, MaxSize, tf);
                    maxWidth = Math.Max(maxWidth, point.X+wordSize.Width);
                    if (wordSize.Width > (bounds.Width - point.X - Margin.Width)) { point.X = Margin.Width; point.Y += prevHeight; word = word.TrimStart(); }
                    if (word.Length == 0) continue;
                    if (word == "<hr>") {
                        float y = point.Y + prevHeight + 2;
                        if (!notShow) g.DrawLine(Pens.Gray, Margin.Width, y, bounds.Width - Margin.Width, y);
                        point.Y += 4;
                        continue;
                    }
                    if (!notShow) { 
                        notColored = (color == colorString) || (color == colorValue);
                        if      (!notColored && isKeyWord(word)) DrawText(g, word, font, point, colorKeyword, words);
                        else if (!notColored && isClass  (word)) DrawText(g, word, font, point, colorClass  , words);
                        else                                     DrawText(g, word, font, point, color       , words);
                        if (word2.Length > 0) {
                            point.X += wordSize.Width - 4;
                            DrawText(g, word2, font, point, color, words);
                            point.X -= wordSize.Width - 4;
                        }
                    }
                    point.X   += wordSize.Width;
                    prevHeight = wordSize.Height;
                }
                point.Y += wordSize.Height; point.X = Margin.Width;
                if (tb != null && tb.IsHandleCreated && !tb.IsDisposed) {
                    tb.AppendText("\r\n");
                    if (iline == 0) tb.AppendText("\r\n");
                }
            }
            return new Size(maxWidth+Margin.Width, point.Y+Margin.Height);
        }

        private static void DrawText(Graphics g, string text, Font font, Point point, Color color, List<WordStyle> words) {
            if (words == null) {
                TextRenderer.DrawText(g, text, font, point, color, tf);
            } else {
                WordStyle s = new WordStyle(text, font, point, color);
                if (words.Count > 0) {
                    if (words[words.Count - 1].SameStyle(s)) {
                        words[words.Count - 1].Text += text;
                        return;
                    }
                }
                words.Add(s);
            }
        }

        private static bool isKeyWord(string word) {
            return (HMS.HmsTypesString+"const|var|").IndexOf("|" + word.ToLower() + "|", StringComparison.Ordinal) >= 0;
        }

        private static bool isClass(string word) {
            return HMS.ClassesString.IndexOf("|" + word.ToLower() + "|", StringComparison.Ordinal) >= 0;
        }

    }

    public class WordStyle {
        public string Text;
        public Font   Font;
        public Point  Point;
        public Color  Color;

        public WordStyle(string text, Font font, Point point, Color color) {
            Text  = text;
            Font  = font;
            Point = point;
            Color = color;
        }

        public bool SameStyle(WordStyle o) {
            return Equals(Font, o.Font) && (Color == o.Color) && (Point.Y == o.Point.Y);
        }

        public void Draw(Graphics g) {
            TextRenderer.DrawText(g, Text, Font, Point, Color);
        }
    }
}
