using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FastColoredTextBoxNS {
    public partial class FlatScrollbar: UserControl {
        public Color ChannelColor    = Color.WhiteSmoke;
        public Color ThumbColor      = Color.DarkGray;
        public Color ThumbHoverColor = Color.Gray;
        public int ThumbHeight = 0;
        public int TrackHeight = 0;
        
        protected Image moUpArrowImage    = null;
        protected Image moDownArrowImage  = null;
        protected int UpArrowHeight   = 0;
        protected int DownArrowHeight = 0;

        protected int moLargeChange = 10;
        protected int moSmallChange = 1;
        protected int moMinimum     = 0;
        protected int moMaximum     = 100;
        protected int moValue       = 0;
        private   int nClickPoint;

        protected int moThumbTop = 0;

        private bool moThumbDown     = false;
        private bool moThumbDragging = false;

        private bool _thumbIsHover = false;
        private bool ThumbIsHover {
            get {
                return _thumbIsHover;
            }
            set {
                if (_thumbIsHover != value) {
                    _thumbIsHover = value;
                    Invalidate();
                }
            }
        }

        public new event EventHandler Scroll = null;

        protected Timer repeatTimer;
        protected int FirstRepeatInterval = 600;
        protected int NextRepeatInterval  = 150;
        protected int FastRepeatInterval  =  50;
        protected int NextRepeatCount = 0;

        public FlatScrollbar() {
            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            Dock = DockStyle.Right;
            Recalc();
            base.MinimumSize = new Size(Width, ThumbHeight);
            repeatTimer = new Timer();
            repeatTimer.Tag = 0;
            repeatTimer.Interval = FirstRepeatInterval;
            repeatTimer.Tick += RepeatTimer_Tick;
        }

        private void RepeatTimer_Tick(object sender, EventArgs e) {
            if      (repeatTimer.Interval == FirstRepeatInterval) NextRepeatCount = 0;
            else if (repeatTimer.Interval == NextRepeatInterval ) NextRepeatCount++;
            int newVal = Value + (int)repeatTimer.Tag;
            if (NextRepeatCount > 15) newVal += (int)repeatTimer.Tag; // increase speed
            if (NextRepeatCount > 22) newVal += (int)repeatTimer.Tag * 2;
            Value = newVal;
            if ((newVal <= Minimum) || (newVal >= Maximum)) {
                repeatTimer.Stop();
                return;
            }
            //repeatTimer.Interval = (NextRepeatCount < 15) ? NextRepeatInterval : FastRepeatInterval;
            repeatTimer.Interval = NextRepeatInterval;
        }

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            Recalc();
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(56), Category("Behavior"), Description("Minimum height of thumb")]
        public int MinThumbHeight = 56;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("LargeChange")]
        public int LargeChange {
            get { return moLargeChange; }
            set {
                moLargeChange = value;
                Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("SmallChange")]
        public int SmallChange {
            get { return moSmallChange; }
            set {
                moSmallChange = value;
                Invalidate();
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

                if (Scroll != null)
                    Scroll(this, new EventArgs());

                Invalidate();
                Application.DoEvents();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
        public Image UpArrowImage {
            get { return moUpArrowImage; }
            set { moUpArrowImage = value; UpArrowHeight = (value != null) ? value.Height : 0; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
        public Image DownArrowImage {
            get { return moDownArrowImage; }
            set { moDownArrowImage = value; DownArrowHeight = (value != null) ? value.Height : 0; }
        }

        public void Recalc() {
            TrackHeight = (Height - (UpArrowHeight + DownArrowHeight));
            ThumbHeight = 0;
            if ((Maximum + Height) != 0) ThumbHeight = (int)((float)((float)Height / (float)(Maximum+Height)) * (float)TrackHeight);
            ThumbHeight = Math.Min(TrackHeight   , ThumbHeight);
            ThumbHeight = Math.Max(MinThumbHeight, ThumbHeight);
            if (!Visible && ((ThumbHeight < TrackHeight) && (Maximum >  Minimum))) Visible = true;
            if (Visible && ((ThumbHeight == TrackHeight) || (Maximum <= Minimum))) Visible = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            //draw channel
            using (Brush oBrush = new SolidBrush(ChannelColor)) {
                e.Graphics.FillRectangle(oBrush, new Rectangle(0, 0, Width, Height));
            }
            //draw up arrow
            if (UpArrowImage != null) {
                e.Graphics.DrawImage(UpArrowImage, new Rectangle(new Point(0, 0), new Size(this.Width, UpArrowHeight)));
            }

            //draw thumb
            int nTop = moThumbTop + UpArrowHeight;
            using (Brush oBrush = new SolidBrush((ThumbIsHover) ? ThumbHoverColor : ThumbColor)) {
                e.Graphics.FillRectangle(oBrush, new Rectangle(2, nTop, Width - 3, ThumbHeight));
            }

            if (DownArrowImage != null) {
                e.Graphics.DrawImage(DownArrowImage, new Rectangle(new Point(0, (Height - DownArrowHeight)), new Size(Width, DownArrowHeight)));
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
        }

        private void CustomScrollbar_MouseUp(object sender, MouseEventArgs e) {
            this.moThumbDown     = false;
            this.moThumbDragging = false;
            repeatTimer.Stop();
        }

        protected override void OnMouseLeave(EventArgs e) {
            ThumbIsHover = false;
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

        private void CheckHover() {
            if (moThumbDown) { ThumbIsHover = true; return; }
            Point ptPoint = this.PointToClient(Cursor.Position);
            Rectangle thumbRect = new Rectangle(new Point(1, moThumbTop + UpArrowHeight), new Size(Width, ThumbHeight));
            ThumbIsHover = thumbRect.Contains(ptPoint);
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
