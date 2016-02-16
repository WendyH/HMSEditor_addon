using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using HMSEditorNS;
using System.Drawing;

namespace FastColoredTextBoxNS
{
    public partial class FindForm : Form
    {
        bool firstSearch = true;
        Place startPlace;
        FastColoredTextBox tb;
        ToolTip tooltip1 = new ToolTip();
        ToolTip tooltip2 = new ToolTip();
        ToolTip tooltip3 = new ToolTip();
        string MBCaption = "HMS Editor - Поиск";
        Timer timer = new Timer();

        public FindForm(FastColoredTextBox tb)
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
            tooltip3.SetToolTip(cbRegex, "Поиск по указанному регулярному вырежению.\nНапример: MyVar\\s*?=");
        }

        private void Timer_Tick(object sender, EventArgs e) {
            timer.Stop();
            CheckCount();
        }

        private void CheckCount() {
            int n = 0;
            try {
                string pattern = tbFind.Text;
                if (pattern!="") {
                    RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
                    if (!cbRegex.Checked) pattern = Regex.Escape(pattern);
                    if (cbWholeWord.Checked) pattern = "\\b" + pattern + "\\b";

                    Regex regex = new Regex(pattern, opt);
                    n = regex.Matches(tb.Text).Count;
                }
            } catch {

            }
            lblFound.Text = n.ToString();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, HMS.BordersColor, ButtonBorderStyle.Solid);
        }

        public virtual void FindNext(string pattern)
        {
            tb.YellowSelection    = true;
            tb.SelectionAfterFind = true;

            try {
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
                    tb.Selection = r;
                    tb.DoSelectionVisible();
                    tb.Invalidate();
                    return;
                }
                //
                if (range.Start >= startPlace && startPlace > Place.Empty)
                {
                    tb.Selection.Start = new Place(0, 0);
                    FindNext(pattern);
                    return;
                }
                MessageBox.Show("Совпадений не найдено", MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tbFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                FindNext(tbFind.Text);
                e.Handled = true;
                return;
            }
            if (e.KeyChar == '\x1b')
            {
                Hide();
                e.Handled = true;
                SetFocusToEditor();
                return;
            }
        }

        private void FindForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            SetFocusToEditor();
        }
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                SetFocusToEditor();
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
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

        private void SetFocusToEditor() {
            tb.YellowSelection = false;
            tb.Focus();
        }

        private void cbMatchCase_KeyPress(object sender, KeyPressEventArgs e) {
        }

        private void cbWholeWord_KeyPress(object sender, KeyPressEventArgs e) {
        }

        private void cbRegex_KeyPress(object sender, KeyPressEventArgs e) {
        }

        private void FindForm_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\t') {
                SelectNextControl(ActiveControl, true, false, false, true);
                e.Handled = true;
                return;
            }
        }

        private void FindForm_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, (IntPtr)NativeMethods.HT_CAPTION, (IntPtr)0);
            }
        }
    }
}
