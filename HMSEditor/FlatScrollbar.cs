using System;
using System.Drawing;
using System.Windows.Forms;

namespace FastColoredTextBoxNS {
    public partial class FlatScrollbar: Control {

        public bool  AlignByLines    = false;
        public Color ChannelColor    = Color.WhiteSmoke;
        public Color ThumbColor      = Color.DarkGray;
        public Color ThumbHoverColor = Color.Gray;
        public Color ArrowColor      = Color.DarkGray;
        public Color ArrowHoverColor = Color.CornflowerBlue;
        public int   ThumbSize     = 0;
        public int   TrackSize     = 0;

        public int MinThumbLength  = 16;
        public int LargeChange     = 10;
        public int SmallChange     = 1;
        public int ArrowAreaSize   = 16;

        private bool _isHorizontal = false;
        public bool IsHorizontal {
            get { return _isHorizontal; }
            set {
                _isHorizontal = value;
                if (_isHorizontal) {
                    Dock   = DockStyle.Bottom;
                    Height = ArrowAreaSize;
                    MinimumSize = new Size(ArrowAreaSize * 2 + MinThumbLength, Height);
                } else {
                    Dock   = DockStyle.Right;
                    Width  = ArrowAreaSize;
                    MinimumSize = new Size(Width, ArrowAreaSize * 2 + MinThumbLength);
                }
            }
        }

        protected Cursor oldCursor    = Cursors.Arrow;
        protected Timer  repeatTimer;

        protected int _minimum     = 0;
        protected int _maximum     = 100;
        protected int _value       = 0;
        private   int ClickPoint;

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

        public event EventHandler ValueChanged = null;

        public int Value {
            get { return _value; }
            set {
                if (AlignByLines)
                    value = (int)(Math.Ceiling(1d * value / SmallChange) * SmallChange);
                if (_value == value) return;
                _value = Math.Max(Minimum, Math.Min(Maximum, value));
                if ((_value == Minimum) || (_value == Minimum)) repeatTimer.Stop();

                int realRange = Maximum - Minimum;
                if (realRange > 0)
                    ThumbTop = (int)((float)_value / (float)realRange * (float)(TrackSize - ThumbSize));
                else
                    ThumbTop = 0;

                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
                Invalidate();
                Application.DoEvents();
            }
        }

        protected int FirstRepeatInterval = 600;
        protected int NextRepeatInterval  = 150;
        protected int NextRepeatCount     = 0;

        public FlatScrollbar(bool isHorisontal=false) {
            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            IsHorizontal = isHorisontal;
            repeatTimer  = new Timer();
            repeatTimer.Tag = 0;
            repeatTimer.Interval = FirstRepeatInterval;
            repeatTimer.Tick += RepeatTimer_Tick;
            TabStop = false;
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
            Point point = this.PointToClient(Cursor.Position);
            int pointVal;
            if (IsHorizontal)
                pointVal = X2Value(point.X);
            else
                pointVal = Y2Value(point.Y);
            if (delta > 0) {
                if (newVal > pointVal) repeatTimer.Stop();
            } else {
                if (newVal < pointVal) repeatTimer.Stop();
            }
            Value = newVal;
        }

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            Recalc();
        }

