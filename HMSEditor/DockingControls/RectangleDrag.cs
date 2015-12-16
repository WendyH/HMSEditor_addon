using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Darwen.Drawing.General
{
    public class RectangleDrag : IDisposable
    {
        private Point _origin;
        private Bitmap _lastBitmap;
        private Rectangle _lastRectangle;
        private Size _originalSize;
        private const int PenWidth = 2;

        public RectangleDrag()
        {
        }

        public void Start(Rectangle rectangle, Graphics graphics)
        {
            _origin = rectangle.Location;
            _originalSize = rectangle.Size;
            DrawRectangle(graphics, rectangle);            
        }

        public void Offset(Point offset, Graphics graphics)
        {
            RestoreBitmap(graphics);
            _origin.Offset(offset);
            DrawRectangle(graphics);
        }

        public void Size(Rectangle newRectangle, Graphics graphics)
        {
            if (newRectangle != _lastRectangle)
            {
                RestoreBitmap(graphics);
                DrawRectangle(graphics, newRectangle);
            }
        }                

        public Rectangle End(Graphics graphics)
        {
            RestoreBitmap(graphics);
            return GetDragRectangle();
        }

        private void SaveBitmap(Graphics graphics, Rectangle location)
        {
            if (_lastBitmap != null)
            {
                _lastBitmap.Dispose();
            }

            _lastBitmap = new Bitmap(location.Width + 2 * PenWidth, location.Height + 2 * PenWidth);

            DrawingNativeMethods.CopyBitmapFromGraphics(graphics, GetLocation(location), _lastBitmap);
            _lastRectangle = location;
        }

        private Point GetLocation(Rectangle location)
        {
            Point offsetLocation = location.Location;
            offsetLocation.X -= (PenWidth);
            offsetLocation.Y -= (PenWidth);

            return offsetLocation;
        }

        private void RestoreBitmap(Graphics graphics)
        {
            graphics.DrawImageUnscaled(_lastBitmap, GetLocation(_lastRectangle));
        }

        private Rectangle GetDragRectangle()
        {
            return new Rectangle(_origin.X, _origin.Y, _originalSize.Width, _originalSize.Height);
        }

        private void DrawRectangle(Graphics graphics)
        {
            DrawRectangle(graphics, GetDragRectangle());
        }

        private void DrawRectangle(Graphics graphics, Rectangle rectangle)
        {
            SaveBitmap(graphics, rectangle);

            using (Pen pen = new Pen(Color.FromKnownColor(KnownColor.ActiveCaption), (float)PenWidth))
            {
                graphics.DrawRectangle(pen, rectangle);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_lastBitmap != null)
            {
                _lastBitmap.Dispose();
                _lastBitmap = null;
            }
        }

        #endregion
    }
}
