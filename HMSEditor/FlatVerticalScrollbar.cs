//#define debug
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FastColoredTextBoxNS {
    public partial class FlatVerticalScrollbar: Control {
        const int DefaultArrowHeght = 16;

        public Color ChannelColor    = Color.WhiteSmoke;
        public Color ThumbColor      = Color.DarkGray;
        public Color ThumbHoverColor = Color.Gray;
        public Color ArrowColor      = Color.DarkGray;
        public Color ArrowHoverColor = Color.CornflowerBlue;
        public int   ThumbHeight     = 0;
        public int   TrackHeight     = 0;

        protected Image moUpArrowImage    = null;
        protected Image moDownArrowImage  = null;
        protected int UpArrowHeight   = DefaultArrowHeght;
        protected int DownArrowHeight = DefaultArrowHeght;
        protected Cursor oldCursor = Cursors.Arrow;

        protected int moLargeChange = 10;
        protected int moSmallChange = 1;
        protected int moMinimum     = 0;
        protected int moMaximum     = 100;
        protected int moValue       = 0;
        private   int nClickPoint;

        protected int moThumbTop = 0;

        private bool moThumbDown     = false;
        private bool moThumbDragging = false;

        private bool _thumbIsHover     = false;
        private bool _arrowUpIsHover   = false;
        private bool _arrowDownIsHover = false;

        private bool ThumbIsHover {
            get { return _thumbIsHover; }
            set { if (_thumbIsHover != value) { _thumbIsHover = value; Invalidate(); }
            }
        }
        private bool ArrowUpIsHover {
            get { return _arrowUpIsHover; }
            set {
                if (_arrowUpIsHover != value) { _arrowUpIsHover = value; Invalidate(); }
            }
        }
        private bool ArrowDownIsHover {
            get { return _arrowDownIsHover; }
            set {
                if (_arrowDownIsHover != value) { _arrowDownIsHover = value; Invalidate(); }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
        public Image ArrowUpImage {
            get { return moUpArrowImage; }
            set { moUpArrowImage = value; UpArrowHeight = (value != null) ? value.Height : DefaultArrowHeght; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
        public Image ArrowDownImage {
            get { return moDownArrowImage; }
            set { moDownArrowImage = value; DownArrowHeight = (value != null) ? value.Width : DefaultArrowHeght; }
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
            MinimumSize = new Size(Width, DownArrowHeight + UpArrowHeight + MinThumbLength);
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

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(16), Category("Behavior"), Description("Minimum heigth of thumb")]
        public int MinThumbLength = 16;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("LargeChange")]
        public int LargeChange {
            get { return moLargeChange; }
            set {
                moLargeChange = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("SmallChange")]
        public int SmallChange {
            get { return moSmallChange; }
            set {
                moSmallChange = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Minimum")]
        public int Minimum {
            get { return moMinimum; }
            set {
                if (moMinimum != value) {
                    moMinimum = value;
                    Recalc();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Maximum")]
        public int Maximum {
            get { return moMaximum; }
            set {
                if (moMaximum != value) {
                    moMaximum = value;
                    Recalc();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Value")]
        public int Value {
            get { return moValue; }
            set {
                if (moValue == value) return;
                if (value < Minimum) {
                    moValue = Minimum;
                    repeatTimer.Stop();
                } else if (value > Maximum) {
                    moValue = Maximum;
                    repeatTimer.Stop();
                } else {
                    moValue = value;
                }
                int nPixelRange = TrackHeight - ThumbHeight;
                int nRealRange  = Maximum - Minimum;
                float fPerc = 0.0f;
                if (nRealRange != 0) {
                    fPerc = (float)moValue / (float)nRealRange;
                }
                float fTop = fPerc * nPixelRange;
                moThumbTop = (int)fTop;

                Invalidate();
                Application.DoEvents();
            }
        }

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            Recalc();
        }

        public void Recalc() {
            TrackHeight = (Height - (UpArrowHeight + DownArrowHeight));
            ThumbHeight = 0;
            if ((Maximum + Height) != 0) ThumbHeight = (int)((float)((float)Height / (float)(Maximum+ Height)) * (float)TrackHeight);
            ThumbHeight = Math.Min(TrackHeight   , ThumbHeight);
            ThumbHeight = Math.Max(MinThumbLength, ThumbHeight);
            float k = 0; if ((Maximum - Minimum) != 0) k = (float)Value / (float)(Maximum - Minimum);
            moThumbTop = (int)(((float)TrackHeight * k) - ((float)ThumbHeight / 2));
            moThumbTop = Math.Max(0, moThumbTop);
            moThumbTop = Math.Min(TrackHeight - ThumbHeight, moThumbTop);
            if (!Visible && ((ThumbHeight < TrackHeight) && (Maximum >  Minimum))) Visible = true;
            if ( Visible && ((ThumbHeight > TrackHeight) || (Maximum <= Minimum))) Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e) {
#if debug
            var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
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
            if (ArrowUpImage != null) {
                g.DrawImage(ArrowUpImage, new Rectangle(new Point(0, 0), new Size(this.Width, UpArrowHeight)));
            } else {
                Point[] points = new Point[] {
                    new Point(arrowsXPadding, arrowsYPadding + arrowsHeight),
                    new Point(Width - arrowsXPadding, arrowsYPadding + arrowsHeight),
                    new Point(Width / 2, arrowsYPadding)
                };
                using (Brush brush = new SolidBrush((ArrowUpIsHover) ? ArrowHoverColor : ArrowColor)) {
                    g.FillPolygon(brush, points);
                }
            }
            //draw thumb
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            int nTop = moThumbTop + UpArrowHeight;
            using (Brush oBrush = new SolidBrush((ThumbIsHover) ? ThumbHoverColor : ThumbColor)) {
                g.FillRectangle(oBrush, new Rectangle(1, nTop, Width - 2, ThumbHeight));
            }
            //draw down arrow
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            if (ArrowDownImage != null) {
                g.DrawImage(ArrowDownImage, new Rectangle(new Point(0, (Height - DownArrowHeight)), new Size(Width, DownArrowHeight)));
            } else {
                Point[] points = new Point[] {
                    new Point(arrowsXPadding, Height - arrowsYPadding - arrowsHeight),
                    new Point(Width - arrowsXPadding, Height - arrowsYPadding - arrowsHeight),
                    new Point(Width / 2, Height - arrowsYPadding)
                };
                using (Brush brush = new SolidBrush((ArrowDownIsHover) ? ArrowHoverColor : ArrowColor)) {
                    g.FillPolygon(brush, points);
                }
            }
#if debug
            sw.Stop();
            System.Console.WriteLine("FlatScrollBar OnPaint: " + sw.ElapsedMilliseconds);
#endif
        }

        public override bool AutoSize {
            get {
                return base.AutoSize;
            }
            set {
                base.AutoSize = value;
                if (base.AutoSize) {
                    if (moUpArrowImage != null)
                        this.Width = moUpArrowImage.Width;
                    else
                        this.Width = 16;
                }
            }
        }

        private void CustomScrollbar_MouseDown(object sender, MouseEventArgs e) {
            int   nTop    = moThumbTop + UpArrowHeight;
            Point ptPoint = this.PointToClient(Cursor.Position);

            Rectangle upArrowRect     = new Rectangle(new Point(1, 0), new Size(Width, UpArrowHeight));
            Rectangle beforeThumbRect = new Rectangle(new Point(1, UpArrowHeight + 1), new Size(Width, moThumbTop));
            Rectangle thumbRect       = new Rectangle(new Point(1, nTop), new Size(Width, ThumbHeight));
            Rectangle afterThumbRect  = new Rectangle(new Point(1, nTop + ThumbHeight), new Size(Width, Height - nTop - ThumbHeight - DownArrowHeight));
            Rectangle downArrowRect   = new Rectangle(new Point(1, DownArrowHeight + TrackHeight), new Size(Width, DownArrowHeight));

            int oldVal = Value;
            if (thumbRect.Contains(ptPoint)) {
                //hit the thumb
                nClickPoint = (ptPoint.Y - nTop);
                this.moThumbDown = true;

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
            this.moThumbDown     = false;
            this.moThumbDragging = false;
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
            if (moThumbDown && nRealRange > 0) {
                if (nPixelRange > 0) {
                    int nNewThumbTop = y - (UpArrowHeight + nSpot);

                    if (nNewThumbTop < 0) {
                        moThumbTop = nNewThumbTop = 0;
                    } else if (nNewThumbTop > nPixelRange) {
                        moThumbTop = nNewThumbTop = nPixelRange;
                    } else {
                        moThumbTop = y - (UpArrowHeight + nSpot);
                    }

                    //figure out value
                    float fPerc = (float)moThumbTop / (float)nPixelRange;
                    float fValue = fPerc * (Maximum);
                    moValue = (int)fValue;

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
            Rectangle thumbRect     = new Rectangle(new Point(1, moThumbTop + UpArrowHeight), new Size(Width, ThumbHeight));
            Rectangle arrowUpRect   = new Rectangle(new Point(1, 0), new Size(Width, UpArrowHeight));
            Rectangle arrowDownRect = new Rectangle(new Point(1, DownArrowHeight + TrackHeight), new Size(Width, DownArrowHeight));
            ThumbIsHover     = moThumbDown || thumbRect.Contains(ptPoint);
            ArrowUpIsHover   = arrowUpRect  .Contains(ptPoint);
            ArrowDownIsHover = arrowDownRect.Contains(ptPoint);
        }

        private void CustomScrollbar_MouseMove(object sender, MouseEventArgs e) {
            CheckHover();

            if (moThumbDown == true) {
                this.moThumbDragging = true;
            }

            if (this.moThumbDragging) {
                MoveThumb(e.Y);
                if (Scroll != null) Scroll(this, new EventArgs());
            }

        }

    }

}
