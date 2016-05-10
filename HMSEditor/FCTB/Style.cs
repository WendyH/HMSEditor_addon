using System.Drawing;
using System;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace FastColoredTextBoxNS
{
    /// <summary>
    /// Style of chars
    /// </summary>
    /// <remarks>This is base class for all text and design renderers</remarks>
    public abstract class Style : IDisposable
    {
        /// <summary>
        /// This style is exported to outer formats (HTML for example)
        /// </summary>
        public bool IsExportable { get; set; }
        /// <summary>
        /// Occurs when user click on StyleVisualMarker joined to this style 
        /// </summary>
        public event EventHandler<VisualMarkerEventArgs> VisualMarkerClick;

        /// <summary>
        /// Constructor
        /// </summary>
        protected Style()
        {
            IsExportable = true;
        }

        /// <summary>
        /// Renders given range of text
        /// </summary>
        /// <param name="gr">Graphics object</param>
        /// <param name="position">Position of the range in absolute control coordinates</param>
        /// <param name="range">Rendering range of text</param>
        public abstract void Draw(Graphics gr, Point position, Range range);

        /// <summary>
        /// Occurs when user click on StyleVisualMarker joined to this style 
        /// </summary>
        public virtual void OnVisualMarkerClick(FastColoredTextBox tb, VisualMarkerEventArgs args)
        {
            VisualMarkerClick?.Invoke(tb, args);
        }

        /// <summary>
        /// Shows VisualMarker
        /// Call this method in Draw method, when you need to show VisualMarker for your style
        /// </summary>
        protected virtual void AddVisualMarker(FastColoredTextBox tb, StyleVisualMarker marker)
        {
            tb.AddVisualMarker(marker);
        }

        public static Size GetSizeOfRange(Range range)
        {
            return new Size((range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
        }

        public static GraphicsPath GetRoundedRectangle(Rectangle rect, int d)
        {
            var gp = new GraphicsPath();

            gp.AddArc(rect.X, rect.Y, d, d, 180, 90);
            gp.AddArc(rect.X + rect.Width - d, rect.Y, d, d, 270, 90);
            gp.AddArc(rect.X + rect.Width - d, rect.Y + rect.Height - d, d, d, 0, 90);
            gp.AddArc(rect.X, rect.Y + rect.Height - d, d, d, 90, 90);
            gp.AddLine(rect.X, rect.Y + rect.Height - d, rect.X, rect.Y + d / 2);

            return gp;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) { }

        /// <summary>
        /// Returns CSS for export to HTML
        /// </summary>
        /// <returns></returns>
        public virtual string GetCSS()
        {
            return "";
        }

        /// <summary>
        /// Returns RTF descriptor for export to RTF
        /// </summary>
        /// <returns></returns>
        public virtual RTFStyleDescriptor GetRTF()
        {
            return new RTFStyleDescriptor();
        }
    }

    public class ErrorStyle: Style {
        public string Message = "";
        public override void Draw(Graphics gr, Point position, Range range) {
            int charH = range.tb.CharHeight;
            int charW = range.tb.CharWidth;
            int linedx = charW / 2;
            int linedy = charH / 6;
            Pen redPen = new Pen(Color.Red, (float)charH / 10);
            float x1 = position.X - (float)range.tb.CharWidth / 3;
            float y1 = position.Y + (float)range.tb.LineInterval / 2 + charH;
            int charCount = range.End.iChar - range.Start.iChar;
            List<PointF> pointList = new List<PointF>();
            for (int i = 0; i < charCount; i++) {
                pointList.Add(new PointF(x1, y1));
                pointList.Add(new PointF(x1 + linedx, y1 - linedy));
                pointList.Add(new PointF(x1 + linedx + linedx, y1));
                x1 += charW;
            }
            gr.DrawLines(redPen, pointList.ToArray());
        }
    }


    /// <summary>
    /// Style for chars rendering
    /// This renderer can draws chars, with defined fore and back colors
    /// </summary>
    public class TextStyle : Style
    {
        public Brush        ForeBrush       { get; set; }
        public Brush        BackgroundBrush { get; set; }
        public FontStyle    FontStyle       { get; set; }
        public StringFormat Sf              { get; set; }

        protected new virtual void Dispose(bool isDispose) {
            if (isDispose) {
                ForeBrush      ?.Dispose();
                BackgroundBrush?.Dispose();
                Sf             ?.Dispose();
            }
            ForeBrush       = null;
            BackgroundBrush = null;
            Sf              = null;
        }

        public TextStyle(Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle)
        {
            ForeBrush       = foreBrush;
            BackgroundBrush = backgroundBrush;
            FontStyle       = fontStyle;
            Sf = new StringFormat(StringFormatFlags.MeasureTrailingSpaces);
        }

        public void RefreshColors(FastColoredTextBox tb) {
            ForeBrush = new SolidBrush(tb.ForeColor);
        }

        public TextStyle Clone(int val=0) {
            if (ForeBrush == null) return null;
            Color c = ((SolidBrush)ForeBrush).Color;
            int r = Math.Max(0, c.R - val);
            int g = Math.Max(0, c.G - val);
            int b = Math.Max(0, c.B - val);
            Brush newForeBrush = new SolidBrush(Color.FromArgb(r, g, b));
            return new TextStyle(newForeBrush, BackgroundBrush, FontStyle);
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            int charH = range.tb.CharHeight;
            int charW = range.tb.CharWidth;
            int rangeH = charH;
            int rangeW = (range.End.iChar - range.Start.iChar) * charW;
            float dx = charW;
            float y = position.Y + (float)range.tb.LineInterval / 2;
            float x = position.X - (float)charW / 3;
            //draw background
            if (BackgroundBrush != null)
                gr.FillRectangle(BackgroundBrush, position.X, position.Y, rangeW, rangeH);

            //draw chars
            using (var f = new Font(range.tb.Font, FontStyle))
            {
                Line line = range.tb[range.Start.iLine];

                if (ForeBrush == null)
                    ForeBrush = new SolidBrush(range.tb.ForeColor);

                if (range.tb.ImeAllowed)
                {
                    //IME mode
                    for (int i = range.Start.iChar; i < range.End.iChar; i++)
                    {
                        SizeF size = FastColoredTextBox.GetCharSize(f, line[i].c);

                        var gs = gr.Save();
                        float k = size.Width > charW + 1 ? charW / size.Width : 1;
                        gr.TranslateTransform(x, y + (1 - k)*range.tb.CharHeight/2);
                        gr.ScaleTransform(k, (float) Math.Sqrt(k));
                        gr.DrawString(line[i].c.ToString(), f, ForeBrush, 0, 0, Sf);
                        gr.Restore(gs);
                        x += dx;
                    }
                }
                else
                {
                    //classic mode 
                    for (int i = range.Start.iChar; i < range.End.iChar; i++)
                    {
                        //draw char
                        gr.DrawString(line[i].c.ToString(), f, ForeBrush, x, y, Sf);
                        x += dx;
                    }
                }
            }
        }

        public override string GetCSS()
        {
            string result = "";

            var brush = BackgroundBrush as SolidBrush;
            if (brush != null)
            {
                var s =  ExportToHTML.GetColorAsString(brush.Color);
                if (s != "")
                    result += "background-color:" + s + ";";
            }
            var foreBrush = ForeBrush as SolidBrush;
            if (foreBrush != null)
            {
                var s = ExportToHTML.GetColorAsString(foreBrush.Color);
                if (s != "")
                    result += "color:" + s + ";";
            }
            if ((FontStyle & FontStyle.Bold) != 0)
                result += "font-weight:bold;";
            if ((FontStyle & FontStyle.Italic) != 0)
                result += "font-style:oblique;";
            if ((FontStyle & FontStyle.Strikeout) != 0)
                result += "text-decoration:line-through;";
            if ((FontStyle & FontStyle.Underline) != 0)
                result += "text-decoration:underline;";

            return result;
        }

        public override RTFStyleDescriptor GetRTF()
        {
            var result = new RTFStyleDescriptor();

            var brush = BackgroundBrush as SolidBrush;
            if (brush != null)
                result.BackColor = brush.Color;

            var foreBrush = ForeBrush as SolidBrush;
            if (foreBrush != null)
                result.ForeColor = foreBrush.Color;
            
            if ((FontStyle & FontStyle.Bold) != 0)
                result.AdditionalTags += @"\b";
            if ((FontStyle & FontStyle.Italic) != 0)
                result.AdditionalTags += @"\i";
            if ((FontStyle & FontStyle.Strikeout) != 0)
                result.AdditionalTags += @"\strike";
            if ((FontStyle & FontStyle.Underline) != 0)
                result.AdditionalTags += @"\ul";

            return result;
        }
    }

    /// <summary>
    /// Renderer for folded block
    /// </summary>
    public class FoldedBlockStyle : TextStyle
    {
        public FoldedBlockStyle(Brush foreBrush, Brush backgroundBrush, FontStyle fontStyle):
            base(foreBrush, backgroundBrush, fontStyle)
        {
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            if (range.End.iChar > range.Start.iChar)
            {
                base.Draw(gr, position, range);

                int firstNonSpaceSymbolX = position.X;
                
                //find first non space symbol
                for (int i = range.Start.iChar; i < range.End.iChar; i++)
                    if (range.tb[range.Start.iLine][i].c != ' ')
                        break;
                    else
                        firstNonSpaceSymbolX += range.tb.CharWidth;

                //create marker
                range.tb.AddVisualMarker(new FoldedAreaMarker(range.Start.iLine, new Rectangle(firstNonSpaceSymbolX, position.Y, position.X + (range.End.iChar - range.Start.iChar) * range.tb.CharWidth - firstNonSpaceSymbolX, range.tb.CharHeight)));
            }
            else
            {
                //draw '...'
                using(Font f = new Font(range.tb.Font, FontStyle))
                    gr.DrawString("...", f, ForeBrush, range.tb.LeftIndent, position.Y - 2);
                //create marker
                range.tb.AddVisualMarker(new FoldedAreaMarker(range.Start.iLine, new Rectangle(range.tb.LeftIndent + 2, position.Y, 2 * range.tb.CharHeight, range.tb.CharHeight)));
            }
        }
    }

    /// <summary>
    /// Renderer for selected area
    /// </summary>
    public class SelectionStyle : Style
    {
        public Color Background { get; }
        public Color Foreground { get; }
        public Color Border     { get; }

        public SelectionStyle(Color background)
            : this(background, Color.Transparent, Color.Transparent) {
        }

        public SelectionStyle(Color background, Color foreground)
            : this(background, foreground, Color.Transparent) {
        }

        public SelectionStyle(Color background, Color foreground, Color border) {
            IsExportable = false;
            Foreground   = foreground;
            Background   = background;
            Border       = border == Color.Transparent ? Color.FromArgb(245, Background) : border;
        }

        public override void Draw(Graphics gr, Point position, Range range) {
            if (range.Start == range.End) return;
            if (range.tb.SelectionWithBorders) {
                //Point position = new Point(startX + (range.Start.iChar - from) * charW, 1 + y);
                int startX = (position.X - (range.Start.iChar * range.tb.CharWidth));
                Draw(gr, startX, 0, position.Y, range, range);
                return;
            }
            //draw background
            if (Background != Color.Transparent) {
                gr.SmoothingMode = SmoothingMode.None;
                var rect = new Rectangle(position.X, position.Y, (range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
                if (rect.Width <= 0) return;
                using (var brush = new SolidBrush(Color.FromArgb(Background.A - 20, Background))) {
                    gr.FillRectangle(brush, rect);
                }
            }
            if (Foreground != Color.Transparent) {
                //draw text
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                var r = new Range(range.tb, range.Start.iChar, range.Start.iLine, Math.Min(range.tb[range.End.iLine].Count, range.End.iChar), range.End.iLine);
                using (var brush = new SolidBrush(Foreground)) {
                    using (var style = new TextStyle(brush, null, FontStyle.Regular)) {
                        style.Draw(gr, new Point(position.X, position.Y - 1), r);
                    }
                }
            }
        }

        public void Draw(Graphics gr, int startX, int from, int y, Range range, Range selection) {
            if (range.Start == range.End) return;
            int charW = range.tb.CharWidth;
            int charH = range.tb.CharHeight;
            Point position = new Point(startX + (range.Start.iChar - from) * charW, y);
            //draw background
            if (Background != Color.Transparent) {
                gr.SmoothingMode = SmoothingMode.None;
                using (var brush = new SolidBrush(Color.FromArgb(Background.A - 20, Background))) {
                    // Calculate previous and next lines selection rectangles
                    Rectangle rectPrev = new Rectangle();
                    Rectangle rectNext = new Rectangle();

                    int iLine = range.Start.iLine;
                    int startChar = iLine == selection.Start.iLine ? selection.Start.iChar : 0;
                    int lastChar  = iLine == selection.End.iLine   ? selection.End  .iChar : selection.tb.Lines[iLine].Length + 1;
                    Rectangle rectBord = new Rectangle(startX + startChar * charW, position.Y, (lastChar - startChar) * charW, charH);

                    int prevLine = range.Start.iLine - 1;
                    if (prevLine >= selection.Start.iLine) {
                        startChar = 0;
                        if (prevLine == selection.Start.iLine)
                            startChar = selection.Start.iChar;
                        rectPrev = new Rectangle(startX + startChar * charW, position.Y - charH, (selection.tb.Lines[prevLine].Length + 1 - startChar) * charW, charH);
                    }
                    int nextLine = range.End.iLine + 1;
                    if (nextLine <= selection.End.iLine) {
                        lastChar = nextLine == selection.End.iLine ? selection.End.iChar : selection.tb.Lines[nextLine].Length + 1;
                        rectNext = new Rectangle(startX, position.Y + charH, lastChar * charW, charH);
                    }

                    // draw border
                    using (Pen pen = new Pen(Border)) {
                        gr.SmoothingMode = SmoothingMode.None;
                        GraphicsPath path4Fill = new GraphicsPath();
                        GraphicsPath path = GetRoundedPath1(rectBord, rectPrev, rectNext, ref path4Fill, charW);
                        path4Fill.CloseAllFigures();
                        gr.FillPath(brush, path4Fill);
                        gr.SmoothingMode = FastColoredTextBox.RoundedCornersRadius > 3 ? SmoothingMode.HighQuality : SmoothingMode.None;
                        gr.DrawPath(pen, path);
                        gr.SmoothingMode = SmoothingMode.None;
                    }
                }
            }

            if (Foreground != Color.Transparent) {
                //draw text
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                var r = new Range(range.tb, range.Start.iChar, range.Start.iLine, Math.Min(range.tb[range.End.iLine].Count, range.End.iChar), range.End.iLine);
                using (var brush = new SolidBrush(Foreground)) {
                    using (var style = new TextStyle(brush, null, FontStyle.Regular)) {
                        style.Draw(gr, new Point(position.X, position.Y), r);
                    }
                }
            }
        }

        private GraphicsPath GetRoundedPath1(Rectangle rect, Rectangle rectPrev, Rectangle rectNext, ref GraphicsPath path4Fill, int charW) {
            int r = FastColoredTextBox.RoundedCornersRadius; int d = r * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(d, d));
            GraphicsPath path4Bord = new GraphicsPath();
            if (d > rect.Height) {
                d = rect.Height; r = rect.Height / 2;
                arc.Width  = d;
                arc.Height = d;
            }

            if (rectNext.Width >0 && rect.Left > rectNext.Left) {
                arc.X = rect.Left - d;
                arc.Y = rect.Bottom - d;
                var p = new GraphicsPath();
                p.AddArc(arc, 0, 90);
                p.Reverse();
                path4Bord.AddPath(p, true);
                var p2 = new GraphicsPath();
                p2.AddArc(arc, 0, 90);
                p2.Reverse();
                path4Fill.AddPath(p2, true);
            }

            Point pLeftBottom = new Point(rect.Left, rect.Bottom - r);
            Point pLeftTop    = new Point(rect.Left, rect.Top    + r);

            if (rectNext.Width!=0 && rectNext.Left == rect.Left)
                pLeftBottom = new Point(rect.Left, rect.Bottom);

            if (rectPrev.Width!=0 && rectPrev.Left == rect.Left)
                pLeftTop    = new Point(rect.Left, rect.Top   );

            path4Bord.AddLine(pLeftBottom, pLeftTop);
            path4Fill.AddLine(new Point(pLeftBottom.X, pLeftBottom.Y), pLeftTop);
            if (rectPrev.IsEmpty) {
                // This is first line
                arc.X = rect.Left;
                arc.Y = rect.Top;
                path4Bord.AddArc(arc, 180, 90);
                path4Fill.AddArc(arc, 180, 90);
                arc.X = rect.Right - d;
                path4Bord.AddArc(arc, 270, 90);
                path4Fill.AddArc(arc, 270, 90);
                NextLinePath(ref path4Bord, ref path4Fill, rect, rectNext, arc, d, charW);
            } else {
                // This is no first line
                if (rect.Left < rectPrev.Left) {
                    path4Bord.AddArc(arc, 180, 90);
                    path4Fill.AddArc(arc, 180, 90);
                    path4Bord.StartFigure();
                    path4Bord.AddLine(rect.Left + r, rect.Top, rectPrev.Left - r, rect.Top);
                    path4Fill.AddLine(rect.Left + r, rect.Top, rectPrev.Left - r, rect.Top);
                }
                if (rect.Right > rectPrev.Right) {
                    int a = 0;
                    if (d > (rect.Right - rectPrev.Right)) {
                        a = d;
                        d = charW; r = charW / 2;
                        arc.Width  = d;
                        arc.Height = d;
                    }
                    path4Bord.StartFigure();
                    path4Bord.AddLine(rectPrev.Right + r, rect.Top, rect.Right - r, rect.Top);
                    path4Fill.AddLine(rectPrev.Right + r, rect.Top, rect.Right - r, rect.Top);
                    arc.X = rect.Right - d;
                    arc.Y = rect.Top;
                    path4Bord.AddArc(arc, 270, 90);
                    path4Fill.AddArc(arc, 270, 90);
                    if (a > 0) {
                        d = a; r = d / 2;
                        arc.Width  = d;
                        arc.Height = d;
                        arc.X = rect.Right - d;
                        arc.Y = rect.Top;
                    }
                    NextLinePath(ref path4Bord, ref path4Fill, rect, rectNext, arc, d, charW);
                } else if (rect.Right < rectPrev.Right) {
                    int a = 0;
                    if (d > (rectPrev.Right - rect.Right)) {
                        a = d;
                        d = charW; r = charW / 2;
                        arc.Width  = d;
                        arc.Height = d;
                    }
                    path4Bord.StartFigure();
                    arc.Y = rect.Top;
                    arc.X = rect.Right;
                    var p = new GraphicsPath();
                    p.AddArc(arc, 180, 90);
                    p.Reverse();
                    path4Bord.AddPath(p, false);
                    path4Fill.AddPath(p, true);
                    if (a > 0) {
                        d = a; r = d / 2;
                        arc.Width  = d;
                        arc.Height = d;
                    }
                    path4Bord.AddLine(rect.Right, rect.Top + r, rect.Right, rect.Bottom - r);
                    path4Fill.AddLine(rect.Right, rect.Top + r, rect.Right, rect.Bottom - r);
                    arc.X = rect.Right - d;
                    arc.Y = rect.Bottom;
                    NextLinePath(ref path4Bord, ref path4Fill, rect, rectNext, arc, d, charW);
                } else {
                    path4Bord.StartFigure();
                    path4Bord.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom - d);
                    path4Fill.AddLine(rect.Right, rect.Top, rect.Right, rect.Bottom - d);
                    arc.X = rect.Right - d;
                    arc.Y = rect.Bottom;
                    NextLinePath(ref path4Bord, ref path4Fill, rect, rectNext, arc, d, charW);
                }
            }
            return path4Bord;
        }

        private void NextLinePath(ref GraphicsPath path4Bord, ref GraphicsPath path4Fill, Rectangle rect, 
                                  Rectangle rectNext, Rectangle arc, int d, int charW) {
            if (rectNext.Width == 0) {
                arc.X = rect.Right  - d;
                arc.Y = rect.Bottom - d;
                path4Bord.AddArc(arc, 0, 90);
                path4Fill.AddArc(arc, 0, 90);
                arc.X = rect.Left;
                path4Bord.AddArc(arc, 90, 90);
                path4Fill.AddArc(arc, 90, 90);
            } else if (rectNext.Right > rect.Right) {
                if (d > (rectNext.Right - rect.Right)) {
                    d = charW;
                    arc.Width  = d;
                    arc.Height = d;
                }
                arc.X = rect.Right;
                arc.Y = rect.Bottom - d;
                var p1 = new GraphicsPath();
                p1.AddArc(arc, 90, 90);
                p1.Reverse();
                path4Bord.AddPath(p1, true);
                path4Fill.AddPath(p1, true);
            } else if (rectNext.Right < rect.Right) {
                if (d > (rect.Right - rectNext.Right)) {
                    d = charW;
                    arc.Width  = d;
                    arc.Height = d;
                }
                arc.X = rect.Right  - d;
                arc.Y = rect.Bottom - d;
                path4Bord.AddArc(arc, 0, 90);
                path4Fill.AddArc(arc, 0, 90);
                path4Bord.AddLines(new Point[] { new Point(rectNext.Right + d / 2, rect.Bottom) });
                path4Fill.AddLines(new Point[] { new Point(rectNext.Right + d / 2, rect.Bottom) });
            } else {
                path4Bord.AddLines(new Point[] { new Point(rect.Right, rect.Bottom) });
                path4Fill.AddLines(new Point[] { new Point(rect.Right, rect.Bottom+1) });
            }
        }
    }

    /// <summary>
    /// Marker style
    /// Draws background color for text
    /// </summary>
    public class MarkerStyle : Style
    {
        public Brush BackgroundBrush { get; }
        public Color Border { get; }

        public MarkerStyle(Brush backgroundBrush)
        {
            BackgroundBrush = backgroundBrush;
            IsExportable = true;
            SolidBrush b = backgroundBrush as SolidBrush;
            if (b != null)
                Border = Color.FromArgb(145, b.Color);
        }

        public void DrawBracketMarker(Graphics gr, Point position, Range range) {
            Rectangle rect = new Rectangle(position.X, position.Y+ range.tb.CharHeight - 1, (range.End.iChar - range.Start.iChar) * range.tb.CharWidth, 1);
            if (rect.Width == 0) return;
            gr.FillRectangle(new SolidBrush(Color.FromArgb(150, range.tb.ForeColor)), rect);
        }

        public void Draw(Graphics gr, Point position, Range range, bool withBorder) {
            Rectangle rect = new Rectangle(position.X, position.Y, (range.End.iChar - range.Start.iChar) * range.tb.CharWidth, range.tb.CharHeight);
            //draw background
            if (BackgroundBrush != null) {
                if (rect.Width == 0) return;
                gr.FillRectangle(BackgroundBrush, rect);
            }
            // draw border
            if (withBorder && Border != Color.Transparent && range.tb.SelectionWithBorders) {
                using (Pen pen = new Pen(Border)) {
                    GraphicsPath path = GetRoundedRect(rect);
                    gr.DrawPath(pen, path);
                }
            }
        }

        public override void Draw(Graphics gr, Point position, Range range) {
            Draw(gr, position, range, true);
        }

        public override string GetCSS()
        {
            string result = "";

            var brush = BackgroundBrush as SolidBrush;
            if (brush != null)
            {
                var s = ExportToHTML.GetColorAsString(brush.Color);
                if (s != "")
                    result += "background-color:" + s + ";";
            }

            return result;
        }

        private GraphicsPath GetRoundedRect(Rectangle baseRect) {
            int diameter = FastColoredTextBox.RoundedCornersRadius * 2;
            RectangleF arc = new Rectangle(baseRect.Location, new Size(diameter, diameter));
            GraphicsPath path = new GraphicsPath();
            path.AddArc(arc, 180, 90); // top left arc 
            arc.X = baseRect.Right  - diameter;
            path.AddArc(arc, 270, 90); // top right arc 
            arc.Y = baseRect.Bottom - diameter;
            path.AddArc(arc,   0, 90); // bottom right arc 
            arc.X = baseRect.Left;
            path.AddArc(arc,  90, 90); // bottom left arc
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Draws small rectangle for popup menu
    /// </summary>
    public class ShortcutStyle : Style
    {
        public Pen borderPen;

        public ShortcutStyle(Pen borderPen)
        {
            this.borderPen = borderPen;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //get last char coordinates
            Point p = range.tb.PlaceToPoint(range.End);
            //draw small square under char
            Rectangle rect = new Rectangle(p.X - 5, p.Y + range.tb.CharHeight - 2, 4, 3);
            gr.FillPath(Brushes.White, GetRoundedRectangle(rect, 1));
            gr.DrawPath(borderPen, GetRoundedRectangle(rect, 1));
            //add visual marker for handle mouse events
            AddVisualMarker(range.tb, new StyleVisualMarker(new Rectangle(p.X-range.tb.CharWidth, p.Y, range.tb.CharWidth, range.tb.CharHeight), this));
        }
    }

    /// <summary>
    /// This style draws a wavy line below a given text range.
    /// </summary>
    /// <remarks>Thanks for Yallie</remarks>
    public class WavyLineStyle : Style
    {
        private Pen Pen;

        public WavyLineStyle(int alpha, Color color)
        {
            Pen = new Pen(Color.FromArgb(alpha, color));
        }

        public override void Draw(Graphics gr, Point pos, Range range)
        {
            var size  = GetSizeOfRange(range);
            var start = new Point(pos.X, pos.Y + size.Height - 1);
            var end   = new Point(pos.X + size.Width, pos.Y + size.Height - 1);
            DrawWavyLine(gr, start, end);
        }

        private void DrawWavyLine(Graphics graphics, Point start, Point end)
        {
            if (end.X - start.X < 2)
            {
                graphics.DrawLine(Pen, start, end);
                return;
            }

            var offset = -1;
            var points = new List<Point>();

            for (int i = start.X; i <= end.X; i += 2)
            {
                points.Add(new Point(i, start.Y + offset));
                offset = -offset;
            }

            graphics.DrawLines(Pen, points.ToArray());
        }

        protected new virtual void Dispose(bool disposing)
        {
            if (disposing) {
                Pen.Dispose();
            }
        }
    }

    /// <summary>
    /// This style is used to mark range of text as ReadOnly block
    /// </summary>
    /// <remarks>You can inherite this style to add visual effects of readonly text</remarks>
    public class ReadOnlyStyle : Style
    {
        public ReadOnlyStyle()
        {
            IsExportable = false;
        }

        public override void Draw(Graphics gr, Point position, Range range)
        {
            //
        }
    }
}
