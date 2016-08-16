using System;
using System.Windows.Forms;

namespace HMSEditorNS {
    public partial class FormDiff: Form {
        public string File1 { get { return diffControl1.File1; } set { diffControl1.File1 = value; } }
        public string File2 { get { return diffControl1.File2; } set { diffControl1.File2 = value; } }

        public string Text1 { get { return diffControl1.Text1; } set { diffControl1.Text1 = value; } }
        public string Text2 { get { return diffControl1.Text2; } set { diffControl1.Text2 = value; } }

        public FormDiff() {
            InitializeComponent();
        }

        public FormDiff(FastColoredTextBoxNS.Language lang) : this() {
            diffControl1.Language = lang;
        }

        protected override void OnShown(EventArgs e) {
            //Compare();
            base.OnShown(e);
        }

    }
}
