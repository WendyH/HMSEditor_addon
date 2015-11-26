using System.Security.Permissions;
using System.Windows.Forms;

namespace HMSEditorNS {
	public partial class frmUpdateInfoDialog: Form {
		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public frmUpdateInfoDialog() {
			InitializeComponent();
        }

		[EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
		public void SetText(string text) {
			webBrowser1.DocumentText = text;
        }

		private void frmUpdateInfoDialog_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape) this.Close();
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape) this.Close();
		}
	}
}
