﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Test {
    public partial class FlatScrollbar: UserControl {
        protected Color moChannelColor    = Color.WhiteSmoke;
        protected Color moThumbColor      = Color.DarkGray;
        protected Image moUpArrowImage    = null;
        protected Image moDownArrowImage  = null;

        protected int moLargeChange = 10;
        protected int moSmallChange = 1;
        protected int moMinimum     = 0;
        protected int moMaximum     = 100;
        protected int moValue       = 0;
        private   int nClickPoint;

        protected int moThumbTop = 0;

        protected bool moAutoSize = false;

        private bool moThumbDown = false;
        private bool moThumbDragging = false;

        public new event EventHandler Scroll       = null;
        public     event EventHandler ValueChanged = null;

        public FlatScrollbar() {

            InitializeComponent();
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            UpArrowImage   = new Bitmap(16, 16);
            DownArrowImage = new Bitmap(16, 16);

            this.Width = UpArrowImage.Width;
            base.MinimumSize = new Size(UpArrowImage.Width, UpArrowImage.Height + DownArrowImage.Height + GetThumbHeight());
        }

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
                moMinimum = value;
                Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Maximum")]
        public int Maximum {
            get { return moMaximum; }
            set {
                moMaximum = value;
                Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Behavior"), Description("Value")]
        public int Value {
            get { return moValue; }
            set {
                moValue = value;

                int nTrackHeight = (this.Height - (UpArrowImage.Height + DownArrowImage.Height));
                float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
                int nThumbHeight = (int)fThumbHeight;

                if (nThumbHeight > nTrackHeight) {
                    nThumbHeight = nTrackHeight;
                    fThumbHeight = nTrackHeight;
                }
                if (nThumbHeight < 56) {
                    nThumbHeight = 56;
                    fThumbHeight = 56;
                }

                //figure out value
                int nPixelRange = nTrackHeight - nThumbHeight;
                int nRealRange = (Maximum - Minimum) - LargeChange;
                float fPerc = 0.0f;
                if (nRealRange != 0) {
                    fPerc = (float)moValue / (float)nRealRange;

                }

                float fTop = fPerc * nPixelRange;
                moThumbTop = (int)fTop;


                Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Channel Color")]
        public Color ChannelColor {
            get { return moChannelColor; }
            set { moChannelColor = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
        public Image UpArrowImage {
            get { return moUpArrowImage; }
            set { moUpArrowImage = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(false), Category("Skin"), Description("Up Arrow Graphic")]
        public Image DownArrowImage {
            get { return moDownArrowImage; }
            set { moDownArrowImage = value; }
        }

        private int GetThumbHeight() {
            int nTrackHeight = (this.Height - (UpArrowImage.Height + DownArrowImage.Height));
            float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
            int nThumbHeight = (int)fThumbHeight;

            if (nThumbHeight > nTrackHeight) {
                nThumbHeight = nTrackHeight;
                fThumbHeight = nTrackHeight;
            }
            if (nThumbHeight < 56) {
                nThumbHeight = 56;
                fThumbHeight = 56;
            }

            return nThumbHeight;
        }

        protected override void OnPaint(PaintEventArgs e) {

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            if (UpArrowImage != null) {
                e.Graphics.DrawImage(UpArrowImage, new Rectangle(new Point(0, 0), new Size(this.Width, UpArrowImage.Height)));
            }

            //draw channel
            using (Brush oBrush = new SolidBrush(moChannelColor)) {
                e.Graphics.FillRectangle(oBrush, new Rectangle(1, UpArrowImage.Height, this.Width - 2, (this.Height - DownArrowImage.Height)));
            }


            //draw thumb
            int nTrackHeight = (this.Height - (UpArrowImage.Height + DownArrowImage.Height));
            float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
            int nThumbHeight = (int)fThumbHeight;

            if (nThumbHeight > nTrackHeight) {
                nThumbHeight = nTrackHeight;
                fThumbHeight = nTrackHeight;
            }
            if (nThumbHeight < 56) {
                nThumbHeight = 56;
                fThumbHeight = 56;
            }

            int nTop = moThumbTop;
            nTop += UpArrowImage.Height;

            using (Brush oBrush = new SolidBrush(moThumbColor)) {
                e.Graphics.FillRectangle(oBrush, new Rectangle(1, nTop, this.Width - 2, 56));
            }

            if (DownArrowImage != null) {
                e.Graphics.DrawImage(DownArrowImage, new Rectangle(new Point(0, (this.Height - DownArrowImage.Height)), new Size(this.Width, DownArrowImage.Height)));
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
            Point ptPoint = this.PointToClient(Cursor.Position);
            int nTrackHeight = (this.Height - (UpArrowImage.Height + DownArrowImage.Height));
            float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
            int nThumbHeight = (int)fThumbHeight;

            if (nThumbHeight > nTrackHeight) {
                nThumbHeight = nTrackHeight;
                fThumbHeight = nTrackHeight;
            }
            if (nThumbHeight < 56) {
                nThumbHeight = 56;
                fThumbHeight = 56;
            }

            int nTop = moThumbTop;
            nTop += UpArrowImage.Height;


            Rectangle thumbrect = new Rectangle(new Point(1, nTop), new Size(this.Width, nThumbHeight));
            if (thumbrect.Contains(ptPoint)) {

                //hit the thumb
                nClickPoint = (ptPoint.Y - nTop);
                //MessageBox.Show(Convert.ToString((ptPoint.Y - nTop)));
                this.moThumbDown = true;
            }

            Rectangle uparrowrect = new Rectangle(new Point(1, 0), new Size(UpArrowImage.Width, UpArrowImage.Height));
            if (uparrowrect.Contains(ptPoint)) {

                int nRealRange = (Maximum - Minimum) - LargeChange;
                int nPixelRange = (nTrackHeight - nThumbHeight);
                if (nRealRange > 0) {
                    if (nPixelRange > 0) {
                        if ((moThumbTop - SmallChange) < 0)
                            moThumbTop = 0;
                        else
                            moThumbTop -= SmallChange;

                        //figure out value
                        float fPerc = (float)moThumbTop / (float)nPixelRange;
                        float fValue = fPerc * (Maximum - LargeChange);

                        moValue = (int)fValue;

                        if (ValueChanged != null)
                            ValueChanged(this, new EventArgs());

                        if (Scroll != null)
                            Scroll(this, new EventArgs());

                        Invalidate();
                    }
                }
            }

            Rectangle downarrowrect = new Rectangle(new Point(1, UpArrowImage.Height + nTrackHeight), new Size(UpArrowImage.Width, UpArrowImage.Height));
            if (downarrowrect.Contains(ptPoint)) {
                int nRealRange = (Maximum - Minimum) - LargeChange;
                int nPixelRange = (nTrackHeight - nThumbHeight);
                if (nRealRange > 0) {
                    if (nPixelRange > 0) {
                        if ((moThumbTop + SmallChange) > nPixelRange)
                            moThumbTop = nPixelRange;
                        else
                            moThumbTop += SmallChange;

                        //figure out value
                        float fPerc = (float)moThumbTop / (float)nPixelRange;
                        float fValue = fPerc * (Maximum - LargeChange);

                        moValue = (int)fValue;

                        if (ValueChanged != null)
                            ValueChanged(this, new EventArgs());

                        if (Scroll != null)
                            Scroll(this, new EventArgs());

                        Invalidate();
                    }
                }
            }
        }

        private void CustomScrollbar_MouseUp(object sender, MouseEventArgs e) {
            this.moThumbDown     = false;
            this.moThumbDragging = false;
        }

        private void MoveThumb(int y) {
            int nRealRange = Maximum - Minimum;
            int nTrackHeight = (this.Height - (UpArrowImage.Height + DownArrowImage.Height));
            float fThumbHeight = ((float)LargeChange / (float)Maximum) * nTrackHeight;
            int nThumbHeight = (int)fThumbHeight;

            if (nThumbHeight > nTrackHeight) {
                nThumbHeight = nTrackHeight;
                fThumbHeight = nTrackHeight;
            }
            if (nThumbHeight < 56) {
                nThumbHeight = 56;
                fThumbHeight = 56;
            }

            int nSpot = nClickPoint;

            int nPixelRange = (nTrackHeight - nThumbHeight);
            if (moThumbDown && nRealRange > 0) {
                if (nPixelRange > 0) {
                    int nNewThumbTop = y - (UpArrowImage.Height + nSpot);

                    if (nNewThumbTop < 0) {
                        moThumbTop = nNewThumbTop = 0;
                    } else if (nNewThumbTop > nPixelRange) {
                        moThumbTop = nNewThumbTop = nPixelRange;
                    } else {
                        moThumbTop = y - (UpArrowImage.Height + nSpot);
                    }

                    //figure out value
                    float fPerc = (float)moThumbTop / (float)nPixelRange;
                    float fValue = fPerc * (Maximum - LargeChange);
                    moValue = (int)fValue;

                    Application.DoEvents();

                    Invalidate();
                }
            }
        }

        private void CustomScrollbar_MouseMove(object sender, MouseEventArgs e) {
            if (moThumbDown == true) {
                this.moThumbDragging = true;
            }

            if (this.moThumbDragging) {

                MoveThumb(e.Y);
            }

            if (ValueChanged != null)
                ValueChanged(this, new EventArgs());

            if (Scroll != null)
                Scroll(this, new EventArgs());
        }

    }

}