        public void Recalc() {
            int len = IsHorizontal ? Width : Height;
            TrackSize = len - (ArrowAreaSize * 2);
            ThumbSize = 0;
            if ((Maximum + len) != 0) ThumbSize = (int)((float)((float)len / (float)(Maximum+ len)) * (float)TrackSize);
            ThumbSize = Math.Max(MinThumbLength, Math.Min(TrackSize, ThumbSize));
            float k = 0; if ((Maximum - Minimum) != 0) k = (float)Value / (float)(Maximum - Minimum);
            ThumbTop = (int)(((float)TrackSize * k) - ((float)ThumbSize / 2));
            ThumbTop = Math.Max(0, ThumbTop);
            ThumbTop = Math.Min(TrackSize - ThumbSize, ThumbTop);
            if (!Visible && ((ThumbSize < TrackSize) && (Maximum >  Minimum))) Visible = true;
            if ( Visible && ((ThumbSize > TrackSize) || (Maximum <= Minimum))) Visible = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            int arrowsXPadding = 3;
            int arrowsYPadding = 4;
            int arrowsWidth    = Width - (arrowsXPadding * 2);
            int arrowsHeight   = arrowsWidth - (arrowsWidth / 3);
            if (IsHorizontal) {
                arrowsXPadding = 4;
                arrowsYPadding = 3;
                arrowsHeight   = Height - (arrowsYPadding * 2);
                arrowsWidth    = arrowsHeight - (arrowsHeight / 3);
            }

            //draw channel
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            using (Brush oBrush = new SolidBrush(ChannelColor)) {
                g.FillRectangle(oBrush, new Rectangle(0, 0, Width, Height));
            }
            //draw up arrow
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Point[] points1;
            if (IsHorizontal) {
                points1 = new Point[] {
                    new Point(arrowsXPadding + arrowsWidth, arrowsYPadding),
                    new Point(arrowsXPadding + arrowsWidth, Height - arrowsYPadding),
                    new Point(arrowsXPadding, Height / 2)
                };
            } else {
                points1 = new Point[] {
                    new Point(arrowsXPadding, arrowsYPadding + arrowsHeight),
                    new Point(Width - arrowsXPadding, arrowsYPadding + arrowsHeight),
                    new Point(Width / 2, arrowsYPadding)
                };
            }
            using (Brush brush = new SolidBrush((ArrowUpIsHover) ? ArrowHoverColor : ArrowColor)) {
                g.FillPolygon(brush, points1);
            }
            //draw thumb
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            int nTop = ThumbTop + ArrowAreaSize;
            if (IsHorizontal) {
                using (Brush oBrush = new SolidBrush((ThumbIsHover) ? ThumbHoverColor : ThumbColor)) {
                    g.FillRectangle(oBrush, new Rectangle(nTop, 2, ThumbSize, Height - 3));
                }
            } else {
                using (Brush brush = new SolidBrush((ThumbIsHover) ? ThumbHoverColor : ThumbColor)) {
                    g.FillRectangle(brush, new Rectangle(1, nTop, Width - 2, ThumbSize));
                }
            }
            //draw down arrow
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Point[] points2;
            if (IsHorizontal) {
                points2 = new Point[] {
                    new Point(Width - arrowsXPadding - arrowsWidth, arrowsYPadding),
                    new Point(Width - arrowsXPadding - arrowsWidth, Height - arrowsYPadding),
                    new Point(Width - arrowsXPadding, Height / 2)
                };
            } else {
                points2 = new Point[] {
                    new Point(arrowsXPadding, Height - arrowsYPadding - arrowsHeight),
                    new Point(Width - arrowsXPadding, Height - arrowsYPadding - arrowsHeight),
                    new Point(Width / 2, Height - arrowsYPadding)
                };
            }
            using (Brush brush = new SolidBrush((ArrowDownIsHover) ? ArrowHoverColor : ArrowColor)) {
                g.FillPolygon(brush, points2);
            }
        }

