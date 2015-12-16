using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Darwen.Windows.Forms.General
{
    public partial class GradientFillUserControl : UserControl
    {
        public GradientFillUserControl()
        {
            InitializeComponent();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Rectangle clientRectangle = ClientRectangle;

            if (clientRectangle.Width > 0 && clientRectangle.Height > 0)
            {
                using (Brush brush = new LinearGradientBrush(clientRectangle, Color.FromKnownColor(KnownColor.Control),
                    Color.FromKnownColor(KnownColor.Menu), LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, clientRectangle);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Invalidate();
        }
    }
}
