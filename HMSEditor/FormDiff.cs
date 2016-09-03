using System;
using System.Windows.Forms;

namespace HMSEditorNS {
    public partial class FormDiff: Form {
        public string File1 { get { return diffControl1.File1; } set { diffControl1.File1 = value; } }
        public string File2 { get { return diffControl1.File2; } set { diffControl1.File2 = value; } }

        public string Text1 { get { return diffControl1.Text1; } set { diffControl1.Text1 = value; } }
        public string Text2 { get { return diffControl1.Text2; } set { diffControl1.Text2 = value; } }

        public bool HideLineBreakInvisibleChar    { get { return diffControl1.HideLineBreakInvisibleChar   ; } set { diffControl1.HideLineBreakInvisibleChar    = value; } }
        public bool ShowInvisibleCharsInSelection { get { return diffControl1.ShowInvisibleCharsInSelection; } set { diffControl1.ShowInvisibleCharsInSelection = value; } }

        public FormDiff() {
            InitializeComponent();
        }

        public FormDiff(FastColoredTextBoxNS.Language lang) : this() {
            diffControl1.Language = lang;
        }

        public void Compare() {
            diffControl1.Compare();
        }

        private void FormDiff_FormClosing(object sender, FormClosingEventArgs e) {
            Properties.Settings.Default.FormDiffMaximized = WindowState == FormWindowState.Maximized;
            Properties.Settings.Default.FormDiffWindowPosition = DesktopBounds;
            Properties.Settings.Default.FormDiffFile1 = File1;
            Properties.Settings.Default.FormDiffFile2 = File2;
            Properties.Settings.Default.FormDiffFilterIndex = diffControl1.FilterIndex;
            Properties.Settings.Default.Save();
        }

        private void FormDiff_Load(object sender, EventArgs e) {
            var rect = Properties.Settings.Default.FormDiffWindowPosition;
            if (Properties.Settings.Default.FormDiffMaximized)
                WindowState = FormWindowState.Maximized;
            else foreach(var screen in Screen.AllScreens) {
                    if (rect.Height > 0 && screen.WorkingArea.IntersectsWith(rect)) {
                        StartPosition = FormStartPosition.Manual;
                        DesktopBounds = rect;
                        WindowState   = FormWindowState.Normal;
                        break;
                    }
                }
            File1 = Properties.Settings.Default.FormDiffFile1;
            File2 = Properties.Settings.Default.FormDiffFile2;
            diffControl1.FilterIndex = Properties.Settings.Default.FormDiffFilterIndex;
        }

        private void FormDiff_KeyDown(object sender, KeyEventArgs e) {
            //if (e.KeyCode == Keys.Escape)
            //    Close();
        }
    }
}
