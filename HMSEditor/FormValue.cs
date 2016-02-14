using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HMSEditorNS {
    public partial class FormValue: Form {

        public string Value { get { return fastColoredTextBox1.Text; } set { fastColoredTextBox1.Text = value; } }

        public FormValue() {
            InitializeComponent();
            chkWordWrap_CheckedChanged(null, EventArgs.Empty);
        }

        public void Show(Control ctl, string capt, string text, Point point) {
            if (IsDisposed) {
                MessageBox.Show(this, "Окно отображения значения не может быть показано.\nЕго кто-то или что-то уничтожило.", HMSEditor.Title);
                return;
            }
            Left = point.X;
            Top = point.Y;
            textBox1.Text = capt;
            Value = text;
            if (!Visible) {
                Show(ctl);
            }
        }

        public void Show(Control ctl, string capt, string text) {
            if (IsDisposed) {
                MessageBox.Show(this, "Окно отображения значения не может быть показано.\nЕго кто-то или что-то уничтожило.", HMSEditor.Title);
                return;
            }
            textBox1.Text = capt;
            Value = text;
            fastColoredTextBox1.Language = fastColoredTextBox1.SyntaxHighlighter.DetectLang(text);
            if (!Visible) Show(ctl);
        }

        private void chkWordWrap_CheckedChanged(object sender, EventArgs e) {
            fastColoredTextBox1.WordWrap = chkWordWrap.Checked;
        }

        private void FormValue_KeyDown(object sender, KeyEventArgs e) {
            if ((e.KeyValue >= (int)Keys.F1) && (e.KeyValue <= (int)Keys.F12)) {
                if (HMSEditor.ActiveEditor!=null) {
                    NativeMethods.SendNotifyKey(HMSEditor.ActiveEditor.Editor.Handle, e.KeyValue);
                }
                e.Handled = true;
                return;
            }
        }

        private void btnClose_Click(object sender, EventArgs e) {
            Close();
        }

        protected override void OnClosing(CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

    }
}
