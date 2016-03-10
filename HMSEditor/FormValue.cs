using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using FastColoredTextBoxNS;

namespace HMSEditorNS {
    public partial class FormValue: Form {
        private string _src = "";

        public string Value {
            get { return fastColoredTB.Text; }
            set {
                fastColoredTB.Language = fastColoredTB.SyntaxHighlighter.DetectLang(value);
                fastColoredTB.Text     = value;
                cbLanguage.Text = fastColoredTB.SyntaxHighlighter.Lang2Str(fastColoredTB.Language);
                //cbLanguage_SelectedIndexChanged(null, EventArgs.Empty);
            }
        }
        public string Expression = " ";
        public string RealExpression { get { return tbExpression.Text; } set { tbExpression.Text = value; } }

        public FormValue() {
            InitializeComponent();
            chkWordWrap_CheckedChanged(null, EventArgs.Empty);
            chkOnTop_CheckedChanged(null, EventArgs.Empty);

            cbResultIndex.Items.Add("Группировка 0");
            cbResultIndex.Items.Add("Группировка 1");
            cbResultIndex.Items.Add("Группировка 2");
            cbResultIndex.Items.Add("Группировка 3");
            cbResultIndex.Text = "Группировка 1";

            cbPattern.Items.Add(@"");
            cbPattern.Items.Add(@"<video(.*?)</video>");
            cbPattern.Items.Add(@"<a[^>]+href=['""](.*?)['""]");
            cbPattern.Items.Add(@"(<a.*?</a>)");
            cbPattern.Items.Add(@"(<li.*?</li>)");
            cbPattern.Items.Add(@"(<h\d.*?</h\d>)");
            cbPattern.Items.Add(@"<img[^>]+src=""(.*?)""");

            chkUseRegex_CheckedChanged(null, EventArgs.Empty);
        }

        public void Show(Control ctl, string expr, string text, string realExpression) {
            if (IsDisposed) {
                MessageBox.Show(this, "Окно отображения значения не может быть показано.\nЕго кто-то или что-то уничтожило.", HMSEditor.Title);
                return;
            }
            fastColoredTB.Font = ctl.Font; 
            Expression = expr;
            Value      = text;
            RealExpression = realExpression;
            _src       = text;

            chkFormatting_CheckedChanged(null, EventArgs.Empty);

            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;

            if (!Visible) {
                Show(ctl);
            }
        }

        private void chkWordWrap_CheckedChanged(object sender, EventArgs e) {
            fastColoredTB.WordWrap = chkWordWrap.Checked;
            if (!chkWordWrap.Checked) fastColoredTB.GoHome();
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

        private void chkUseRegex_CheckedChanged(object sender, EventArgs e) {
            if (chkUseRegex.Checked) {
                panelTop.Height = groupRegex.Top + groupRegex.Height + 4;
            } else {
                panelTop.Height = chkUseRegex.Top + chkUseRegex.Height + 4;
            }
        }

        Regex tt;
        private void cbPattern_TextChanged(object sender, EventArgs e) {
            label1.ForeColor = CreateRegex() ? Color.Black : Color.Red;
        }

        public MatchCollection MatchesWithTimeout(Regex regex, string input, int duration) {
            var reset = new AutoResetEvent(false);
            Exception ex = null;
            MatchCollection r = null;
            
            var t = new Thread(() => {
                try {
                    r = regex.Matches(input);
                } catch (Exception e) {
                    ex = e;
                }
                reset.Set();
            });

            t.Start();

            if (!reset.WaitOne(duration)) {
                t.Abort();
                //throw new TimeoutException();
            }
            return r; // if error - return null
        }

        private void SearchRegexPattern() {
            if (tt == null || string.IsNullOrEmpty(Value)) return;

            string text;
            List<Place> charIndexToPlace;
            var range = fastColoredTB.Range;
            range.GetText(out text, out charIndexToPlace);

            MatchCollection mc = MatchesWithTimeout(tt, text, 5000);
            if (mc == null) {
                tbResult.Text = "Операция прервана по таймауту.";
                return;
            }

            tbRegexCount.Text = mc.Count.ToString();
            int pos = fastColoredTB.SelectionStart;
            bool waspos = false;
            range.ClearStyle(fastColoredTB.GreenSelectionStyle, fastColoredTB.BlueSelectionStyle);
            fastColoredTB.SyntaxHighlighter.HighlightSyntax(fastColoredTB.Language, range);
            string result = "";
            foreach (Match m in mc) {
                Group group = m.Groups[0];
                Range r = new Range(range.tb);
                r.Start = charIndexToPlace[group.Index];
                r.End = charIndexToPlace[group.Index + group.Length]; 
                r.SetStyleOwerwrite(fastColoredTB.BlueSelectionStyle);
                bool skipfisrt = false;
                foreach (Group g in m.Groups) {
                    if (!skipfisrt) { skipfisrt = true; continue; }
                    Range rg = new Range(range.tb);
                    rg.Start = charIndexToPlace[g.Index];
                    rg.End = charIndexToPlace[g.Index + g.Length];
                    rg.SetStyleOwerwrite(fastColoredTB.GreenSelectionStyle);
                }
                if (!waspos) {
                    int posr = fastColoredTB.PlaceToPosition(r.Start);
                    if (pos < posr) {
                        waspos = true;
                        fastColoredTB.SelectionStart = posr;
                        fastColoredTB.DoCaretVisible();
                        result = m.Groups[cbResultIndex.SelectedIndex].Value;
                        break;
                    }
                }
            }
            if (chkHTML2Text.Checked) result = HtmlRemoval.Html2Text(result);
            tbResult.Text = result;
            fastColoredTB.YellowSelection    = true;
            fastColoredTB.SelectionAfterFind = true;
            int n = fastColoredTB.LightYellowSelect(tt);
        }

        private bool CreateRegex() {
            RegexOptions options = RegexOptions.CultureInvariant;
            if (chkPCRE_CASELESS  .Checked) options |= RegexOptions.IgnoreCase;
            if (chkPCRE_MULTILINE .Checked) options |= RegexOptions.Multiline;
            if (chkPCRE_SINGLELINE.Checked) options |= RegexOptions.Singleline;
            try {
                tt = new Regex(cbPattern.Text, options);
                label1.ForeColor = Color.Black; 
            } catch {
                tt = null;
                return false;
            }
            return true;
        }

        private void cbPattern_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                SearchRegexPattern();
            }
        }

