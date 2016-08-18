using System;
using System.Drawing;
using System.Windows.Forms;
using DifferenceEngine;
using System.IO;
using System.Collections;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace HMSEditorNS {
    public partial class DiffControl: UserControl {
        public string File1 = "";
        public string File2 = "";

        public string Text1 = "";
        public string Text2 = "";

        public Language Language { get { return tb1.Language; } set { tb1.Language = value; tb2.Language = value; } }

        private int FilterIndex = 1;
        private List<int> GreenLines1 = new List<int>();
        private List<int> GreenLines2 = new List<int>();
        private List<int> RedLines1   = new List<int>();
        private List<int> RedLines2   = new List<int>();
        private int LineCount1 = 0;
        private int LineCount2 = 0;

        Color ColorBackLines  = Color.FromArgb(230, 231, 232);
        Color ColorRedLines   = Color.FromArgb(255, 102, 102);
        Color ColorGreenLines = Color.FromArgb(118, 146, 60 );
        Color ColorVision     = Color.FromArgb(121, 121, 121);

        public DiffControl() {
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
            PrepareTB(tb1);
            PrepareTB(tb2);
            tb1.VerticalScrollValueChanged += Tb1_VerticalScrollValueChanged;
            tb2.VerticalScrollValueChanged += Tb2_VerticalScrollValueChanged;
            tb1.VerticalScrollVisible = false;
        }

        private void Tb2_VerticalScrollValueChanged(object sender, EventArgs e) {
            tb1.SetVerticalScrollValueNoEvent(tb2.GetVerticalScrollValue());
            Invalidate();
        }

        private void Tb1_VerticalScrollValueChanged(object sender, EventArgs e) {
            tb2.SetVerticalScrollValueNoEvent(tb1.GetVerticalScrollValue());
            Invalidate();
        }

        private void GetBoundsBars(out int x1, out int x2, out int y, out int w, out int h) {
            Padding pad = new Padding(10, 5, 10, 5);
            w = 16;
            h = Height - pad.Top - pad.Bottom;
            y = pad.Top;
            x2 = Width - w - pad.Right;
            x1 = x2 - w - 10;
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            int x1, x2, y, w, h;
            GetBoundsBars(out x1, out x2, out y, out w, out h);
            Rectangle rect1 = new Rectangle(x1, y, w, h);
            Rectangle rect2 = new Rectangle(x2, y, w, h);

            Brush brushBack  = new SolidBrush(ColorBackLines);
            Brush brushGreen = new SolidBrush(ColorGreenLines);
            Brush brushRed   = new SolidBrush(ColorRedLines);
            Pen   penVis     = new Pen(ColorVision, 3);

            g.FillRectangle(brushBack, rect1);
            g.FillRectangle(brushBack, rect2);
            if (LineCount1 > 0) {
                int we1 = (int)Math.Round((double)h / LineCount1);
                foreach(int iLine in GreenLines1) {
                    int ye = y + (int)Math.Round((double)h * iLine / LineCount1);
                    g.FillRectangle(brushGreen, x1, ye, w, we1 + 1);
                }
                foreach (int iLine in RedLines1) {
                    int ye = y + (int)Math.Round((double)h * iLine / LineCount1);
                    g.FillRectangle(brushRed, x1, ye, w, we1 + 1);
                }
            }

            if (LineCount2 > 0) {
                int we2 = (int)Math.Round((double)h / LineCount2);
                foreach (int iLine in GreenLines2) {
                    int ye = y + (int)Math.Round((double)h * iLine / LineCount2);
                    g.FillRectangle(brushGreen, x2, ye, w, we2 + 1);
                }
                foreach (int iLine in RedLines2) {
                    int ye = y + (int)Math.Round((double)h * iLine / LineCount2);
                    g.FillRectangle(brushRed, x2, ye, w, we2 + 1);
                }
            }
            if (LineCount1 > 0 && LineCount2 > 0) {
                int iLineTop, iLineBot;
                GetTopAndBottomLineNums(tb1, out iLineTop, out iLineBot);

                int y1top = y + (int)Math.Round((double)h * iLineTop / LineCount1);
                int y1bot = y + (int)Math.Round((double)h * iLineBot / LineCount1);

                GetTopAndBottomLineNums(tb2, out iLineTop, out iLineBot);

                int y2top = y + (int)Math.Round((double)h * iLineTop / LineCount2);
                int y2bot = y + (int)Math.Round((double)h * iLineBot / LineCount2);

                bar1vis = (y1bot - y1top) / 2;
                bar2vis = (y2bot - y2top) / 2;
                GraphicsPath path = GetVisionPath(new Rectangle(x1 - 4, y1top, w + 8, y1bot - y1top), new Rectangle(x2 - 4, y2top, w + 8, y2bot - y2top), 5);

                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawPath(penVis, path);
            }

            brushBack .Dispose();
            brushGreen.Dispose();
            brushRed  .Dispose();
            penVis    .Dispose();

            base.OnPaint(e);
        }
        int bar1vis = 0;
        int bar2vis = 0;
        public static GraphicsPath GetVisionPath(Rectangle bounds1, Rectangle bounds2, int radius) {
            int diameter = radius * 2;
            Size         size = new Size(diameter, diameter);
            Rectangle    arc  = new Rectangle(bounds1.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0) {
                path.AddRectangle(bounds1);
                return path;
            }
            path.AddArc(arc, 180, 90); // top left arc  
            path.AddLine(bounds1.X + radius, bounds1.Y, bounds1.Right, bounds1.Y);
            path.StartFigure();
            path.AddLine(bounds1.X, bounds1.Y + radius, bounds1.X, bounds1.Bottom - radius + 2);
            path.StartFigure();
            arc.Y = bounds1.Bottom - diameter;
            arc.X = bounds1.Left;
            path.AddArc(arc,  90, 90); // bottom left arc 
            path.StartFigure();
            path.AddLine(bounds1.X + radius, bounds1.Bottom, bounds1.Right, bounds1.Bottom);

            path.AddLine(bounds2.X, bounds2.Bottom, bounds2.Right - radius, bounds2.Bottom);
            arc.X = bounds2.Right  - diameter;
            arc.Y = bounds2.Bottom - diameter;
            path.StartFigure();
            path.AddArc(arc,   0, 90); // bottom right arc  

            path.StartFigure();
            path.AddLine(bounds2.Right, bounds2.Bottom - radius + 2, bounds2.Right, bounds2.Y + radius);

            path.StartFigure();
            arc.Y = bounds2.Y;
            path.AddArc(arc, 270, 90); // top right arc  

            path.StartFigure();
            path.AddLine(bounds2.Right - radius, bounds2.Y, bounds2.X, bounds2.Y);

            path.AddLine(bounds2.X, bounds2.Y, bounds1.Right, bounds1.Y);

            return path;
        }

        private void GetTopAndBottomLineNums(FastColoredTextBox tb, out int top, out int bot) {
            int iTopLine = tb.YtoLineIndex();
            int iLine = iTopLine;
            int countVisibleLines = tb.ClientSize.Height / tb.CharHeight;
            Line line = tb.GetRealLine(iLine);
            while (line.Unavaliable && iLine > 0) {
                iLine--;
                line = tb.GetRealLine(iLine);
            }
            top = Math.Max(line.LineNo - 1, 0);
            bot = top;
            for (int i = 0; i <= countVisibleLines; i++) {
                iLine = iTopLine + i;
                if (iLine >= tb.LinesCount) break;
                line = tb.GetRealLine(iLine);
                if (!line.Unavaliable) bot = line.LineNo - 1;
            }
            bot++;
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

        DiffList_TextFile sLF = new DiffList_TextFile();
        DiffList_TextFile dLF = new DiffList_TextFile();

        public void Compare() {

            sLF.LoadText(Text1);
            dLF.LoadText(Text2);

            try {
                double time = 0;
                DiffEngine de = new DiffEngine();
                time = de.ProcessDiff(sLF, dLF, DiffEngineLevel.Medium);

                ArrayList rep = de.DiffReport();

                Color red   = Color.FromArgb(100, Color.Red);
                Color green = Color.FromArgb(80, Color.Green);

                GreenLines1.Clear();
                GreenLines2.Clear();
                RedLines1.Clear();
                RedLines2.Clear();
                tb1.ClearAllLines();
                tb2.ClearAllLines();

                int i;
                LineCount1 = 1;
                LineCount2 = 1;
                foreach (DiffResultSpan drs in rep) {
                    switch (drs.Status) {
                        case DiffResultSpanStatus.DeleteSource:
                            for (i = 0; i < drs.Length; i++) {
                                tb1.AddLine(((TextLine)sLF.GetByIndex(drs.SourceIndex + i)).Line, red, ref LineCount1);
                                tb2.AddUnavaliableLine();
                                RedLines1.Add(drs.SourceIndex + i);
                            }
                            break;
                        case DiffResultSpanStatus.NoChange:
                            for (i = 0; i < drs.Length; i++) {
                                tb1.AddLine(((TextLine)sLF.GetByIndex(drs.SourceIndex + i)).Line, Color.Transparent, ref LineCount1);
                                tb2.AddLine(((TextLine)dLF.GetByIndex(drs.DestIndex   + i)).Line, Color.Transparent, ref LineCount2);
                            }
                            break;
                        case DiffResultSpanStatus.AddDestination:
                            for (i = 0; i < drs.Length; i++) {
                                tb1.AddUnavaliableLine();
                                tb2.AddLine(((TextLine)dLF.GetByIndex(drs.DestIndex + i)).Line, green, ref LineCount2);
                                GreenLines2.Add(drs.DestIndex + i);
                            }
                            break;
                        case DiffResultSpanStatus.Replace:
                            for (i = 0; i < drs.Length; i++) {
                                tb1.AddLine(((TextLine)sLF.GetByIndex(drs.SourceIndex + i)).Line, red  , ref LineCount1);
                                tb2.AddLine(((TextLine)dLF.GetByIndex(drs.DestIndex   + i)).Line, green, ref LineCount2);
                                RedLines1  .Add(drs.SourceIndex + i);
                                GreenLines2.Add(drs.DestIndex   + i);
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
            Invalidate();

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

        private void DiffControl_Load(object sender, EventArgs e) {

        }

        bool ThumbIsDown;
        private void SetScrollByMouseY(int mx, int my) {
            int x1, x2, y, w, h;
            GetBoundsBars(out x1, out x2, out y, out w, out h);
            int halfgap = (x2 - x1 - w) / 2;
            Rectangle rectBar1 = new Rectangle(x1, y, w + halfgap, h);
            Rectangle rectBar2 = new Rectangle(x2 - halfgap, y, w + halfgap, h);
            if        (rectBar1.Contains(mx, my)) {
                double kbar = ((double)(my - y - bar1vis) / h);
                tb1.SetVerticalScrollKoef(kbar);
            } else if (rectBar2.Contains(mx, my)) {
                double kbar = ((double)(my - y - bar2vis) / h);
                tb2.SetVerticalScrollKoef(kbar);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            int x1, x2, y, w, h;
            GetBoundsBars(out x1, out x2, out y, out w, out h);
            Rectangle rectBars = new Rectangle(x1, y, x2 - x1 + w, h);
            if (rectBars.Contains(e.X, e.Y)) {
                if (h > 0) {
                    ThumbIsDown = true;
                    SetScrollByMouseY(e.X, e.Y);
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            ThumbIsDown = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseEnter(EventArgs e) {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (ThumbIsDown) {
                SetScrollByMouseY(e.X, e.Y);
            }
            base.OnMouseMove(e);
        }

    }
}