        private void CustomScrollbar_MouseDown(object sender, MouseEventArgs e) {
            int   top   = ThumbTop + ArrowAreaSize;
            Point point = this.PointToClient(Cursor.Position);
            Rectangle upArrowRect;
            Rectangle beforeThumbRect;
            Rectangle thumbRect;
            Rectangle afterThumbRect;
            Rectangle downArrowRect;

            if (IsHorizontal) {
                upArrowRect     = new Rectangle(new Point(0, 1), new Size(ArrowAreaSize, Height));
                beforeThumbRect = new Rectangle(new Point(ArrowAreaSize + 1, 1), new Size(ThumbTop, Height));
                thumbRect       = new Rectangle(new Point(top, 1), new Size(ThumbSize, Height));
                afterThumbRect  = new Rectangle(new Point(top + ThumbSize, 1), new Size(Width - top - ThumbSize - ArrowAreaSize, Height));
                downArrowRect   = new Rectangle(new Point(ArrowAreaSize + TrackSize, 1), new Size(ArrowAreaSize, Height));
            } else {
                upArrowRect     = new Rectangle(new Point(1, 0), new Size(Width, ArrowAreaSize));
                beforeThumbRect = new Rectangle(new Point(1, ArrowAreaSize + 1), new Size(Width, ThumbTop));
                thumbRect       = new Rectangle(new Point(1, top), new Size(Width, ThumbSize));
                afterThumbRect  = new Rectangle(new Point(1, top + ThumbSize), new Size(Width, Height - top - ThumbSize - ArrowAreaSize));
                downArrowRect   = new Rectangle(new Point(1, ArrowAreaSize + TrackSize), new Size(Width, ArrowAreaSize));
            }

            int oldVal = Value;
            if (thumbRect.Contains(point)) {
                //hit the thumb
                ClickPoint = ((IsHorizontal ? point.X : point.Y) - ThumbTop);
                this.ThumbIsDown = true;

            } else if (beforeThumbRect.Contains(point)) {
                Value -= Height;
                repeatTimer.Tag = -Height;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();

            } else if (afterThumbRect.Contains(point)) {
                Value += Height;
                repeatTimer.Tag = Height;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();

            } else if (upArrowRect.Contains(point)) {
                Value -= SmallChange;
                repeatTimer.Tag = -SmallChange;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();

            } else if (downArrowRect.Contains(point)) {
                Value += SmallChange;
                repeatTimer.Tag = SmallChange;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();
            }
        }

        private void CustomScrollbar_MouseUp(object sender, MouseEventArgs e) {
            this.ThumbIsDown     = false;
            this.ThumbIsDragging = false;
            repeatTimer.Stop();
        }

        protected override void OnMouseEnter(EventArgs e) {
            oldCursor = Cursor;
            Cursor    = Cursors.Arrow;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e) {
            Cursor           = oldCursor;
            ThumbIsHover     = false;
            ArrowUpIsHover   = false;
            ArrowDownIsHover = false;
            repeatTimer.Stop();
            base.OnMouseLeave(e);
        }

        private void MoveThumb(int y) {
            int realRange  = Maximum - Minimum;
            int allowRange = TrackSize - ThumbSize;

            if (ThumbIsDown && (allowRange > 0) && (realRange > 0)) {
                ThumbTop = y - ClickPoint;
                ThumbTop = Math.Max(0, Math.Min(allowRange, ThumbTop));
                _value   = (int)((float)ThumbTop / (float)allowRange * Maximum);
                if (AlignByLines)
                    _value = (int)(Math.Ceiling(1d * _value / SmallChange) * SmallChange);
                _value = Math.Max(Minimum, Math.Min(Maximum, _value));

                Invalidate();
                Application.DoEvents();
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
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
            Point point = this.PointToClient(Cursor.Position);
            Rectangle thumbRect;
            Rectangle arrowUpRect;
            Rectangle arrowDownRect;

            if (IsHorizontal) {
                arrowUpRect   = new Rectangle(new Point(0, 1), new Size(ArrowAreaSize, Height));
                thumbRect     = new Rectangle(new Point(ThumbTop, 1), new Size(ThumbSize, Height));
                arrowDownRect = new Rectangle(new Point(ArrowAreaSize + TrackSize, 1), new Size(ArrowAreaSize, Height));
            } else {
                arrowUpRect   = new Rectangle(new Point(1, 0), new Size(Width, ArrowAreaSize));
                thumbRect     = new Rectangle(new Point(1, ThumbTop + ArrowAreaSize), new Size(Width, ThumbSize));
                arrowDownRect = new Rectangle(new Point(1, ArrowAreaSize + TrackSize), new Size(Width, ArrowAreaSize));
            }

            ThumbIsHover     = ThumbIsDown || thumbRect.Contains(point);
            ArrowUpIsHover   = arrowUpRect  .Contains(point);
            ArrowDownIsHover = arrowDownRect.Contains(point);
        }

        private void CustomScrollbar_MouseMove(object sender, MouseEventArgs e) {
            CheckHover();

            if (ThumbIsDown == true) {
                this.ThumbIsDragging = true;
            }

            if (this.ThumbIsDragging) {
                if (IsHorizontal)
                    MoveThumb(e.X);
                else
                    MoveThumb(e.Y);
            }

        }

    }

}
