using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Permissions;

namespace FastColoredTextBoxNS
{
    public partial class ReplaceForm : Form
    {
        FastColoredTextBox tb;
        bool firstSearch = true;
        Place startPlace;
		ToolTip tooltip1 = new ToolTip();
		ToolTip tooltip2 = new ToolTip();
		ToolTip tooltip3 = new ToolTip();
		string MBCaption = "HMS Editor - Поиск и замена";

		public ReplaceForm(FastColoredTextBox tb)
        {
            InitializeComponent();
            this.tb = tb;
			tooltip1.ToolTipIcon = ToolTipIcon.Info;
			tooltip1.ToolTipTitle = "Регистрозависимый поиск";
            tooltip1.SetToolTip(cbMatchCase, "Поиск будет осуществляться согласно указанному регистру символов");

			tooltip2.ToolTipIcon = ToolTipIcon.Info;
			tooltip2.ToolTipTitle = "Поиск отдельных слов";
			tooltip2.SetToolTip(cbWholeWord, "Поиск указанного цельного отдельного слова.\nБудут найдены только слова, стоящие отдельно, не входящие в состав другого.");

			tooltip3.ToolTipIcon = ToolTipIcon.Info;
			tooltip3.ToolTipTitle = "Regex - регулярные выражения";
			tooltip3.SetToolTip(cbRegex    , "Поиск по указанному регулярному вырежению.\nНапример: MyVar\\s*?=");
		}

		private void btClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btFindNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Find(tbFind.Text))
					MessageBox.Show("Указанный текст не найден: \n\n"+tbFind.Text, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			} catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public List<Range> FindAll(string pattern)
        {
            var opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
            if (!cbRegex.Checked)
                pattern = Regex.Escape(pattern);
            if (cbWholeWord.Checked)
                pattern = "\\b" + pattern + "\\b";
            //
            var range = tb.Selection.IsEmpty? tb.Range.Clone() : tb.Selection.Clone();
            //
            var list = new List<Range>();
            foreach (var r in range.GetRangesByLines(pattern, opt))
                list.Add(r);

            return list;
        }

        public bool Find(string pattern)
        {
            RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
            if (!cbRegex.Checked)
                pattern = Regex.Escape(pattern);
            if (cbWholeWord.Checked)
                pattern = "\\b" + pattern + "\\b";
            //
            Range range = tb.Selection.Clone();
            range.Normalize();
            //
            if (firstSearch)
            {
                startPlace = range.Start;
                firstSearch = false;
            }
            //
            range.Start = range.End;
            if (range.Start >= startPlace)
                range.End = new Place(tb.GetLineLength(tb.LinesCount - 1), tb.LinesCount - 1);
            else
                range.End = startPlace;
            //
            foreach (var r in range.GetRangesByLines(pattern, opt))
            {
                tb.Selection.Start = r.Start;
                tb.Selection.End = r.End;
                tb.DoSelectionVisible();
                tb.Invalidate();
                return true;
            }
            if (range.Start >= startPlace && startPlace > Place.Empty)
            {
                tb.Selection.Start = new Place(0, 0);
                return Find(pattern);
            }
            return false;
        }

        private void tbFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                btFindNext_Click(sender, null);
            if (e.KeyChar == '\x1b')
                Hide();
        }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) // David
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ReplaceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            this.tb.Focus();
        }

        private void btReplace_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb.SelectionLength != 0)
                if (!tb.Selection.ReadOnly)
                    tb.InsertText(tbReplace.Text);
                btFindNext_Click(sender, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btReplaceAll_Click(object sender, EventArgs e)
        {
            try
            {
                tb.Selection.BeginUpdate();

                //search
                var ranges = FindAll(tbFind.Text);
                //check readonly
                var ro = false;
                foreach (var r in ranges)
                    if (r.ReadOnly)
                    {
                        ro = true;
                        break;
                    }
                //replace
                if (!ro)
                if (ranges.Count > 0)
                {
                    tb.TextSource.Manager.ExecuteCommand(new ReplaceTextCommand(tb.TextSource, ranges, tbReplace.Text));
                    tb.Selection.Start = new Place(0, 0);
                }
                //
                tb.Invalidate();
                MessageBox.Show(getNumText(ranges.Count, new[] { "Произведена", "Произведено", "Произведено" }) + " " + ranges.Count + " " + getNumText(ranges.Count, new[] {"замена", "замены", "замен" }), MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            tb.Selection.EndUpdate();
        }

        protected override void OnActivated(EventArgs e)
        {
            tbFind.Focus();
            ResetSerach();
        }

        void ResetSerach()
        {
            firstSearch = true;
        }

        private void cbMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            ResetSerach();
        }

		private void ReplaceForm_Load(object sender, EventArgs e) {

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
	}
}
