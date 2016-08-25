using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HMSEditorNS {
    public partial class FormSelectScript: Form {

        public int ID = 0;
        public string ScriptName => comboBox1.Text;

        public FormSelectScript() {
            InitializeComponent();
        }

        public void SetFile(string file) {
            label1.Text = file;
        }

        public void AddValue(int id, string name) {
            HMSItem item = new HMSItem(name, id);
            comboBox1.Items.Add(item);
        }

        private void btnOK_Click(object sender, System.EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, System.EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnShown(EventArgs e) {
            base.OnShown(e);
            if (comboBox1.Items.Count > 0)
                comboBox1.Text = comboBox1.Items[0].ToString();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            HMSItem item = comboBox1.SelectedItem as HMSItem;
            if (item != null)
                ID = item.ImageIndex;
        }
    }
}
