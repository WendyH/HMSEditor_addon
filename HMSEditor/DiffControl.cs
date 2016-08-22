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

        public int FilterIndex = 2;

        private List<int> GreenLines1 = new List<int>();
        private List<int> GreenLines2 = new List<int>();
        private List<int> RedLines1 = new List<int>();
        private List<int> RedLines2 = new List<int>();
        private int LineCount1 = 0;
        private int LineCount2 = 0;

        Color ColorBackLines = Color.FromArgb(230, 231, 232);
        Color ColorRedLines = Color.FromArgb(255, 102, 102);
        Color ColorGreenLines = Color.FromArgb(118, 146, 60);
        Color ColorVision = Color.FromArgb(111, 111, 111);

        public DiffControl() {
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
            PrepareTB(tb1);
            PrepareTB(tb2);
            tb1.VerticalScrollValueChanged += Tb1_VerticalScrollValueChanged;
            tb2.VerticalScrollValueChanged += Tb2_VerticalScrollValueChanged;
            tb1.HorizontalScrollValueChanged += Tb1_HorizontalScrollValueChanged;
            tb2.HorizontalScrollValueChanged += Tb2_HorizontalScrollValueChanged;
            tb1.ZoomChanged += Tb1_ZoomChanged;
            tb2.ZoomChanged += Tb2_ZoomChanged;
            tb1.VerticalScrollVisible = false;
        }

        private void Tb1_ZoomChanged(object sender, EventArgs e) {
            tb2.SetZoomWithoutEvent(tb1.Zoom);
        }

        private void Tb2_ZoomChanged(object sender, EventArgs e) {
            tb1.SetZoomWithoutEvent(tb2.Zoom);
        }

        private void Tb1_HorizontalScrollValueChanged(object sender, EventArgs e) {
            tb2.SetHorizontalScrollValueNoEvent(tb1.GetHorizontalScrollValue());
            Refresh();
        }

        private void Tb2_HorizontalScrollValueChanged(object sender, EventArgs e) {
            tb1.SetHorizontalScrollValueNoEvent(tb2.GetHorizontalScrollValue());
            Refresh();
        }

        private void Tb2_VerticalScrollValueChanged(object sender, EventArgs e) {
            tb1.SetVerticalScrollValueNoEvent(tb2.GetVerticalScrollValue());
            Refresh();
        }

        private void Tb1_VerticalScrollValueChanged(object sender, EventArgs e) {
            tb2.SetVerticalScrollValueNoEvent(tb1.GetVerticalScrollValue());
            Refresh();
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

            Brush brushBack = new SolidBrush(ColorBackLines);
            Brush brushGreen = new SolidBrush(ColorGreenLines);
            Brush brushRed = new SolidBrush(ColorRedLines);
            Pen penVis = new Pen(ColorVision, 3);

            g.FillRectangle(brushBack, rect1);
            g.FillRectangle(brushBack, rect2);
            if (LineCount1 > 0) {
                int we1 = (int)Math.Round((double)h / LineCount1);
                foreach (int iLine in GreenLines1) {
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

                GraphicsPath path = GetVisionPath(new Rectangle(x1 - 4, y1top, w + 8, y1bot - y1top), new Rectangle(x2 - 4, y2top, w + 8, y2bot - y2top), 5);

                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawPath(penVis, path);
            }

            brushBack.Dispose();
            brushGreen.Dispose();
            brushRed.Dispose();
            penVis.Dispose();

            base.OnPaint(e);
        }

        public static GraphicsPath GetVisionPath(Rectangle bounds1, Rectangle bounds2, int radius) {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds1.Location, size);
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
            path.AddArc(arc, 90, 90); // bottom left arc 
            path.StartFigure();
            path.AddLine(bounds1.X + radius, bounds1.Bottom, bounds1.Right, bounds1.Bottom);

            path.AddLine(bounds2.X, bounds2.Bottom, bounds2.Right - radius, bounds2.Bottom);
            arc.X = bounds2.Right - diameter;
            arc.Y = bounds2.Bottom - diameter;
            path.StartFigure();
            path.AddArc(arc, 0, 90); // bottom right arc  

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
            tb.ShowBeginOfFunctions = false;
            tb.ShowChangedLinesOnScrollbar = false;
            tb.ShowFoldingLines = false;
            tb.EnableFoldingIndicator = false;
            tb.ShowFoldingMarkers = false;
            tb.DrawLineNumberFromInfo = true;
            tb.ShowInvisibleCharsInSelection = true;
            tb.SelectionWithBorders = true;
            tb.AllowSeveralTextStyleDrawing = false;
            tb.AllowInsertRemoveLines = false;
            tb.ReadOnly = true;
            //tb.Font = new System.Drawing.Font("Monospace", 10f); // in linux
            Themes.SetTheme(tb, "Стандартная");
            tb.Paint += Tb_Paint;
        }

        private void Tb_Paint(object sender, PaintEventArgs e) {
            FastColoredTextBox tb = sender as FastColoredTextBox;
            if (tb == null) return;

            lock (tb.Lines) {
                int startLine = tb.YtoLineIndex();
                int vertScrolValue = tb.GetVerticalScrollValue();
                int horzScrolValue = tb.GetHorizontalScrollValue();
                int iLine;
                int CharHeight = tb.CharHeight;
                int CharWidth = tb.CharWidth;

                int firstChar = (Math.Max(0, horzScrolValue - tb.Paddings.Left)) / CharWidth;
                int lastChar = (horzScrolValue + ClientSize.Width) / CharWidth;
                int x = tb.LeftIndent + tb.Paddings.Left - horzScrolValue;
                if (x < tb.LeftIndent) firstChar++;

                for (iLine = startLine; iLine < tb.LinesCount; iLine++) {
                    Graphics gr = e.Graphics;
                    Line line = tb[iLine];
                    LineInfo lineInfo = tb.LineInfos[iLine];

                    if (lineInfo.startY > vertScrolValue + ClientSize.Height)
                        break;
                    if (lineInfo.startY + lineInfo.WordWrapStringsCount * CharHeight < vertScrolValue)
                        continue;
                    if (lineInfo.VisibleState == VisibleState.Hidden)
                        continue;

                    int y = lineInfo.startY - vertScrolValue;

                    gr.SmoothingMode = SmoothingMode.None;
                    Range textRange = new Range(tb, firstChar, iLine, lastChar + 1, iLine);

                    if (tb == tb1) {
                        foreach (Range rr in RangesRed) {
                            Range withTextRange = rr.GetIntersectionWith(textRange);
                            if (withTextRange != null && withTextRange.Start != withTextRange.End) {
                                Range r = rr.Clone();
                                r.Normalize();
                                tb.StyleDiffRed?.Draw(gr, x, 0, y, withTextRange, r);
                            }
                        }
                    } else {
                        foreach (Range rr in RangesGreen) {
                            Range withTextRange = rr.GetIntersectionWith(textRange);
                            if (withTextRange != null && withTextRange.Start != withTextRange.End) {
                                Range r = rr.Clone();
                                r.Normalize();
                                tb.StyleDiffGreen?.Draw(gr, x, 0, y, withTextRange, r);
                            }
                        }
                    }

                }
            }
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
                            text = HttpUtility.HtmlDecode(m.Groups[1].Value).Replace("&apos;", "'");
                            switch (m.Groups[2].Value) {
                                case "C++Script": Language = Language.CPPScript; break;
                                case "PascalScript": Language = Language.PascalScript; break;
                                case "BasicScript": Language = Language.BasicScript; break;
                                case "JScript": Language = Language.JScript; break;
                                default: Language = Language.YAML; break;
                            }
                        }
                    }
                    success = true;
                }
            }
            FilterIndex = fileFialog.FilterIndex;
            return success;
        }

        public void Compare() {
            RangesGreen.Clear();
            RangesRed.Clear();
            try {
                DiffEngine de = new DiffEngine(Text1, Text2);
                ArrayList rep = de.ProcessDiff(DiffEngineLevel.Medium);

                Color red = Color.FromArgb(100, Color.Red);
                Color green = Color.FromArgb(80, Color.Green);

                GreenLines1.Clear();
                GreenLines2.Clear();
                RedLines1.Clear();
                RedLines2.Clear();

                lock (tb1.Lines) {
                    lock (tb2.Lines) {
                        tb1.Clear();
                        tb2.Clear();
                        int i;
                        LineCount1 = 0;
                        LineCount2 = 0;
                        foreach (DiffResultSpan drs in rep) {
                            switch (drs.Status) {
                                case DiffResultSpanStatus.DeleteSource:
                                    for (i = 0; i < drs.Length; i++) {
                                        string line1 = de.GetSrcLineByIndex(drs.SourceIndex + i);
                                        tb1.AddLine(line1, red, ref LineCount1);
                                        tb2.AddUnavaliableLine();
                                        RedLines1.Add(drs.SourceIndex + i);
                                        DiffChars(line1, "");
                                        if (line1.Length == 0)
                                            SetRangeStyle(tb1, 0, 0, true);
                                    }
                                    break;
                                case DiffResultSpanStatus.NoChange:
                                    for (i = 0; i < drs.Length; i++) {
                                        tb1.AddLine(de.GetSrcLineByIndex(drs.SourceIndex + i), Color.Transparent, ref LineCount1);
                                        tb2.AddLine(de.GetDstLineByIndex(drs.DestIndex + i), Color.Transparent, ref LineCount2);
                                    }
                                    break;
                                case DiffResultSpanStatus.AddDestination:
                                    for (i = 0; i < drs.Length; i++) {
                                        string line2 = de.GetDstLineByIndex(drs.DestIndex + i);
                                        tb1.AddUnavaliableLine();
                                        tb2.AddLine(line2, green, ref LineCount2);
                                        GreenLines2.Add(drs.DestIndex + i);
                                        DiffChars("", line2);
                                        if (line2.Length == 0)
                                            SetRangeStyle(tb2, 0, 0, false);
                                    }
                                    break;
                                case DiffResultSpanStatus.Replace:
                                    for (i = 0; i < drs.Length; i++) {
                                        string line1 = de.GetSrcLineByIndex(drs.SourceIndex + i);
                                        string line2 = de.GetDstLineByIndex(drs.DestIndex + i);
                                        tb1.AddLine(line1, red, ref LineCount1);
                                        tb2.AddLine(line2, green, ref LineCount2);
                                        RedLines1.Add(drs.SourceIndex + i);
                                        GreenLines2.Add(drs.DestIndex + i);
                                        DiffChars(line1, line2);
                                    }
                                    break;
                            }
                        }
                    }
                }

                tb1.NeedRecalc(true);
                tb2.NeedRecalc(true);
                tb1.OnSyntaxHighlight(new TextChangedEventArgs(tb1.Range));
                tb2.OnSyntaxHighlight(new TextChangedEventArgs(tb2.Range));

            } catch (Exception ex) {
                HMS.LogError(ex);
            }
            Invalidate();

        }

        private void DiffChars(string line1, string line2) {
            var del = new DiffEngine(line1, line2, true);
            ArrayList res = del.ProcessDiff(DiffEngineLevel.FastImperfect);
            foreach (DiffResultSpan drs in res) {
                switch (drs.Status) {
                    case DiffResultSpanStatus.DeleteSource:
                        SetRangeStyle(tb1, drs.SourceIndex, drs.SourceIndex + drs.Length, true);
                        break;

                    case DiffResultSpanStatus.AddDestination:
                        SetRangeStyle(tb2, drs.DestIndex, drs.DestIndex + drs.Length, false);
                        break;

                    case DiffResultSpanStatus.Replace:
                        SetRangeStyle(tb1, drs.SourceIndex, drs.SourceIndex + drs.Length, true);
                        SetRangeStyle(tb2, drs.DestIndex, drs.DestIndex + drs.Length, false);
                        break;
                }

            }

        }
        List<Range> RangesGreen = new List<Range>();
        List<Range> RangesRed = new List<Range>();

        private Range SetRangeStyle(FastColoredTextBox tb, int iChar1, int iChar2, bool isRed) {
            int iLine = tb.LinesCount - 1;
            Range r = new Range(tb, iChar1, iLine, iChar2, iLine);
            if (isRed) {
                if (RangesRed.Count > 0) {
                    Range pr = RangesRed[RangesRed.Count - 1];
                    int plen = tb.Lines[pr.End.iLine].Length;
                    if ((pr.End >= r.Start) || (pr.End.iLine == iLine - 1 && pr.End.iChar == plen)) {
                        RangesRed.Remove(pr);
                        r = new Range(tb, pr.Start, r.End);
                    }
                }
                RangesRed.Add(r);
            } else {
                if (RangesGreen.Count > 0) {
                    Range pr = RangesGreen[RangesGreen.Count - 1];
                    int plen = tb.Lines[pr.End.iLine].Length;
                    if ((pr.End >= r.Start) || (pr.End.iLine == iLine - 1 && pr.End.iChar >= plen)) {
                        RangesGreen.Remove(pr);
                        r = new Range(tb, pr.Start, r.End);
                    }
                }
                RangesGreen.Add(r);
            }
            return r;
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
        int tbnum = 0;
        protected override void OnMouseDown(MouseEventArgs e) {
            int x1, x2, y, w, h;
            GetBoundsBars(out x1, out x2, out y, out w, out h);
            Rectangle rectBars = new Rectangle(x1, y, x2 - x1 + w, h);
            if (rectBars.Contains(e.X, e.Y)) {
                if (h > 0) {
                    Rectangle rectBar1 = new Rectangle(x1, y, w + (x2 - x1 - w) / 2, h);
                    tbnum = rectBar1.Contains(e.X, e.Y) ? 0 : 1;
                    GoToLineByY(e.Y);
                    ThumbIsDown = true;
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

        private void GoToLineByY(int my) {
            int x1, x2, y, w, h;
            GetBoundsBars(out x1, out x2, out y, out w, out h);
            if (tbnum == 0) {
                int i = (int)Math.Round((float)(my - y) * LineCount1 / h);
                tb1.NavigateToLineNum(i);
            } else {
                int i = (int)Math.Round((float)(my - y) * LineCount2 / h);
                tb2.NavigateToLineNum(i);
            }
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (ThumbIsDown) {
                GoToLineByY(e.Y);
            }
            base.OnMouseMove(e);
        }

        private bool ItsTb1(object sender) {
            ToolStripItem item = sender as ToolStripItem;
            if (item != null) {
                ToolStrip ts = item.GetCurrentParent();
                if (ts.Bounds.IntersectsWith(tb1.RectangleToScreen(tb1.Bounds)))
                    return true;
            }
            return false;
        }

        private void ToolStripMenuItemSelectAll_Click(object sender, EventArgs e) {
            if (ItsTb1(sender))
                tb1.SelectAll();
            else
                tb2.SelectAll();
        }

        private void ToolStripMenuItemCopy_Click(object sender, EventArgs e) {
            if (ItsTb1(sender))
                tb1.Copy();
            else
                tb2.Copy();
        }

        private void ToolStripMenuItemZoom100_Click(object sender, EventArgs e) {
            if (ItsTb1(sender))
                tb1.Zoom = 100;
            else
                tb2.Zoom = 100;
        }
    }
}
