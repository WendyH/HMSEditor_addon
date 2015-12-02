using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
	public partial class FormRename: Form {
		public string OldVarName;
		public string NewVarName { get { return textBoxName .Text; } set { textBoxName .Text = value; } }
		public string Context    { get { return labelContext.Text; } set { labelContext.Text = value; } }
		public string ExcludeFunctions = "";
		public string LocalFunction    = "";
		public FastColoredTextBox TextBox { get { return fastColoredTextBox1; } }
		public int LineNo4Goto = 0;
		public int CharNo4Goto = 0;

		public List<Range> OrigRanges = new List<Range>();
		public string TextLines = "";

		private static Regex regexAllowedChars = new Regex(@"^\w+$", RegexOptions.Compiled);

		private ToolTip ContextToolTip = new ToolTip();

		public FormRename() {
			InitializeComponent();
			ContextToolTip.AutoPopDelay = 20000;
		}

		private void FormRename_Load(object sender, EventArgs e) {
			labelFounded.Text = "";
			NewVarName = OldVarName;
			textBoxName.SelectAll();
			textBoxName.Focus();
			labelContext.Left = textBoxName.Left + textBoxName.Width - labelContext.Width;
			
			if (ExcludeFunctions != "") {
				ContextToolTip.ToolTipTitle = "Исключённые из поиска функции и процедуры";
				string msg = "Исключены все те функции, где локально было переопределено\nданное наименование как переменная или параметр:\n";
				ContextToolTip.SetToolTip(labelContext, msg + ExcludeFunctions);
			} else if (LocalFunction!="") {
				ContextToolTip.ToolTipTitle = "Поиск только в локальной видимости";
				ContextToolTip.SetToolTip(labelContext, "Поиск происходит только в локальной видимости функции "+ LocalFunction + ",\nгде была определена переменная или параметр с данным наименованием.");
			} else {
				ContextToolTip.ToolTipTitle = "Поиск по всему скрипту";
				ContextToolTip.SetToolTip(labelContext, "Поиск происходит во всём тексте скрипта, включая контекст всех функций или процедур.");
			}
		}

		private void textBoxName_TextChanged(object sender, EventArgs e) {
			Place oldCaret = TextBox.Selection.Start;
			TextBox.Range.ClearStyle(TextBox.SelectionStyle);
			TextBox.Text = TextLines;
			TextBox.Selection.BeginUpdate();
			TextBox.BeginUpdate();
			List<Range> ranges = new List<Range>();
			int iLine;
			foreach (Range range in OrigRanges) {
				iLine = range.StoredLineNo;
				Line line = TextBox.GetRealLine(iLine);
				line.LineNo = range.Start.iLine + 1;
				Range r = new Range(TextBox, range.Start.iChar, iLine, range.End.iChar, iLine);
				ranges.Add(r);
			}
			TextBox.TextSource.Manager.ExecuteCommand(new ReplaceTextCommand(TextBox.TextSource, ranges, NewVarName));
			HighlightWord(NewVarName);
			TextBox.Selection.Start = oldCaret;
			TextBox.NeedRecalc(true);
			TextBox.Invalidate();
			TextBox.Selection.EndUpdate();
			TextBox.EndUpdate();

			int count = OrigRanges.Count;
			if (NewVarName != OldVarName)
				labelFounded.Text = getNumText(count, new[] { "Произведена", "Произведено", "Произведено" }) + " " + count.ToString() + " " + getNumText(count, new[] { "замена", "замены", "замен" });
			else
				labelFounded.Text = "Найдено " + count.ToString() + " " + getNumText(count, new[] { "совпадение", "совпадения", "совпадений" });
		}

		private void HighlightWord(string word) {
			TextBox.Range.ClearStyle(TextBox.SelectionStyle);
			if (word == "") return;
			var range = TextBox.Range.Clone();
			foreach (var r in range.GetRangesByLines("\\b" + word + "\\b", RegexOptions.IgnoreCase)) {
				if (TextBox.SyntaxHighlighter.IsCommentOrString(r)) continue;
				r.SetStyle(TextBox.SelectionStyle);
			}
		}

		private void textBoxName_KeyPress(object sender, KeyPressEventArgs e) {
			if (System.Char.IsDigit  (e.KeyChar)) return;
			if (System.Char.IsControl(e.KeyChar)) return;
			e.Handled = !regexAllowedChars.IsMatch(e.KeyChar.ToString());
		}

		/// <summary>
		/// Преобразование падежей, в зависимости от числа. Например: WH.getNumText(nCount, new[] {"замена", "замены", "замен" }) 
		/// </summary>
		/// <param name="num">Число</param>
		/// <param name="words">Массив трёх строк, разного склонения. Напр.: ["День", "Дня", "Дней"]</param>
		/// <returns></returns>
		public static string getNumText(int num, string[] words) {
			int x = num % 100;
			if (x > 20) x = x % 10;
			if (x > 4) return words[2];
			else
				switch (x) {
					case 1: return words[0];
					case 0: return words[2];
					default: return words[1];
				}
		}

		private void btnOK_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.OK;
			Close();
		}

		private void fastColoredTextBox1_Load(object sender, EventArgs e) {

		}

		private void fastColoredTextBox1_MouseDoubleClick(object sender, MouseEventArgs e) {
			if (e.X < (TextBox.LeftIndent - 4)) {
				int iFirstLine = TextBox.YtoLineIndex();
				int yFirstLine = TextBox.LineInfos[iFirstLine].startY - TextBox.VerticalScroll.Value;
				int iLine = (int)((e.Y - yFirstLine) / (TextBox.Font.Height - 1)) + iFirstLine;
				try {
					Line line    = TextBox.GetRealLine(iLine);
					LineNo4Goto  = line.LineNo-1;
					// search char position
					foreach(var range in OrigRanges) {
						if (range.Start.iLine == LineNo4Goto) {
							CharNo4Goto = range.Start.iChar;
						}
					}
					DialogResult = DialogResult.Retry;
					Close();
				} catch { }
			}
		}
	}
}
