using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
	public partial class HelpPanel: UserControl {
		public HelpPanel() {
			InitializeComponent();
		}

        public void Init() {
            listBox1.Items.Clear();
            foreach (var item in HMS.ItemsClass) {
                listBox1.Items.Add(item);
            }
            foreach (var item in HMS.ItemsFunction) {
                listBox1.Items.Add(item);
            }
            foreach (var item in HMS.ItemsVariable) {
                listBox1.Items.Add(item);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            var item = listBox1.SelectedItem as HMSItem;
            if (item!=null) {
                float h;
                string help = HmsToolTip.GetText(item, out h);
                richTextBox1.Text = help;
            } else {
                richTextBox1.Text = "";
            }
        }

        private void buttonClose_Click(object sender, EventArgs e) {
        }
    }

}
