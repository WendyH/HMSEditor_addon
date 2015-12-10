using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FastColoredTextBoxNS {
    public partial class FlatHorizontalScrollbar: Control {
        const int DefaultArrowWidth = 16;

        public Color ChannelColor    = Color.WhiteSmoke;
        public Color ThumbColor      = Color.DarkGray;
        public Color ThumbHoverColor = Color.Gray;
        public Color ArrowColor      = Color.DarkGray;
        public Color ArrowHoverColor = Color.CornflowerBlue;
        public int   ThumbWidth      = 0;
        public int   TrackWidth      = 0;

        public int MinThumbLength = 16;
        public int LargeChange = 10;
        public int SmallChange = 1;

        protected Image  moUpArrowImage   = null;
        protected Image  moDownArrowImage = null;
        protected int    LeftArrowWidth   = DefaultArrowWidth;
        protected int    RightArrowWidth  = DefaultArrowWidth;
        protected Cursor oldCursor        = Cursors.Arrow;

        protected int _minimum     = 0;
        protected int _maximum     = 100;
        protected int _value       = 0;
        private   int nClickPoint;

        protected int ThumbLeft = 0;

        private bool ThumbIsDown     = false;
        private bool ThumbIsDragging = false;

        private bool _thumbIsHover     = false;
        private bool _arrowUpIsHover   = false;
        private bool _arrowDownIsHover = false;
         
        private bool ThumbIsHover      { get { return _thumbIsHover    ; } set { if (_thumbIsHover     != value) { _thumbIsHover     = value; Invalidate(); } } }
        private bool ArrowLeftIsHover  { get { return _arrowUpIsHover  ; } set { if (_arrowUpIsHover   != value) { _arrowUpIsHover   = value; Invalidate(); } } }
        private bool ArrowRightIsHover { get { return _arrowDownIsHover; } set { if (_arrowDownIsHover != value) { _arrowDownIsHover = value; Invalidate(); } } }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
        public Image ArrowLeftImage {
            get { return moUpArrowImage; }
            set { moUpArrowImage = value; LeftArrowWidth = (value != null) ? value.Width : DefaultArrowWidth; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
        public Image ArrowRightImage {
            get { return moDownArrowImage; }
            set { moDownArrowImage = value; RightArrowWidth = (value != null) ? value.Width : DefaultArrowWidth; }
        }
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Minimum")]
        public int Minimum { get { return _minimum; } set { if (_minimum != value) { _minimum = value; Recalc(); } } }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Maximum")]
        public int Maximum { get { return _maximum; } set { if (_maximum != value) { _maximum = value; Recalc(); } } }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Value")]
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
                int nPixelRange = TrackWidth - ThumbWidth;
                int nRealRange = Maximum - Minimum;
                float fPerc = 0.0f;
                if (nRealRange != 0) {
                    fPerc = (float)_value / (float)nRealRange;
                }
                float fTop = fPerc * nPixelRange;
                ThumbLeft = (int)fTop;

                Invalidate();
                Application.DoEvents();
            }
        }

        public event EventHandler Scroll = null;

        protected Timer repeatTimer;
        protected int FirstRepeatInterval = 600;
        protected int NextRepeatInterval  = 150;
        protected int NextRepeatCount     = 0;

        public FlatHorizontalScrollbar() {
            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            Dock = DockStyle.Bottom;
            repeatTimer = new Timer();
            repeatTimer.Tag = 0;
            repeatTimer.Interval = FirstRepeatInterval;
            repeatTimer.Tick += RepeatTimer_Tick;
            TabStop = false;
            MinimumSize = new Size(LeftArrowWidth + RightArrowWidth + MinThumbLength, Height);
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
            int xval = X2Value(ptPoint.X);
            if (delta > 0) {
                if (newVal > xval) repeatTimer.Stop();
            } else {
                if (newVal < xval) repeatTimer.Stop();
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
            TrackWidth = (Width - (LeftArrowWidth + RightArrowWidth));
            ThumbWidth = 0;
            if ((Maximum + Width) != 0) ThumbWidth = (int)((float)((float)Width / (float)(Maximum+Width)) * (float)TrackWidth);
            ThumbWidth  = Math.Min(TrackWidth, ThumbWidth);
            ThumbWidth  = Math.Max(MinThumbLength, ThumbWidth);
            float k = 0; if ((Maximum - Minimum) != 0) k = (float)Value / (float)(Maximum - Minimum);
            ThumbLeft = (int)(((float)TrackWidth * k) - ((float)ThumbWidth / 2));
            ThumbLeft = Math.Max(0, ThumbLeft);
            ThumbLeft = Math.Min(TrackWidth - ThumbWidth, ThumbLeft);
            if (!Visible && ((ThumbWidth < TrackWidth) && (Maximum >  Minimum))) Visible = true;
            if ( Visible && ((ThumbWidth > TrackWidth) || (Maximum <= Minimum))) Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;
            int arrowsXPadding = 4;
            int arrowsYPadding = 3;
            int arrowsHeight = Height - (arrowsYPadding * 2); 
            int arrowsWidth  = arrowsHeight - (arrowsHeight / 3);

            //draw channel
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            using (Brush oBrush = new SolidBrush(ChannelColor)) {
                g.FillRectangle(oBrush, new Rectangle(0, 0, Width, Height));
            }
            //draw up arrow
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            if (ArrowLeftImage != null) {
                g.DrawImage(ArrowLeftImage, new Rectangle(new Point(0, 0), new Size(LeftArrowWidth, this.Height)));
            } else {
                Point[] points = new Point[] {
                    new Point(arrowsXPadding + arrowsWidth, arrowsYPadding),
                    new Point(arrowsXPadding + arrowsWidth, Height - arrowsYPadding),
                    new Point(arrowsXPadding, Height / 2)
                };
                using (Brush brush = new SolidBrush((ArrowLeftIsHover) ? ArrowHoverColor : ArrowColor)) {
                    g.FillPolygon(brush, points);
                }
            }
            //draw thumb
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            int nLeft = ThumbLeft + LeftArrowWidth;
            using (Brush oBrush = new SolidBrush((ThumbIsHover) ? ThumbHoverColor : ThumbColor)) {
                g.FillRectangle(oBrush, new Rectangle(nLeft, 2, ThumbWidth, Height - 3));
            }
            //draw down arrow
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            if (ArrowRightImage != null) {
                g.DrawImage(ArrowRightImage, new Rectangle(new Point(0, (Height - RightArrowWidth)), new Size(Width, RightArrowWidth)));
            } else {
                Point[] points = new Point[] {
                    new Point(Width - arrowsXPadding - arrowsWidth, arrowsYPadding),
                    new Point(Width - arrowsXPadding - arrowsWidth, Height - arrowsYPadding),
                    new Point(Width - arrowsXPadding, Height / 2)
                };
                using (Brush brush = new SolidBrush((ArrowRightIsHover) ? ArrowHoverColor : ArrowColor)) {
                    g.FillPolygon(brush, points);
                }
            }
        }

        public override bool AutoSize {
            get {
                return base.AutoSize;
            }
            set {
                base.AutoSize = value;
                if (base.AutoSize) {
                    if (moUpArrowImage != null)
                        this.Height = moUpArrowImage.Height;
                    else
                        this.Height = 16;
                }
            }
        }

        private void CustomScrollbar_MouseDown(object sender, MouseEventArgs e) {
            int   nLeft   = ThumbLeft + LeftArrowWidth;
            Point ptPoint = this.PointToClient(Cursor.Position);

            Rectangle leftArrowRect   = new Rectangle(new Point(0, 1), new Size(LeftArrowWidth, Height));
            Rectangle beforeThumbRect = new Rectangle(new Point(LeftArrowWidth + 1, 1), new Size(ThumbLeft, Height));
            Rectangle thumbRect       = new Rectangle(new Point(nLeft, 1), new Size(ThumbWidth, Height));
            Rectangle afterThumbRect  = new Rectangle(new Point(nLeft + ThumbWidth, 1), new Size(Width - nLeft - ThumbWidth - RightArrowWidth, Height));
            Rectangle rightArrowRect  = new Rectangle(new Point(RightArrowWidth + TrackWidth, 1), new Size(RightArrowWidth, Height));

            int oldVal = Value;
            if (thumbRect.Contains(ptPoint)) {
                //hit the thumb
                nClickPoint = (ptPoint.X - nLeft);
                this.ThumbIsDown = true;

            } else if (beforeThumbRect.Contains(ptPoint)) {
                Value -= Width;
                repeatTimer.Tag = -Width;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();

            } else if (afterThumbRect.Contains(ptPoint)) {
                Value += Width; Invalidate();
                repeatTimer.Tag = Width;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();

            } else if (leftArrowRect.Contains(ptPoint)) {
                Value -= SmallChange; Invalidate();
                repeatTimer.Tag = -SmallChange;
                repeatTimer.Interval = FirstRepeatInterval;
                repeatTimer.Start();

            } else if (rightArrowRect.Contains(ptPoint)) {
                Value += SmallChange; Invalidate();
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
            ThumbIsHover      = false;
            ArrowLeftIsHover  = false;
            ArrowRightIsHover = false;
            repeatTimer.Stop();
            base.OnMouseLeave(e);
        }

        private void MoveThumb(int x) {
            int nRealRange   = Maximum - Minimum;
            int nSpot        = nClickPoint;

            int nPixelRange = (TrackWidth - ThumbWidth);
            if (ThumbIsDown && nRealRange > 0) {
                if (nPixelRange > 0) {
                    int nNewThumbLeft = x - (LeftArrowWidth + nSpot);

                    if (nNewThumbLeft < 0) {
                        ThumbLeft = nNewThumbLeft = 0;
                    } else if (nNewThumbLeft > nPixelRange) {
                        ThumbLeft = nNewThumbLeft = nPixelRange;
                    } else {
                        ThumbLeft = x - (LeftArrowWidth + nSpot);
                    }

                    //figure out value
                    float fPerc = (float)ThumbLeft / (float)nPixelRange;
                    float fValue = fPerc * (Maximum);
                    _value = (int)fValue;

                    Application.DoEvents();

                    Invalidate();
                }
            }
        }

        private int X2Value(int x) {
            x = Math.Max(0, Math.Min(Width, x));
            float k = (float)x / (float)Width;
            int val = (int)(((float)Maximum - (float)Minimum) * k) + Minimum;
            return val;
        }

        private void CheckHover() {
            Point ptPoint = this.PointToClient(Cursor.Position);
            Rectangle leftArrowRect  = new Rectangle(new Point(0, 1), new Size(LeftArrowWidth, Height));
            Rectangle thumbRect      = new Rectangle(new Point(ThumbLeft, 1), new Size(ThumbWidth, Height));
            Rectangle rightArrowRect = new Rectangle(new Point(RightArrowWidth + TrackWidth, 1), new Size(RightArrowWidth, Height));
            ThumbIsHover = ThumbIsDown || thumbRect.Contains(ptPoint);
            ArrowLeftIsHover  = leftArrowRect .Contains(ptPoint);
            ArrowRightIsHover = rightArrowRect.Contains(ptPoint);
        }

        private void CustomScrollbar_MouseMove(object sender, MouseEventArgs e) {
            CheckHover();

            if (ThumbIsDown == true) {
                this.ThumbIsDragging = true;
            }

            if (this.ThumbIsDragging) {
                MoveThumb(e.X);
                if (Scroll != null) Scroll(this, new EventArgs());
            }

        }

    }

}
