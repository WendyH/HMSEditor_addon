using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using HMSEditorNS;

namespace FastColoredTextBoxNS {
	/// <summary>
	/// Класс всплывающей подсказки (Tooltip) с отображением дополнительного текста, возможностью разметки и подсветки ключевых слов и классов HMS.
	/// </summary>
	public sealed class HmsToolTip: ToolTip {
		#region Static computed field
		private static Regex regexWords          = new Regex(@"(<.*?>|\w+|.)", RegexOptions.Compiled);
		private static Regex regexSplitFuncParam = new Regex("[,;]"          , RegexOptions.Compiled);
		private static Regex regexFunctionParams = new Regex(@"\(([^\)]+)"   , RegexOptions.Compiled);
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
//		private static Color colorString  = Color.Brown;
//		private static Color colorKeyword = Color.FromArgb(0x04396C);
		private static Color colorClass   = Color.FromArgb(0x2B91AF);
		private static Color colorHelp    = Color.FromArgb(0x247256);
		private static Color colorString  = Color.FromArgb(0xAA5C36);
		private static Color colorValue   = Color.FromArgb(0xAA5C36);
		private static Size  MaxSize      = new Size(760, 600);
		private static TextFormatFlags tf = TextFormatFlags.NoPadding | TextFormatFlags.PreserveGraphicsClipping;
		private static int MaxValueLenght = 100;
		#endregion Static computed field

		private string  _value   = "";
		public string    Help    = "";
		public Rectangle Bounds  = new Rectangle();
		public int       iLine   = 0;
		public bool      Visible = false;

		public string Value {
			get { return _value; }
			set {
				if (value.Length > MaxValueLenght)
					_value = value.Substring(0, MaxValueLenght);
				else
					_value = value;
			}
		}

		public HmsToolTip() {
			this.OwnerDraw    = true;
			this.Popup       += new PopupEventHandler(this.OnPopup);
			this.Draw        += new DrawToolTipEventHandler(this.OnDraw);
			this.UseFading    = false;
			this.UseAnimation = false;
			this.ShowAlways   = false;
			if (HMS.PFC.Families.Length > 0) {
				FontTitle    = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Regular, GraphicsUnit.Point);
				FontText     = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Regular, GraphicsUnit.Point);
				FontTextBold = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Bold   , GraphicsUnit.Point);
				FontHelp     = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Regular, GraphicsUnit.Point);
				FontItalic   = new Font(HMS.PFC.Families[0], 9.75f, FontStyle.Italic , GraphicsUnit.Point);
			}
		}

		public new void Hide(IWin32Window win) {
			Visible = false;
			base.Hide(win);
		}

		public void OnLostFocus() {
			Visible = false;
		}

		private void OnPopup(object sender, PopupEventArgs e) // use this event to set the size of the tool tip
		{
			float heightCorrection = 0;
			string text  = GetText(GetToolTip(e.AssociatedControl), out heightCorrection);
			Size   size  = TextRenderer.MeasureText(text, FontTextBold, MaxSize, TextFormatFlags.WordBreak);
			size.Width  += Margin.Width  * 2;
			size.Height += Margin.Height * 2 + (int)heightCorrection;
			e.ToolTipSize = size;
		}

		private string GetText(string tooltipText, out float heightCorrection) {
			heightCorrection = 0;
			string s1 = ToolTipTitle.Trim();
			string s2 = tooltipText.Trim();
			string s3 = Help.Trim();
			string s4 = Value.Trim();
			string text = "";
			if (s1.Length > 0) { text += "<t>" + s1  + "</b>"; heightCorrection += 3; }
			if (s2.Length > 0) { text += "<id>\n"    + s2;     heightCorrection += 3; }
			if (s3.Length > 0) { text += "<id>\n<h>" + s3;     heightCorrection += 3; }
			if (s4.Length > 0) { text += "<hr>\n<v>" + s4;     heightCorrection += 7; }
			return text.Trim();
		}

		private void OnDraw(object sender, DrawToolTipEventArgs e) // use this event to customise the tool tip
		{
			               Bounds = e.Bounds; // Store show Bounds
			HmsToolTip        tip = sender as HmsToolTip;
			Graphics            g = e.Graphics;
			LinearGradientBrush b = new LinearGradientBrush(Bounds, Color.White, Color.FromArgb(0xE4, 0xE5, 0xF0), 90f);
			g.FillRectangle(b, Bounds);
			e.DrawBorder();
			g.SmoothingMode = SmoothingMode.HighQuality;
            float i;
			WriteWords(GetText(e.ToolTipText, out i), Bounds, g);
			Visible = true;
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
				int ind = parameters.IndexOf(activparam);
				paramtype = HMS.GetType(parameters.Substring(ind));
			}
			HMS.CurrentParamType = paramtype.ToLower();
			if (activparam.Length > 0) title = title.Replace(activparam, "<b>" + activparam + "</b>");
			if (paramHelp.Length == 0) paramHelp = " ";
			Help = paramHelp;
			ToolTipTitle = title;
			if (!Visible) Show(" ", window, p);
		}

		private void WriteWords(string text, Rectangle bounds, Graphics g) {
			if (text.Length == 0) return;
			Point  point      = new Point(Margin.Width, Margin.Height);
			Font   font       = FontText;
			Color  color      = colorText;
			Color  prevColor  = colorText;
			Size   wordSize   = new Size();
			int    prevHeight = 0;
			bool   notColored = false;
			string[] lines = text.Split('\n');
			for (int iline = 0; iline < lines.Length; iline++) {
				MatchCollection mc = regexWords.Matches(lines[iline]);
				foreach (Match m in mc) {
					string word = m.Groups[1].Value;
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
					if (word == "<id>") { point.Y += 3       ; continue; }
					wordSize = TextRenderer.MeasureText(g, word, font, MaxSize, tf);
					if (wordSize.Width > (bounds.Width - point.X - Margin.Width)) { point.X = Margin.Width; point.Y += prevHeight; }
					if (word == "<hr>") {
						float y = point.Y + prevHeight + 2;
						g.DrawLine(Pens.Gray, Margin.Width, y, bounds.Width - Margin.Width, y);
						point.Y += 4;
						continue;
					}
					notColored = (color == colorString) || (color == colorValue);
					if      (!notColored && isKeyWord(word)) TextRenderer.DrawText(g, word, font, point, colorKeyword, tf);
					else if (!notColored && isClass  (word)) TextRenderer.DrawText(g, word, font, point, colorClass  , tf);
					else                                     TextRenderer.DrawText(g, word, font, point, color       , tf);
					point.X   += wordSize.Width;
					prevHeight = wordSize.Height;
				}
				point.Y += wordSize.Height; point.X = Margin.Width;
			}
		}

		private static bool isKeyWord(string word) {
			return (HMS.HmsTypesString + HMS.KeywordsString).IndexOf("|" + word.ToLower() + "|") >= 0;
		}

		private static bool isClass(string word) {
			return HMS.ClassesString.IndexOf("|" + word.ToLower() + "|") >= 0;
		}
	}
}
