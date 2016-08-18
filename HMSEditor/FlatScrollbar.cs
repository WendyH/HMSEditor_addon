using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace FastColoredTextBoxNS {
    public partial class FlatScrollbar: Control {

        public List<int> FoundLines  = new List<int>();
        public List<int> Bookmarks   = new List<int>();
        public List<int> Breakpoints = new List<int>();
        public int       ErrorLine   = 0;
        public int       CurrentLine = 0;

        public bool  ShowIfVisible   = true;
        public bool  NoEvent4Value   = false;
        public bool  ShowChanedLines = false;
        public bool  AlignByLines    = false;
        public Color ChannelColor    = Color.WhiteSmoke;
        public Color ThumbColor      = Color.DarkGray;
        public Color ThumbHoverColor = Color.Gray;
        public Color ArrowColor      = Color.DarkGray;
        public Color ArrowHoverColor = Color.CornflowerBlue;
        public Color FoundColor      = HMSEditorNS.Themes.ToColor("#E5C63BFF");
        public Color ChangedColor    = Color.FromArgb(108, 226, 108);
        public int   ThumbSize;
        public int   TrackSize;

        public int MinThumbLength  = 16;
        public int LargeChange     = 10;
        public int SmallChange     = 1;
        public int ArrowAreaSize   = 16;

        private FastColoredTextBox tb;

        private bool _isHorizontal;
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

        protected int _minimum;
        protected int _maximum     = 100;
        protected int _value;
        private   int ClickPoint;

        protected int ThumbTop;

        private bool ThumbIsDown;

        private bool _thumbIsHover;
        private bool _arrowUpIsHover;
        private bool _arrowDownIsHover;

        private bool ThumbIsHover     { get { return _thumbIsHover    ; } set { if (_thumbIsHover     != value) { _thumbIsHover     = value; Invalidate(); } } }
        private bool ArrowUpIsHover   { get { return _arrowUpIsHover  ; } set { if (_arrowUpIsHover   != value) { _arrowUpIsHover   = value; Invalidate(); } } }
        private bool ArrowDownIsHover { get { return _arrowDownIsHover; } set { if (_arrowDownIsHover != value) { _arrowDownIsHover = value; Invalidate(); } } }

        public int Minimum { get { return _minimum; } set { if (_minimum != value) { _minimum = value; Recalc(); } } }
        public int Maximum { get { return _maximum; } set { if (_maximum != value) { _maximum = value; Recalc(); } } }

        public event EventHandler ValueChanged;

        public void SetValue(int val) {
            _value = Math.Max(Minimum, Math.Min(Maximum, val));
            int realRange = Maximum - Minimum;
            if (realRange > 0)
                ThumbTop = (int)(_value / (float)realRange * (TrackSize - ThumbSize));
            else
                ThumbTop = 0;
            Invalidate();
        }

        public int Value {
            get { return _value; }
            set {
                if (AlignByLines)
                    value = (int)(Math.Ceiling(1d * value / SmallChange) * SmallChange);
                if (_value == value) return;
                _value = Math.Max(Minimum, Math.Min(Maximum, value));
                if ((_value == Minimum) || (_value == Maximum)) repeatTimer.Stop();

                int realRange = Maximum - Minimum;
                if (realRange > 0)
                    ThumbTop = (int)(_value / (float)realRange * (TrackSize - ThumbSize));
                else
                    ThumbTop = 0;
                Application.DoEvents();
                if (!NoEvent4Value)
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        protected int FirstRepeatInterval = 600;
        protected int NextRepeatInterval  = 150;
        protected int NextRepeatCount;

        public FlatScrollbar(FastColoredTextBox fctb): this(false) {
            this.tb = fctb;
        }

        public FlatScrollbar(bool isHorisontal=false) {
            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            IsHorizontal = isHorisontal;
            repeatTimer = new Timer {
                Tag      = 0,
                Interval = FirstRepeatInterval
            };
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
            Point point = PointToClient(Cursor.Position);
            var pointVal = IsHorizontal ? X2Value(point.X) : Y2Value(point.Y);
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
            TrackSize = len - ArrowAreaSize * 2;
            ThumbSize = 0;
            if (Maximum + len != 0) ThumbSize = (int)(len / (float)(Maximum+ len) * TrackSize);
            ThumbSize = Math.Max(MinThumbLength, Math.Min(TrackSize, ThumbSize));
            float k = 0; if (Maximum - Minimum != 0) k = Value / (float)(Maximum - Minimum);
            ThumbTop = (int)(TrackSize * k - ((float)ThumbSize / 2));
            ThumbTop = Math.Max(0, ThumbTop);
            ThumbTop = Math.Min(TrackSize - ThumbSize, ThumbTop);
            if (!Visible && ((ThumbSize < TrackSize) && (Maximum >  Minimum))) Visible = ShowIfVisible;
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
                points1 = new[] {
                    new Point(arrowsXPadding + arrowsWidth, arrowsYPadding),
                    new Point(arrowsXPadding + arrowsWidth, Height - arrowsYPadding),
                    new Point(arrowsXPadding, Height / 2)
                };
            } else {
                points1 = new[] {
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
                points2 = new[] {
                    new Point(Width - arrowsXPadding - arrowsWidth, arrowsYPadding),
                    new Point(Width - arrowsXPadding - arrowsWidth, Height - arrowsYPadding),
                    new Point(Width - arrowsXPadding, Height / 2)
                };
            } else {
                points2 = new[] {
                    new Point(arrowsXPadding, Height - arrowsYPadding - arrowsHeight),
                    new Point(Width - arrowsXPadding, Height - arrowsYPadding - arrowsHeight),
                    new Point(Width / 2, Height - arrowsYPadding)
                };
            }
            using (Brush brush = new SolidBrush((ArrowDownIsHover) ? ArrowHoverColor : ArrowColor)) {
                g.FillPolygon(brush, points2);
            }

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            if (CurrentLine > 0) DrawRectByLine(g, CurrentLine, Color.CornflowerBlue, 0, Width, 3);
            if (ErrorLine   > 0) DrawRectByLine(g, ErrorLine  , Color.OrangeRed     , 9, 5, 5);
            foreach (int iLine in Bookmarks  ) DrawRectByLine(g, iLine, Color.LightSkyBlue, 4, 7, 4);
            foreach (int iLine in Breakpoints) DrawRectByLine(g, iLine, Color.IndianRed, 0, 7, 4);
            foreach (int iLine in FoundLines ) DrawRectByLine(g, iLine, FoundColor     , 2, 12, 3);
            if (ShowChanedLines && tb != null) {
                int count = tb.LinesCount;
                for (int i = 0; i < count; i++)
                    if (tb[i].IsChanged) DrawRectByLine(g, i + 1, ChangedColor, 1, 4, 3);
            }
        }

        private void DrawRectByLine(Graphics g, int iLine, Color color, int x, int width, int height) {
            float k = (float)(iLine-0.5) * SmallChange / (Maximum - Minimum + Height);
            int top = (int)(TrackSize * k);
            using (Brush brush = new SolidBrush(color)) {
                g.FillRectangle(brush, new Rectangle(x, top + ArrowAreaSize - (height/2), width, height));
            }
        }

        private void CustomScrollbar_MouseDown(object sender, MouseEventArgs e) {
            int   top   = ThumbTop + ArrowAreaSize;
            Point point = PointToClient(Cursor.Position);
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

            if (thumbRect.Contains(point)) {
                //hit the thumb
                ClickPoint = ((IsHorizontal ? point.X : point.Y) - ThumbTop);
                ThumbIsDown = true;

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
            ThumbIsDown = false;
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

        public void SetThumbY(int y) {
            int realRange = Maximum - Minimum;
            int allowRange = TrackSize - ThumbSize;

            if ((allowRange > 0) && (realRange > 0)) {
                ThumbTop = y - ClickPoint;
                ThumbTop = Math.Max(0, Math.Min(allowRange, ThumbTop));
                _value = (int)(ThumbTop / (float)allowRange * Maximum);
                if (AlignByLines)
                    _value = (int)(Math.Ceiling(1d * _value / SmallChange) * SmallChange);
                _value = Math.Max(Minimum, Math.Min(Maximum, _value));

                Invalidate();
                Application.DoEvents();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetThumbIsDown(int x, bool isSet) {
            if (isSet) {
                ClickPoint = x - ThumbTop;
            }
            ThumbIsDown = isSet;
        }

        public void MoveThumb(int y) {
            int realRange  = Maximum - Minimum;
            int allowRange = TrackSize - ThumbSize;

            if (ThumbIsDown && (allowRange > 0) && (realRange > 0)) {
                ThumbTop = y - ClickPoint;
                ThumbTop = Math.Max(0, Math.Min(allowRange, ThumbTop));
                _value   = (int)(ThumbTop / (float)allowRange * Maximum);
                if (AlignByLines)
                    _value = (int)(Math.Ceiling(1d * _value / SmallChange) * SmallChange);
                _value = Math.Max(Minimum, Math.Min(Maximum, _value));

                Invalidate();
                Application.DoEvents();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SetValueByKoef(double k) {
            Value  = (int)Math.Round((Maximum - Minimum) * k) + Minimum;
        }

        public int Y2Value(int y) {
            y = Math.Max(0, Math.Min(Height, y));
            float k = y / (float)Height;
            int val = (int)((Maximum - (float)Minimum) * k) + Minimum;
            return val;
        }

        private int X2Value(int x) {
            x = Math.Max(0, Math.Min(Width, x));
            float k = x / (float)Width;
            int val = (int)((Maximum - (float)Minimum) * k) + Minimum;
            return val;
        }

        private void CheckHover() {
            Point point = PointToClient(Cursor.Position);
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
            if (!ThumbIsDown) return;
            MoveThumb(IsHorizontal ? e.X : e.Y);
        }

    }

}
