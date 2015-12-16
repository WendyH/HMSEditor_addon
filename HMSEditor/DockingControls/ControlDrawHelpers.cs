using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Darwen.Drawing.General;

namespace Darwen.Windows.Forms.General
{
    public class ControlDrawHelpers
    {
        static public void DrawCaptionButton(Control control, ButtonState state)
        {
            using (Graphics graphics = control.CreateGraphics())
            {
                DrawCaptionButton(control, graphics, state);
            }
        }

        static public void DrawCaptionButton(Control control, Graphics graphics, ButtonState state)
        {
            System.Windows.Forms.ControlPaint.DrawCaptionButton(graphics, GetCaptionButtonBounds(control.ClientRectangle), CaptionButton.Close, state);
        }

        static public void DrawCaptionButton(Graphics graphics, Rectangle clientRectangle, ButtonState state)
        {
            System.Windows.Forms.ControlPaint.DrawCaptionButton(graphics, GetCaptionButtonBounds(clientRectangle), CaptionButton.Close, state);
        }

        static public Rectangle GetCaptionButtonBounds(Rectangle bounds)
        {
            Size captionButtonSize = new Size(14, 14);
            Rectangle buttonBounds = new Rectangle(0, 0, captionButtonSize.Width, captionButtonSize.Height);
            return RectangleHelpers.CentreRectangle(bounds, buttonBounds, RectangleCentreAlignment.CentreRight, captionButtonSize.Width / 4);            
        }

        static public Rectangle GetCaptionButtonBounds(Control control)
        {
            return GetCaptionButtonBounds(control.ClientRectangle);            
        }
    }
}
