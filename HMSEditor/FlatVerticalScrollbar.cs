using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FastColoredTextBoxNS {
    public partial class FlatVerticalScrollbar: Control {

        public Color ChannelColor    = Color.WhiteSmoke;
        public Color ThumbColor      = Color.DarkGray;
        public Color ThumbHoverColor = Color.Gray;
        public Color ArrowColor      = Color.DarkGray;
        public Color ArrowHoverColor = Color.CornflowerBlue;
        public int   ThumbHeight     = 0;
        public int   TrackHeight     = 0;

        public int MinThumbLength  = 16;
        public int LargeChange     = 10;
        public int SmallChange     = 1;

        protected int ArrowAreaHeight = 16;
        protected Cursor oldCursor = Cursors.Arrow;

        protected int _minimum     = 0;
        protected int _maximum     = 100;
        protected int _value       = 0;
        private   int nClickPoint;

        protected int ThumbTop = 0;

        private bool ThumbIsDown     = false;
        private bool ThumbIsDragging = false;

        private bool _thumbIsHover     = false;
        private bool _arrowUpIsHover   = false;
        private bool _arrowDownIsHover = false;

        private bool ThumbIsHover     { get { return _thumbIsHover    ; } set { if (_thumbIsHover     != value) { _thumbIsHover     = value; Invalidate(); } } }
        private bool ArrowUpIsHover   { get { return _arrowUpIsHover  ; } set { if (_arrowUpIsHover   != value) { _arrowUpIsHover   = value; Invalidate(); } } }
        private bool ArrowDownIsHover { get { return _arrowDownIsHover; } set { if (_arrowDownIsHover != value) { _arrowDownIsHover = value; Invalidate(); } } }

        public int Minimum { get { return _minimum; } set { if (_minimum != value) { _minimum = value; Recalc(); } } }
        public int Maximum { get { return _maximum; } set { if (_maximum != value) { _maximum = value; Recalc(); } } }

        public int Value {
            get { return _value; }
            set {
                if (_value == value) return;
                if (value < Minimum) {
                    _value = Minimum;
                    repeatTimer.Stop();
                } else if (value > Maximum) {
                    _value = Maximum;
                    repeatTimer.Stop();
                } else {
                    _value = value;
                }
                int nPixelRange = TrackHeight - ThumbHeight;
                int nRealRange = Maximum - Minimum;
                float fPerc = 0.0f;
                if (nRealRange != 0) {
                    fPerc = (float)_value / (float)nRealRange;
                }
                float fTop = fPerc * nPixelRange;
                ThumbTop = (int)fTop;

                Invalidate();
                Application.DoEvents();
            }
        }

        public event EventHandler Scroll  = null;

        protected Timer repeatTimer;
        protected int FirstRepeatInterval = 600;
        protected int NextRepeatInterval  = 150;
        protected int NextRepeatCount     = 0;

        public FlatVerticalScrollbar() {
            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            Dock = DockStyle.Right;
            repeatTimer = new Timer();
            repeatTimer.Tag = 0;
            repeatTimer.Interval = FirstRepeatInterval;
            repeatTimer.Tick += RepeatTimer_Tick;
            TabStop = false;
            MinimumSize = new Size(Width, ArrowAreaHeight * 2 + MinThumbLength);
        }

        private void RepeatTimer_Tick(object sender, EventArgs e) {
            if      (repeatTimer.Interval == FirstRepeatInterval) NextRepeatCount = 0;
            else if (repeatTimer.Interval == NextRepeatInterval ) NextRepeatCount++;
            int delta  = (int)repeatTimer.Tag;
            int newVal = Value + delta;
            if (NextRepeatCount > 06) newVal += delta * 4; // increase speed
            if (NextRepeatCount > 12) newVal += delta * 16;
            if (NextRepeatCount > 18) newVal += delta * 64;
            if (NextRepeatCount > 24) newVal += delta * 128;
            if ((newVal <= Minimum) || (newVal >= Maximum)) {
                repeatTimer.Stop();
            }
            repeatTimer.Interval = NextRepeatInterval;
            Point ptPoint = this.PointToClient(Cursor.Position);
            int yval = Y2Value(ptPoint.Y);
            if (delta > 0) {
                if (newVal > yval) repeatTimer.Stop();
            } else {
                if (newVal < yval) repeatTimer.Stop();
            }
            if (Value != newVal) {
                Value = newVal;
                if (Scroll != null) Scroll(this, new EventArgs());
            }
        }

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            Recalc();
        }

        public void Recalc() {
            TrackHeight = (Height - (ArrowAreaHeight * 2));
            ThumbHeight = 0;
            if ((Maximum + Height) != 0) ThumbHeight = (int)((float)((float)Height / (float)(Maximum+ Height)) * (float)TrackHeight);
            ThumbHeight = Math.Min(TrackHeight   , ThumbHeight);
            ThumbHeight = Math.Max(MinThumbLength, ThumbHeight);
            float k = 0; if ((Maximum - Minimum) != 0) k = (float)Value / (float)(Maximum - Minimum);
            ThumbTop = (int)(((float)TrackHeight * k) - ((float)ThumbHeight / 2));
            ThumbTop = Math.Max(0, ThumbTop);
            ThumbTop = Math.Min(TrackHeight - ThumbHeight, ThumbTop);
            if (!Visible && ((ThumbHeight < TrackHeight) && (Maximum >  Minimum))) Visible = true;
            if ( Visible && ((ThumbHeight > TrackHeight) || (Maximum <= Minimum))) Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            int arrowsXPadding = 3;
            int arrowsYPadding = 4;
            int arrowsWidth    = Width - (arrowsXPadding * 2);
            int arrowsHeight   = arrowsWidth - (arrowsWidth / 3);

            //draw channel
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            using (Brush oBrush = new SolidBrush(ChannelColor)) {
                g.FillRectangle(oBrush, new Rectangle(0, 0, Width, Height));
            }
            //draw up arrow
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Point[] points1 = new Point[] {
                new Point(arrowsXPadding, arrowsYPadding + arrowsHeight),
                new Point(Width - arrowsXPadding, arrowsYPadding + arrowsHeight),
                new Point(Width / 2, arrowsYPadding)
            };
            using (Brush brush = new SolidBrush((ArrowUpIsHover) ? ArrowHoverColor : ArrowColor)) {
                g.FillPolygon(brush, points1);
            }
            //draw thumb
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            int nTop = ThumbTop + ArrowAreaHeight;
            using (Brush brush = new SolidBrush((ThumbIsHover) ? ThumbHoverColor : ThumbColor)) {
                g.FillRectangle(brush, new Rectangle(1, nTop, Width - 2, ThumbHeight));
            }
            //draw down arrow
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Point[] points2 = new Point[] {
                new Point(arrowsXPadding, Height - arrowsYPadding - arrowsHeight),
                new Point(Width - arrowsXPadding, Height - arrowsYPadding - arrowsHeight),
                new Point(Width / 2, Height - arrowsYPadding)
            };
            using (Brush brush = new SolidBrush((ArrowDownIsHover) ? ArrowHoverColor : ArrowColor)) {
                g.FillPolygon(brush, points2);
            }
        }

        private void CustomScrollbar_MouseDown(object sender, MouseEventArgs e) {
            int nTop    = ThumbTop + ArrowAreaHeight;
            Point ptPoint = this.PointToClient(Cursor.Position);

            Rectangle upArrowRect     = new Rectangle(new Point(1, 0), new Size(Width, ArrowAreaHeight));
            Rectangle beforeThumbRect = new Rectangle(new Point(1, ArrowAreaHeight + 1), new Size(Width, ThumbTop));
            Rectangle thumbRect       = new Rectangle(new Point(1, nTop), new Size(Width, ThumbHeight));
            Rectangle afterThumbRect  = new Rectangle(new Point(1, nTop + ThumbHeight), new Size(Width, Height - nTop - ThumbHeight - ArrowAreaHeight));
            Rectangle downArrowRect   = new Rectangle(new Point(1, ArrowAreaHeight + TrackHeight), new Size(Width, ArrowAreaHeight));

            int oldVal = Value;
            if (thumbRect.Contains(ptPoint)) {
                //hit the thumb
                nClickPoint = (ptPoint.Y - nTop);
                this.ThumbIsDown = true;

            } else if (beforeThumbRect.Contains(ptPoint)) {
                Value -= Height;
                repeatTimer.Tag = -Height;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();

            } else if (afterThumbRect.Contains(ptPoint)) {
                Value += Height;
                repeatTimer.Tag = Height;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();

            } else if (upArrowRect.Contains(ptPoint)) {
                Value -= SmallChange;
                repeatTimer.Tag = -SmallChange;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();

            } else if (downArrowRect.Contains(ptPoint)) {
                Value += SmallChange;
                repeatTimer.Tag = SmallChange;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();
            }
            if (oldVal != Value) {
                if (Scroll != null) Scroll(this, new EventArgs());
            }
        }

        private void CustomScrollbar_MouseUp(object sender, MouseEventArgs e) {
            this.ThumbIsDown     = false;
            this.ThumbIsDragging = false;
            repeatTimer.Stop();
        }

        protected override void OnMouseEnter(EventArgs e) {
            oldCursor = Cursor;
            Cursor = Cursors.Arrow;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            Cursor = oldCursor;
            ThumbIsHover = false;
            ArrowUpIsHover   = false;
            ArrowDownIsHover = false;
            repeatTimer.Stop();
            base.OnMouseLeave(e);
        }

        private void MoveThumb(int y) {
            int nRealRange   = Maximum - Minimum;
            int nSpot        = nClickPoint;

            int nPixelRange = (TrackHeight - ThumbHeight);
            if (ThumbIsDown && nRealRange > 0) {
                if (nPixelRange > 0) {
                    int nNewThumbTop = y - (ArrowAreaHeight + nSpot);

                    if (nNewThumbTop < 0) {
                        ThumbTop = nNewThumbTop = 0;
                    } else if (nNewThumbTop > nPixelRange) {
                        ThumbTop = nNewThumbTop = nPixelRange;
                    } else {
                        ThumbTop = y - (ArrowAreaHeight + nSpot);
                    }

                    //figure out value
                    float fPerc = (float)ThumbTop / (float)nPixelRange;
                    float fValue = fPerc * (Maximum);
                    _value = (int)fValue;

                    Application.DoEvents();
                    Invalidate();
                }
            }
        }

        private int Y2Value(int y) {
            y = Math.Max(0, Math.Min(Height, y));
            float k = (float)y / (float)Height;
            int val = (int)(((float)Maximum - (float)Minimum) * k) + Minimum;
            return val;
        }

        private int X2Value(int x) {
            x = Math.Max(0, Math.Min(Width, x));
            float k = (float)x / (float)Width;
            int val = (int)(((float)Maximum - (float)Minimum) * k) + Minimum;
            return val;
        }

        private void CheckHover() {
            Point ptPoint = this.PointToClient(Cursor.Position);
            Rectangle thumbRect     = new Rectangle(new Point(1, ThumbTop + ArrowAreaHeight), new Size(Width, ThumbHeight));
            Rectangle arrowUpRect   = new Rectangle(new Point(1, 0), new Size(Width, ArrowAreaHeight));
            Rectangle arrowDownRect = new Rectangle(new Point(1, ArrowAreaHeight + TrackHeight), new Size(Width, ArrowAreaHeight));
            ThumbIsHover     = ThumbIsDown || thumbRect.Contains(ptPoint);
            ArrowUpIsHover   = arrowUpRect  .Contains(ptPoint);
            ArrowDownIsHover = arrowDownRect.Contains(ptPoint);
        }

        private void CustomScrollbar_MouseMove(object sender, MouseEventArgs e) {
            CheckHover();

            if (ThumbIsDown == true) {
                this.ThumbIsDragging = true;
            }

            if (this.ThumbIsDragging) {
                MoveThumb(e.Y);
                if (Scroll != null) Scroll(this, new EventArgs());
            }

        }

    }

}
