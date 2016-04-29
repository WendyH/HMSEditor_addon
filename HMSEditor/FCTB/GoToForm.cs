using System;
using System.Windows.Forms;
using HMSEditorNS;

// ReSharper disable once CheckNamespace
namespace FastColoredTextBoxNS
{
    public partial class GoToForm : Form
    {
        public int SelectedLineNumber { get; set; }
        public int TotalLineCount { get; set; }
        public ToolStripItemCollection Items => btnGoto.DropDownItems;
        public int GotoPosition;

        public GoToForm()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, HMS.BordersColor, ButtonBorderStyle.Solid);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tbLineNumber.Text = SelectedLineNumber.ToString();

            label.Text = $"Номер строки (1 - {TotalLineCount}):";
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            tbLineNumber.Focus();
            toolStrip1.Visible = (Items.Count != 0);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            int enteredLine;
            if (int.TryParse(tbLineNumber.Text, out enteredLine))
            {
                enteredLine = Math.Min(enteredLine, TotalLineCount);
                enteredLine = Math.Max(1, enteredLine);

                SelectedLineNumber = enteredLine;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
         }

        private void btnGoto_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            DialogResult = DialogResult.Retry;
            GotoPosition = (int)e.ClickedItem.Tag;
            Close();
        }

        private void GoToForm_FormClosed(object sender, FormClosedEventArgs e) {
        }

        private void GoToForm_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, (IntPtr)NativeMethods.HT_CAPTION, (IntPtr)0);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) {
            Close();
        }

    }
}
