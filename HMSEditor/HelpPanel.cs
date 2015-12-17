using System;
using System.Drawing;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;

namespace HMSEditorNS {
	public partial class HelpPanel: UserControl {
        public string Filter = "";

        private Timer timer = new Timer();

        public HelpPanel() {
			InitializeComponent();
            timer.Tick += Timer_Tick;
            timer.Interval = 200;
            flatListBox1.FocussedItemIndexChanged += FlatListBox1_FocussedItemIndexChanged;
            if (HMS.PFC.Families.Length > 0) { // By WendyH
                HelpTextBox.Font = new Font(HMS.PFC.Families[0], 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            } else {
                HelpTextBox.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            }
            flatListBox1.VerticalScroll.SmallChange = flatListBox1.ItemHeight;
        }

        private void FlatListBox1_FocussedItemIndexChanged(object sender, EventArgs e) {
            ShowHelp();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            timer.Stop();
            FillListBox(comboBox1.Text);
        }

        public void Init(ImageList imagelist, string filter) {
            Filter = filter;
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
                if (itemtext.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)) {
                    visibleItems.Add(item);
                }
                if (itemtext.IndexOf(word, StringComparison.InvariantCultureIgnoreCase) > 0)
                    notExacItems.Add(item);
            }
            foreach (var item in HMS.ItemsVariable) {
                if ((Filter.Length > 0) && (item.Filter.Length > 0) && (Filter.IndexOf(item.Filter) < 0)) continue;
                if (visibleItems.ContainsName(item.MenuText)|| notExacItems.ContainsName(item.MenuText)) continue;

                string itemtext = item.ToString();
                if (itemtext.StartsWith(word, StringComparison.InvariantCultureIgnoreCase))
                    visibleItems.Add(item);
                if (itemtext.IndexOf(word, StringComparison.InvariantCultureIgnoreCase) > 0)
                    notExacItems.Add(item);
            }
            foreach (var item in HMS.ItemsConstant) {
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
            ShowHelp();
        }

        private void ShowHelp(HMSItem item = null) {
            HelpTextBox.SuspendLayout();
            HelpTextBox.Text = "";
            if (item == null) item = flatListBox1.SelectedItem;
            if (!string.IsNullOrEmpty(item.ToolTipTitle)) {
                string help = HmsToolTip.GetText(item);
                if (item.Params.Count > 0) {
                    help += "\n-----------------\nПараметры:\n";
                    foreach (var param in item.Params) {
                        help += Regex.Replace(param, "^(\\w+)", "<p>$1</p>") + "\r\n";
                    }
                }
                HmsToolTip.WriteWords(HelpTextBox, help.TrimEnd());
            }
            HelpTextBox.ResumeLayout();
        }

        private void comboBox1_TextChanged(object sender, EventArgs e) {
            timer.Stop();
            timer.Start();
        }

        private void flatListBox1_MouseDoubleClick(object sender, MouseEventArgs e) {
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.Modifiers == Keys.None)
                switch (e.KeyData) {
                    case Keys.Down:
                        flatListBox1.SelectNext(+1);
                        return;
                    case Keys.PageDown:
                        flatListBox1.SelectNext(+10);
                        return;
                    case Keys.Up:
                        flatListBox1.SelectNext(-1);
                        return;
                    case Keys.PageUp:
                        flatListBox1.SelectNext(-10);
                        return;
                    case Keys.Home:
                        flatListBox1.FocussedItemIndex = 0;
                        flatListBox1.DoSelectedVisible();
                        flatListBox1.Invalidate();
                        e.Handled = true;
                        return;
                    case Keys.End:
                        flatListBox1.FocussedItemIndex = (flatListBox1.Items.Count > 0) ? flatListBox1.Items.Count - 1 : 0;
                        flatListBox1.DoSelectedVisible();
                        flatListBox1.Invalidate();
                        e.Handled = true;
                        return;
                    case Keys.Enter:
                        flatListBox1.OnSelecting();
                        return;
                }

        }
    }

}
