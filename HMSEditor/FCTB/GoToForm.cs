using System;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{
    public partial class GoToForm : Form
    {
        public int SelectedLineNumber { get; set; }
        public int TotalLineCount { get; set; }
		public ToolStripItemCollection Items { get { return btnGoto.DropDownItems; } }
		public int GotoPosition = 0;

        public GoToForm()
        {
            InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.tbLineNumber.Text = this.SelectedLineNumber.ToString();

            this.label.Text = String.Format("Номер строки (1 - {0}):", this.TotalLineCount);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.tbLineNumber.Focus();
			toolStrip1.Visible = (Items.Count != 0);
		}

		private void btnOk_Click(object sender, EventArgs e)
        {
            int enteredLine;
            if (int.TryParse(this.tbLineNumber.Text, out enteredLine))
            {
                enteredLine = Math.Min(enteredLine, this.TotalLineCount);
                enteredLine = Math.Max(1, enteredLine);

                this.SelectedLineNumber = enteredLine;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
         }

		private void btnGoto_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
			this.DialogResult = DialogResult.Retry;
			GotoPosition = (int)e.ClickedItem.Tag;
			this.Close();
		}
	}
}
