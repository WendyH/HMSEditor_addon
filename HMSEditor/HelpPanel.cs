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

        private Timer timer = new Timer();

		public HelpPanel() {
			InitializeComponent();
            timer.Tick += Timer_Tick;
            timer.Interval = 200;
        }

        private void Timer_Tick(object sender, EventArgs e) {
            timer.Stop();
            FillListBox(comboBox1.Text);
        }

        public void Init(ImageList imagelist) {
            flatListBox1.ImageList = imagelist;
            FillListBox();
        }

        private void FillListBox(string word = "") {
            AutocompleteItems visibleItems = new AutocompleteItems();
            AutocompleteItems notExacItems = new AutocompleteItems();

            flatListBox1.Items.Clear();
            foreach (var item in HMS.ItemsClass) {
                string itemtext = item.ToString();
                if (itemtext.StartsWith(word, StringComparison.InvariantCultureIgnoreCase))
                    visibleItems.Add(item);
                if (itemtext.IndexOf(word, StringComparison.InvariantCultureIgnoreCase)>0)
                    notExacItems.Add(item);
            }
            foreach (var item in HMS.ItemsFunction) {
                string itemtext = item.ToString();
                if (itemtext.StartsWith(word, StringComparison.InvariantCultureIgnoreCase))
                    visibleItems.Add(item);
                if (itemtext.IndexOf(word, StringComparison.InvariantCultureIgnoreCase) > 0)
                    notExacItems.Add(item);
            }
            foreach (var item in HMS.ItemsVariable) {
                string itemtext = item.ToString();
                if (itemtext.StartsWith(word, StringComparison.InvariantCultureIgnoreCase))
                    visibleItems.Add(item);
                if (itemtext.IndexOf(word, StringComparison.InvariantCultureIgnoreCase) > 0)
                    notExacItems.Add(item);
            }
            flatListBox1.SetAutocompleteItems(visibleItems);
            flatListBox1.AddAutocompleteItems(notExacItems);
            flatListBox1.FocussedItemIndex = 0;
            flatListBox1.Invalidate();
        }

        private void ShowHelp(HMSItem item = null) {
            if (item == null) item = flatListBox1.SelectedItem;
            if (item!=null) {
                float h;
                string help = HmsToolTip.GetText(item, out h);
                richTextBox1.Text = help;
            } else {
                richTextBox1.Text = "";
            }
            
        }

        private void comboBox1_TextChanged(object sender, EventArgs e) {
            timer.Stop();
            timer.Start();
        }

        private void comboBox1_Validated(object sender, EventArgs e) {
            richTextBox1.Focus();
        }

        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\n')
                richTextBox1.Focus();
        }

        private void flatListBox1_MouseDoubleClick(object sender, MouseEventArgs e) {
            ShowHelp();
        }
    }

}
