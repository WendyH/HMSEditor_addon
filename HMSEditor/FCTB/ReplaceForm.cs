using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Permissions;
using HMSEditorNS;
using HMSEditorNS.Properties;

// ReSharper disable once CheckNamespace
namespace FastColoredTextBoxNS
{
    public partial class ReplaceForm : Form
    {
        FastColoredTextBox tb;
        int     FoundCount  = 0;
        bool    firstSearch = true;
        Place   startPlace;
        ToolTip tooltip1  = new ToolTip();
        ToolTip tooltip2  = new ToolTip();
        ToolTip tooltip3  = new ToolTip();
        string  MBCaption = "HMS Editor: Поиск и замена";
        Timer   timer     = new Timer();

        public ReplaceForm(FastColoredTextBox tb)
        {
            InitializeComponent();
            this.tb = tb;
            timer.Interval = 500;
            timer.Tick += Timer_Tick;
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

        private void Timer_Tick(object sender, EventArgs e) {
            timer.Stop();
            CheckCount();
        }

        private void CheckCount() {
            if (!Visible) return;
            FoundCount = 0;
            try {
                string pattern = tbFind.Text;
                if (pattern != "") {
                    RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
                    if (!cbRegex.Checked) pattern = Regex.Escape(pattern);
                    if (cbWholeWord.Checked) pattern = "\\b" + pattern + "\\b";

                    FoundCount = tb.LightYellowSelect(pattern, opt);
                }
            }
            catch {
                // ignored
            }
            btnReplace   .Enabled = (FoundCount > 0);
            btnReplaceAll.Enabled = (FoundCount > 0);
            lblFound.Text = FoundCount.ToString();
        }

        protected override void OnMouseDown(MouseEventArgs mea) {
            base.OnMouseDown(mea);
            //ctrl-leftclick anywhere on the control to drag the form to a new location 
            if (mea.Button == MouseButtons.Left && ModifierKeys == Keys.Control) {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, (IntPtr)NativeMethods.HT_CAPTION, (IntPtr)0);
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, HMS.BordersColor, ButtonBorderStyle.Solid);
        }

        private void btFindNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Find(tbFind.Text)) {
                    if (FoundCount == 0)
                        MessageBox.Show("К сожалению, ничего найдено не было.\r\n:(", MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    else
                        MessageBox.Show(Resources.ReplaceForm_btFindNext_Click_, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public List<Range> FindAll(string pattern)
        {
            tb.YellowSelection    = true;
            tb.SelectionAfterFind = true;
            var opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
            if (!cbRegex.Checked)
                pattern = Regex.Escape(pattern);
            if (cbWholeWord.Checked)
                pattern = "\\b" + pattern + "\\b";

            Regex regex = new Regex(pattern, opt);
            lblFound.Text = regex.Matches(tb.Text).Count.ToString();
            //
            //var range = tb.Selection.IsEmpty? tb.Range.Clone() : tb.Selection.Clone();
            var range = tb.Range.Clone();
            //
            var list = new List<Range>();
            foreach (var r in range.GetRangesByLines(pattern, opt))
                list.Add(r);

            return list;
        }

        public bool Find(string pattern)
        {
            tb.YellowSelection    = true;
            tb.SelectionAfterFind = true;
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
            range.End   = range.Start >= startPlace ? new Place(tb.GetLineLength(tb.LinesCount - 1), tb.LinesCount - 1) : startPlace;
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
                Place oldPlace = tb.Selection.Start;
                tb.Selection.Start = new Place(0, 0);
                bool found = Find(pattern);
                if (!found) {
                    tb.Selection.Start = oldPlace;
                }
                return found;
            }
            return false;
        }

        private void tbFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                btFindNext_Click(sender, null);
            if (e.KeyChar == '\x1b') {
                Hide();
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) // David
        {
            if (keyData == Keys.Escape)
            {
                SetFocusToEditor();
                Hide();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ReplaceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
            Hide();
            SetFocusToEditor();
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
                MessageBox.Show(getNumText(ranges.Count, new[] { "Произведена", "Произведено", "Произведено" }) + @" " + ranges.Count + @" " + getNumText(ranges.Count, new[] {"замена", "замены", "замен" }), MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("К сажалению, произолша какая-то ошибка.\r\nБыло бы замечательно сообщить об этом автору.\r\n"+ex.Message, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            tb.Selection.EndUpdate();
        }

        protected override void OnActivated(EventArgs e)
        {
            tbFind.Focus();
            ResetSearch();
        }

        void ResetSearch()
        {
            timer.Stop();
            timer.Start();
            firstSearch = true;
        }

        private void cbMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            ResetSearch();
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

        private void ReplaceForm_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\t') {
                SelectNextControl(ActiveControl, true, false, false, true);
                e.Handled = true;
            }
        }

        private void SetFocusToEditor() {
            tb.LightYellowOff();
            tb.Focus();
        }

        private void ReplaceForm_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, (IntPtr)NativeMethods.HT_CAPTION, (IntPtr)0);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) {
            Hide();
            SetFocusToEditor();
        }

        private void ReplaceForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible)
                SetFocusToEditor();
        }
    }
}
