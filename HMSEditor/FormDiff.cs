using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using DifferenceEngine;
using System.Drawing;

namespace HMSEditorNS {
    public partial class FormDiff: Form {
        public string File1 = "";
        public string File2 = "";

        public string Text1 = "";
        public string Text2 = "";

        public FastColoredTextBox TB1 => tb1;
        public FastColoredTextBox TB2 => tb2;

        int FilterIndex = 1;

        public FormDiff() {
            InitializeComponent();
            tb1.VerticalScrollValueChange += Tb1_Scroll;
            tb2.VerticalScrollValueChange += Tb2_Scroll;
            tb1.DrawLineNumberFromInfo = true;
            tb2.DrawLineNumberFromInfo = true;
            tb1.AllowSeveralTextStyleDrawing = true;
            tb2.AllowSeveralTextStyleDrawing = true;
            tb1.TextChanged += Tb1_TextChanged1;
            tb2.TextChanged += Tb2_TextChanged2;
            tb1.TextChangedDelayed += Tb1_TextChangedDelayed;
            tb2.TextChangedDelayed += Tb2_TextChangedDelayed;
            PrepareTB(tb1);
            PrepareTB(tb2);
        }

        private void PrepareTB(FastColoredTextBox tb) {
            tb.ShowBeginOfFunctions = false;
            tb.ShowChangedLinesOnScrollbar = false;
            tb.ShowFoldingLines = false;
            tb.EnableFoldingIndicator = false;
            tb.ShowFoldingMarkers = false;
        }

        private void Tb1_TextChanged1(object sender, TextChangedEventArgs e) {
            toolStripMenuItemCompare1.Enabled = false;
            toolStripMenuItemCompare2.Enabled = false;
        }

        private void Tb2_TextChanged2(object sender, TextChangedEventArgs e) {
            toolStripMenuItemCompare1.Enabled = false;
            toolStripMenuItemCompare2.Enabled = false;
        }

        private void Tb1_TextChangedDelayed(object sender, TextChangedEventArgs e) {
//            Text1 = tb1.GetNumeredText();
            toolStripMenuItemCompare1.Enabled = true;
            toolStripMenuItemCompare2.Enabled = true;
        }

        private void Tb2_TextChangedDelayed(object sender, TextChangedEventArgs e) {
//            Text2 = tb2.GetNumeredText();
            toolStripMenuItemCompare1.Enabled = true;
            toolStripMenuItemCompare2.Enabled = true;
        }

        private void Tb1_Scroll(object sender, EventArgs e) {
            tb2.SetVerticalScrollValueNoEvent(tb1.GetVerticalScrollValue());
        }

        private void Tb2_Scroll(object sender, EventArgs e) {
            tb1.SetVerticalScrollValueNoEvent(tb2.GetVerticalScrollValue());
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
                fileFialog.FileName = Path.GetFileName(filename);
            }
            fileFialog.Filter = FileDialogFilter();
            fileFialog.FilterIndex = FilterIndex;
            fileFialog.RestoreDirectory = true;
            fileFialog.Title = @"Выбор файла скрипта";
            if (fileFialog.ShowDialog() == DialogResult.OK) {
                filename = fileFialog.FileName;
                if (File.Exists(filename)) {
                    text = File.ReadAllText(filename);
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
                Color green = Color.FromArgb(180, Color.LightGreen);

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
                                tb2.AddLine(((TextLine)dLF.GetByIndex(drs.DestIndex   + i)).Line, Color.Transparent, ref iLine2);
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
                                tb1.AddLine(((TextLine)sLF.GetByIndex(drs.SourceIndex + i)).Line, red  , ref iLine1);
                                tb2.AddLine(((TextLine)dLF.GetByIndex(drs.DestIndex   + i)).Line, green, ref iLine2);
                            }
                            break;
                    }

                }
                tb1.NeedRecalc();
                tb2.NeedRecalc();
                tb1.RefreshTheme();
                tb2.RefreshTheme();


            } catch (Exception ex) {
                HMS.LogError(ex);
            }

        }

        protected override void OnShown(EventArgs e) {
            Compare();
            base.OnShown(e);
        }

        private void toolStripMenuItemLoad1_Click(object sender, EventArgs e) {
            if (OpenFile(ref Text1, ref File1)) {
                Compare();
            }
        }

        private void toolStripMenuItemLoad2_Click(object sender, EventArgs e) {
            if (OpenFile(ref Text2, ref File2)) {
                Compare();
            }
        }

        private void toolStripMenuItemCompare2_Click(object sender, EventArgs e) {
            Compare();
        }

        private void toolStripMenuItemInsert1_Click(object sender, EventArgs e) {
            Text1 = Clipboard.GetText();
            Compare();
        }

        private void toolStripMenuItemInsert2_Click(object sender, EventArgs e) {
            Text1 = Clipboard.GetText();
            Compare();
        }

        private void toolStripMenuItemCompare1_Click(object sender, EventArgs e) {
            Compare();
        }
    }
}
