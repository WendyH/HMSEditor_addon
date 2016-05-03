using System;
using System.Drawing;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using System.ComponentModel;

namespace HMSEditorNS {
	public sealed partial class HelpPanel: UserControl {
        public string Filter = "";

        public int SplitterDistance { get { return splitContainer1.SplitterDistance; } set { splitContainer1.SplitterDistance = value; } }

	    private HMSItem[] visibleItems;
        //protected TreeNode[] visibleNodes;
        private Timer    timer  = new Timer();
        BackgroundWorker worker = new BackgroundWorker();

        public event EventHandler PanelClose;

        public HelpPanel() {
			InitializeComponent();
            timer.Tick += Timer_Tick;
            timer.Interval = 200;
            flatListBox1.FocussedItemIndexChanged += FlatListBox1_FocussedItemIndexChanged;
            flatListBox1.BackColor    = panel1.BackColor;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (HMS.PFC.Families.Length > 0) { // By WendyH
                HelpTextBox.Font = new Font(HMS.PFC.Families[0], 9.25f, FontStyle.Regular, GraphicsUnit.Point);
            } else {
                HelpTextBox.Font = new Font("Consolas", 9.75f, FontStyle.Regular, GraphicsUnit.Point);
            }
            flatListBox1.VerticalScroll.SmallChange = flatListBox1.ItemHeight;
            flatListBox1.Dock = DockStyle.Fill;
            DoubleBuffered = true;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            flatListBox1.BeginUpdate();
            flatListBox1.Items.Clear();
            flatListBox1.Items.AddRange(visibleItems);
            flatListBox1.SelectFirst();
            flatListBox1.EndUpdate();
            ShowHelp();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e) {
            FillListBox(e.Argument as string);
        }

        private void FlatListBox1_FocussedItemIndexChanged(object sender, EventArgs e) {
            ShowHelp();
        }

        private void Timer_Tick(object sender, EventArgs e) {
            timer.Stop();
            FillBackGround(comboBox1.Text);
        }

        public void Init(ImageList imagelist, string filter) {
            Filter = filter;
            flatListBox1.ImageList = imagelist;
            FillBackGround();
        }

        private void FillBackGround(string word = "") {
            if (!worker.IsBusy)
                worker.RunWorkerAsync(word);
        }

	    private void CheckItem(AutocompleteItems exactlyItems, AutocompleteItems notExacItems, HMSItem item, string word) {
            if ((Filter.Length > 0) && (item.Filter.Length > 0) && (Filter.IndexOf(item.Filter, StringComparison.Ordinal) < 0)) return;
            if (exactlyItems.ContainsName(item.MenuText) || notExacItems.ContainsName(item.MenuText)) return;

            string itemtext = item.ToString();
            if (itemtext.StartsWith(word, StringComparison.InvariantCultureIgnoreCase))
                exactlyItems.Add(item);
            else {
                string text = HmsToolTip.GetTextWithHelp(item);
                if (text.IndexOf(word, StringComparison.InvariantCultureIgnoreCase) > 0)
                    notExacItems.Add(item);
            }
        }

        private string prevWord = "";
        private void FillListBox(string word = "") {
            if (word == null) word = "";
            AutocompleteItems exactlyItems = new AutocompleteItems();
            AutocompleteItems notExacItems = new AutocompleteItems();
            if (word == "") {
                exactlyItems.AddRange(HMS.ItemsFunction);
                exactlyItems.AddRange(HMS.ItemsVariable);
                exactlyItems.AddRange(HMS.ItemsClass);
                exactlyItems.AddRange(HMS.ItemsConstant);
            } else if (word.StartsWith(prevWord)) {
                foreach (var item in visibleItems) CheckItem(exactlyItems, notExacItems, item, word);
            } else {
                foreach (var item in HMS.ItemsFunction) CheckItem(exactlyItems, notExacItems, item, word);
                foreach (var item in HMS.ItemsVariable) CheckItem(exactlyItems, notExacItems, item, word);
                foreach (var item in HMS.ItemsClass   ) CheckItem(exactlyItems, notExacItems, item, word);
                foreach (var item in HMS.ItemsConstant) CheckItem(exactlyItems, notExacItems, item, word);
            }
            exactlyItems.AddRange(notExacItems);
            visibleItems = exactlyItems.ToArray();
            prevWord = word;
        }

        private void ShowHelp(HMSItem item = null) {
            if (flatListBox1.Items.Count == 0) return; 
            HelpTextBox.BeginUpdate();
            if (item == null) item = flatListBox1.SelectedItem;
            if (item.Rtf.Length == 0) {
                string help = HmsToolTip.GetTextWithHelp(item);
                item.Rtf    = HmsToolTip.GetRtf(help);
            }
            HelpTextBox.Rtf = item.Rtf;
            HelpTextBox.EndUpdate();
        }

        private void comboBox1_TextChanged(object sender, EventArgs e) {
            timer.Stop(); 
            timer.Start();
        }


        private void comboBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.Modifiers == Keys.None)
                switch (e.KeyData) {
                    case Keys.Enter:
                        flatListBox1.DoDoubleClick();
                        return;
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
                        flatListBox1.SelectFirst();
                        e.Handled = true;
                        return;
                    case Keys.End:
                        flatListBox1.SelectLast();
                        e.Handled = true;
                        return;
                }

        }

        private void btnClose_Click(object sender, EventArgs e) {
            PanelClose?.Invoke(this, new EventArgs());
        }
	}

}
