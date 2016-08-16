using System;
using System.Drawing;
using System.Windows.Forms;
using DifferenceEngine;
using System.IO;
using System.Collections;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;
using System.Web;

namespace HMSEditorNS {
    public partial class DiffControl: UserControl {
        public string File1 = "";
        public string File2 = "";

        public string Text1 = "";
        public string Text2 = "";

        public Language Language { get { return tb1.Language; } set { tb1.Language = value; tb2.Language = value; } }

        int FilterIndex = 1;

        public DiffControl() {
            InitializeComponent();
            PrepareTB(tb1);
            PrepareTB(tb2);
            tb1.VerticalScrollValueChanged += Tb1_VerticalScrollValueChanged;
            tb2.VerticalScrollValueChanged += Tb2_VerticalScrollValueChanged;
            tb1.VerticalScrollVisible = false;
        }

        private void Tb2_VerticalScrollValueChanged(object sender, EventArgs e) {
            tb1.SetVerticalScrollValueNoEvent(tb2.GetVerticalScrollValue());
        }

        private void Tb1_VerticalScrollValueChanged(object sender, EventArgs e) {
            tb2.SetVerticalScrollValueNoEvent(tb1.GetVerticalScrollValue());
        }

        private void PrepareTB(FastColoredTextBox tb) {
            tb.ShowBeginOfFunctions        = false;
            tb.ShowChangedLinesOnScrollbar = false;
            tb.ShowFoldingLines            = false;
            tb.EnableFoldingIndicator      = false;
            tb.ShowFoldingMarkers          = false;
            tb.DrawLineNumberFromInfo      = true;
            tb.ShowInvisibleCharsInSelection = true;
            tb.SelectionWithBorders = true;
            Themes.SetTheme(tb, "Стандартная");
        }

        private string FileDialogFilter() {
            return "All files (*.*)|*.*|" +
                   "Config files (*.cfg)|*.cfg|" +
                   "PascalScript files (*.pas)|*.pas|" +
                   "C++Script files (*.cpp)|*.cpp|" +
                   "JavaScript files (*.js)|*.js|" +
                   "BasicScript files (*.bas, *.vb)|*.bas;*.vb|" +
                   "Yaml files (*.yml)|*.yml|" +
                   "Text files (*.txt)|*.txt";
        }

