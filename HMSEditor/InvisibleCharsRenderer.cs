﻿using System.Drawing;

namespace FastColoredTextBoxNS {
    public class InvisibleCharsRenderer: Style {
        Pen pen;

        public InvisibleCharsRenderer(Pen pen) {
            this.pen = pen;
        }

        public override void Draw(Graphics gr, Point position, Range range) {
            Draw(gr, position, range, false);
        }

        public void Draw(Graphics gr, Point position, Range range, bool withoutEnd) {
            var tb = range.tb;
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            using (Brush brush = new SolidBrush(pen.Color))
                foreach (var place in range) {
                    switch (tb[place].c) {
                        case ' ':
                            var point = tb.PlaceToPoint(place);
                            point.Offset(tb.CharWidth / 2, tb.CharHeight / 2);
                            gr.DrawLine(pen, point.X, point.Y + 1, point.X + 2, point.Y + 1);
                            break;
                    }
                    if (!withoutEnd && tb.Font.Size > 2) {
                        if (tb[place.iLine].Count - 1 == place.iChar) {
                            var point = tb.PlaceToPoint(place);
                            point.Offset(tb.CharWidth - 2, 1);
                            gr.DrawString("¶", new Font("Arial", tb.Font.Size - 2), brush, point);
                        }
                    }
                }
        }
    }
}