        private void chkPCRE_SINGLELINE_CheckedChanged(object sender, EventArgs e) {
            CreateRegex();
        }

        private void chkPCRE_MULTILINE_CheckedChanged(object sender, EventArgs e) {
            CreateRegex();
        }

        private void chkPCRE_CASELESS_CheckedChanged(object sender, EventArgs e) {
            CreateRegex();
        }

        private void cbPattern_SelectedValueChanged(object sender, EventArgs e) {
        }

        private void chkHTML2Text_CheckedChanged(object sender, EventArgs e) {
            if (chkHTML2Text.Checked)
                tbResult.Text = HtmlRemoval.Html2Text(tbResult.Text);
        }

        private void cbPattern_SelectedIndexChanged(object sender, EventArgs e) {
            if (CreateRegex()) {
                SearchRegexPattern();
                fastColoredTB.Focus();
            }
        }

        private void chkOnTop_CheckedChanged(object sender, EventArgs e) {
            this.TopMost = chkOnTop.Checked;
        }

        private void cbLanguage_SelectedIndexChanged(object sender, EventArgs e) {
            fastColoredTB.Language = fastColoredTB.SyntaxHighlighter.Str2Lang(cbLanguage.Text);
            fastColoredTB.SyntaxHighlighter.HighlightSyntax(fastColoredTB.Language, fastColoredTB.Range);
        }

        private void chkFormatting_CheckedChanged(object sender, EventArgs e) {
            if (cbFormatting.Checked) {
                try {
                    if (Regex.IsMatch(Value.Trim(), @"^[\{\[].*[\}\]]$", RegexOptions.Singleline))
                        Value = Utils.JsonHelper.FormatJson(_src);
                    else
                        Value = PrettyHTML(_src);
                } catch {
                }
            } else {
                Value = _src;
            }
        }

        public static string PrettyHTML(string html) {
            string result = html;
            try {
                TidyNet.Tidy document = new TidyNet.Tidy();
                TidyNet.TidyMessageCollection messageCollection = new TidyNet.TidyMessageCollection();

                document.Options.DocType         = TidyNet.DocType.Auto;
                document.Options.Xhtml           = true;
                document.Options.CharEncoding    = TidyNet.CharEncoding.UTF8;
                document.Options.LogicalEmphasis = false;

                document.Options.MakeClean       = true;
                document.Options.QuoteNbsp       = false;
                document.Options.SmartIndent     = false;
                document.Options.IndentContent   = true;
                document.Options.TidyMark        = false;

                document.Options.DropFontTags    = false;
                document.Options.QuoteAmpersand  = false;
                document.Options.DropEmptyParas  = false;
                document.Options.BreakBeforeBR   = true;
                document.Options.FixComments     = true;
                document.Options.IndentAttributes = false;
                document.Options.LiteralAttribs   = true;
                document.Options.PreserveEntities = false;

                MemoryStream input  = new MemoryStream();
                MemoryStream output = new MemoryStream();
                byte[] array = Encoding.UTF8.GetBytes(html);
                input.Write(array, 0, array.Length);
                input.Position = 0;

                document.Parse(input, output, messageCollection);

                string tidyXhtml = Encoding.UTF8.GetString(output.ToArray());
                if (tidyXhtml!="")
                    result = tidyXhtml;

            } catch { }
            return result;
        }

    }
}
