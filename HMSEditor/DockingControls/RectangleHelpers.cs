using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Darwen.Drawing.General
{
    public enum RectangleCentreAlignment
    {
        TopLeft,
        TopCentre,
        TopRight,
        CentreLeft,
        Centre,
        CentreRight,
        BottomLeft,
        BottomCentre,
        BottomRight
    }
        
    public sealed class RectangleHelpers
    {
        private RectangleHelpers()
        {
        }

        static public Rectangle CentreRectangle(Rectangle outer, Rectangle inner, RectangleCentreAlignment alignment, int borderSize)
        {
            Rectangle result = new Rectangle();

            result.Width = inner.Width;
            result.Height = inner.Height;

            switch (alignment)
            {
                case RectangleCentreAlignment.TopLeft:
                case RectangleCentreAlignment.CentreLeft:
                case RectangleCentreAlignment.BottomLeft:
                    result.X = outer.Left + borderSize;
                    break;

                case RectangleCentreAlignment.TopCentre:
                case RectangleCentreAlignment.Centre:
                case RectangleCentreAlignment.BottomCentre:
                    result.X = outer.X + (outer.Width / 2) - (inner.Width / 2);
                    break;

                case RectangleCentreAlignment.TopRight:
                case RectangleCentreAlignment.CentreRight:
                case RectangleCentreAlignment.BottomRight:
                    result.X = outer.Right - (inner.Width + borderSize);
                    break;
            }

            switch (alignment)
            {
                case RectangleCentreAlignment.TopLeft:
                case RectangleCentreAlignment.TopCentre:
                case RectangleCentreAlignment.TopRight:
                    result.Y = outer.Top + borderSize;
                    break;

                case RectangleCentreAlignment.CentreLeft:
                case RectangleCentreAlignment.Centre:
                case RectangleCentreAlignment.CentreRight:
                    result.Y = outer.Y + (outer.Height / 2) - (inner.Height / 2);
                    break;

                case RectangleCentreAlignment.BottomLeft:
                case RectangleCentreAlignment.BottomCentre:
                case RectangleCentreAlignment.BottomRight:
                    result.Y = outer.Bottom - (inner.Height + borderSize);
                    break;
            }

            return result;
        }
    }
}