        public bool OpenFile(ref string text, ref string filename) {
            bool success = false;
            OpenFileDialog fileFialog = new OpenFileDialog();
            if (filename.Length > 0) {
                fileFialog.InitialDirectory = Path.GetDirectoryName(filename);
                fileFialog.FileName         = Path.GetFileName(filename);
            }
            fileFialog.Filter           = FileDialogFilter();
            fileFialog.FilterIndex      = FilterIndex;
            fileFialog.RestoreDirectory = true;
            fileFialog.Title            = @"Выбор файла скрипта";
            if (fileFialog.ShowDialog() == DialogResult.OK) {
                filename = fileFialog.FileName;
                if (File.Exists(filename)) {
                    HMS.LoadAndDetectEncoding(filename, out text);
                    string msg = "";
                    Match m = Regex.Match(text, "^<\\?xml.*?<TranscodingParams>(.*?)</TranscodingParams>.*?<TranscodingParamsSyntaxType>(.*?)</TranscodingParamsSyntaxType>", RegexOptions.Singleline);
                    if (m.Success)
                        msg = "Загрузить скрипт из профиля транскодирования?\n\n(Если ответите \"Нет\", то файл будет загружен как обычный текст.)";
                    else {
                        m = Regex.Match(text, "^<\\?xml.*?<Script>(.*?)</Script>.*?<ScriptSyntaxType>(.*?)</ScriptSyntaxType>", RegexOptions.Singleline);
                        if (m.Success)
                            msg = "Загрузить скрипт из обработки?\n\n(Если ответите \"Нет\", то файл будет загружен как обычный текст.)";
                    }
                    if (m.Success) {
                        if (MessageBox.Show(msg, HMSEditor.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                            text = HttpUtility.HtmlDecode(m.Groups[1].Value);
                            switch (m.Groups[2].Value) {
                                case "C++Script"   : Language = Language.CPPScript   ; break;
                                case "PascalScript": Language = Language.PascalScript; break;
                                case "BasicScript" : Language = Language.BasicScript ; break;
                                case "JScript"     : Language = Language.JScript     ; break;
                                default            : Language = Language.YAML        ; break;
                            }
                        }
                    }
                    success = true;
                }
            }
            FilterIndex = fileFialog.FilterIndex;
            return success;
        }

        private void Compare() {
            DiffList_TextFile sLF = null;
            DiffList_TextFile dLF = null;

            sLF = new DiffList_TextFile();
            dLF = new DiffList_TextFile();
            sLF.LoadText(Text1);
            dLF.LoadText(Text2);

            try {
                double time = 0;
                DiffEngine de = new DiffEngine();
                time = de.ProcessDiff(sLF, dLF, DiffEngineLevel.Medium);

                ArrayList rep = de.DiffReport();

                Color red   = Color.FromArgb(100, Color.Red);
                Color green = Color.FromArgb(80, Color.Green);

                tb1.ClearAllLines();
                tb2.ClearAllLines();
                int i, iLine1 = 0, iLine2 = 0;
                foreach (DiffResultSpan drs in rep) {
                    switch (drs.Status) {
                        case DiffResultSpanStatus.DeleteSource:
                            for (i = 0; i < drs.Length; i++) {
                                tb1.AddLine(((TextLine)sLF.GetByIndex(drs.SourceIndex + i)).Line, red, ref iLine1);
                                tb2.AddUnavaliableLine();
                            }
                            break;
                        case DiffResultSpanStatus.NoChange:
                            for (i = 0; i < drs.Length; i++) {
                                tb1.AddLine(((TextLine)sLF.GetByIndex(drs.SourceIndex + i)).Line, Color.Transparent, ref iLine1);
                                tb2.AddLine(((TextLine)dLF.GetByIndex(drs.DestIndex + i)).Line, Color.Transparent, ref iLine2);
                            }
                            break;
                        case DiffResultSpanStatus.AddDestination:
                            for (i = 0; i < drs.Length; i++) {
                                tb1.AddUnavaliableLine();
                                tb2.AddLine(((TextLine)dLF.GetByIndex(drs.DestIndex + i)).Line, green, ref iLine2);
                            }
                            break;
                        case DiffResultSpanStatus.Replace:
                            for (i = 0; i < drs.Length; i++) {
                                tb1.AddLine(((TextLine)sLF.GetByIndex(drs.SourceIndex + i)).Line, red, ref iLine1);
                                tb2.AddLine(((TextLine)dLF.GetByIndex(drs.DestIndex + i)).Line, green, ref iLine2);
                            }
                            break;
                    }

                }
                tb1.NeedRecalc(true);
                tb2.NeedRecalc(true);
                tb1.RefreshTheme();
                tb2.RefreshTheme();

            } catch (Exception ex) {
                HMS.LogError(ex);
            }

        }

        private void toolStripMenuItemLoad1_Click(object sender, EventArgs e) {
            if (OpenFile(ref Text1, ref File1)) {
                label1.Text = File1;
                Compare();
            }
        }

        private void toolStripMenuItemLoad2_Click(object sender, EventArgs e) {
            if (OpenFile(ref Text2, ref File2)) {
                label2.Text = File2;
                Compare();
            }
        }

        private void toolStripMenuItemInsert1_Click(object sender, EventArgs e) {
            label1.Text = "Исходный текст";
            Text1 = Clipboard.GetText();
            Compare();
        }

        private void toolStripMenuItemInsert2_Click(object sender, EventArgs e) {
            label2.Text = "Изменённый текст";
            Text2 = Clipboard.GetText();
            Compare();
        }

        private void toolStripMenuItemCompare1_Click(object sender, EventArgs e) {
            Compare();
        }

        private void toolStripMenuItemCompare2_Click(object sender, EventArgs e) {
            Compare();
        }

    }
}
