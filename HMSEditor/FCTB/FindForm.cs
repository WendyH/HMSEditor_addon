﻿using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using HMSEditorNS;

// ReSharper disable once CheckNamespace
namespace FastColoredTextBoxNS
{
    public partial class FindForm : Form
    {
        int FoundCount = 0;
        bool firstSearch = true;
        Place startPlace;
        FastColoredTextBox tb;
        ToolTip tooltip1 = new ToolTip();
        ToolTip tooltip2 = new ToolTip();
        ToolTip tooltip3 = new ToolTip();
        string MBCaption = "HMS Editor: Поиск";
        Timer timer = new Timer();

        public FindForm(FastColoredTextBox tb) {
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
            if (!Visible) return;
            FoundCount = 0;
            try {
                string pattern = cbFind.Text;
                if (pattern!="") {
                    RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
                    if (!cbRegex.Checked) pattern = Regex.Escape(pattern);
                    if (cbWholeWord.Checked) pattern = "\\b" + pattern + "\\b";

                    FoundCount = tb.LightYellowSelect(pattern, opt);
                }
            }
            catch {
                // ignored
            }
            lblFound.Text = FoundCount.ToString();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, HMS.BordersColor, ButtonBorderStyle.Solid);
        }

        public virtual void FindNext(string pattern, bool toBack = false) {
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
                if (firstSearch) {
                    startPlace = range.Start;
                    firstSearch = false;
                }
                //
                range.Start = range.End;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (range.Start >= startPlace)
                    range.End = new Place(tb.GetLineLength(tb.LinesCount - 1), tb.LinesCount - 1);
                else
                    range.End = startPlace;
                //
                foreach (var r in range.GetRangesByLines(pattern, opt)) {
                    tb.Selection = r;
                    tb.DoSelectionVisible();
                    tb.Invalidate();
                    return;
                }
                //
                if (range.Start >= startPlace && startPlace > Place.Empty) {
                    tb.Selection.Start = new Place(0, 0);
                    FindNext(pattern);
                    return;
                }
                firstSearch = true;
                if (FoundCount == 0)
                    MessageBox.Show("Поиск окончен.\r\nНо так и не удалось найти введённый текст :(", MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);

            } catch (Exception ex) {
                MessageBox.Show("К сажалению, произолша какая-то ошибка.\r\nБыло бы замечательно сообщить об этом автору.\r\n" + ex.Message, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public virtual void FindPrev(string pattern) {

            try {
                RegexOptions opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
                if (!cbRegex.Checked)
                    pattern = Regex.Escape(pattern);
                if (cbWholeWord.Checked)
                    pattern = "\\b" + pattern + "\\b";
                Range range = tb.Range.Clone();
                range.End = tb.Selection.Start;
                bool found = false;
                foreach (var r in range.GetRangesByLines(pattern, opt)) {
                    tb.Selection = r;
                    found = true;
                }
                if (found) {
                    tb.YellowSelection    = true;
                    tb.SelectionAfterFind = true;
                    tb.DoSelectionVisible();
                    tb.Invalidate();
                }

            } catch (Exception ex) {
                MessageBox.Show(ex.Message, MBCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbFind_KeyPress(object sender, KeyPressEventArgs e)
        {
            ResetSearch();
            if (e.KeyChar == '\r')
            {
                FindNext(cbFind.Text);
                e.Handled = true;

                // Store search history
                if (!string.IsNullOrWhiteSpace(cbFind.Text) && !cbFind.Items.Contains(cbFind.Text))
                    cbFind.Items.Insert(0, cbFind.Text);
                while (cbFind.Items.Count > 12) cbFind.Items.RemoveAt(cbFind.Items.Count-1);

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
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnActivated(EventArgs e)
        {
            cbFind.Focus();
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
            tb.LightYellowOff();
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
            }
        }

        private void FindForm_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, (IntPtr)NativeMethods.HT_CAPTION, (IntPtr)0);
            }
        }

        private void btnForw_Click(object sender, EventArgs e) {
            FindNext(cbFind.Text);
        }

        private void btnBack_Click(object sender, EventArgs e) {
            FindPrev(cbFind.Text);
        }

        private void btnClose_Click(object sender, EventArgs e) {
            Hide();
            SetFocusToEditor();
        }

        private void FindForm_VisibleChanged(object sender, EventArgs e) {
            if (!Visible) {
                tb.SearchHistory.Clear();
                foreach (var item in cbFind.Items) tb.SearchHistory.Add(item.ToString());
                SetFocusToEditor();
            }
        }

        private void BtnClose_Leave(object sender, EventArgs e) {
            cbFind.Select();
        }

        private void FindForm_Activated(object sender, EventArgs e) {
            cbFind.Select();
        }
    }
}
