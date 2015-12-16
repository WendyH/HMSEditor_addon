using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using Darwen.Drawing.General;

namespace Darwen.Windows.Forms.General
{
    public enum SplitterDragHandlerDirection
    {
        NorthSouth,
        EastWest
    }

    public delegate void SplitterDragEndHandler(SplitterDragHandler sender, int size);

    public class SplitterDragHandler : IDisposable
    {
        private DragHandler _dragHandler;
        private SetCursor _setDragCursor;
        private SplitterDragHandlerDirection _direction;
        private Bitmap _bitmap;
        private int _variantDimension;
        private int _invariantStartDimension;
        private int _invariantEndDimension;
        private int _offset;
        private const int RectangleSize = 3;

        public event SplitterDragEndHandler EndSplitterDrag;     
        
        public SplitterDragHandler(DragHandler dragHandler, SplitterDragHandlerDirection direction)
        {
            _dragHandler = dragHandler;            
            _dragHandler.DragStart += new MouseEventHandler(_handler_DragStart);
            _dragHandler.Drag += new MouseEventHandler(_handler_Drag);
            _dragHandler.DragEnd += new MouseEventHandler(_handler_DragEnd);
            _dragHandler.DragCancelled += new EventHandler(_dragHandler_DragCancelled);
        }
        
        public SplitterDragHandlerDirection Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value;
            }
        }

        public bool Dragging
        {
            get
            {
                return _dragHandler.Dragging;
            }
        }	

        protected virtual void OnEndSplitterDrag(int size)
        {
            if (EndSplitterDrag != null)
            {
                EndSplitterDrag(this, size);
            }
        }

        private void _handler_DragEnd(object sender, MouseEventArgs e)
        {
            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                RestoreBitmap(graphics);
                _bitmap.Dispose();
                _bitmap = null;
            }

            _setDragCursor.Dispose();
            _setDragCursor = null;
            
            switch (_direction)
            {
                case SplitterDragHandlerDirection.NorthSouth:
                    OnEndSplitterDrag(_dragHandler.Offset.Y);
                    break;

                case SplitterDragHandlerDirection.EastWest:
                    OnEndSplitterDrag(_dragHandler.Offset.X);
                    break;
            }            
        }

        private void _handler_Drag(object sender, MouseEventArgs e)
        {
            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                RestoreBitmap(graphics);

                switch (_direction)
                {
                    case SplitterDragHandlerDirection.EastWest:
                        _offset = _dragHandler.Offset.X;
                        break;

                    case SplitterDragHandlerDirection.NorthSouth:
                        _offset = _dragHandler.Offset.Y;
                        break;
                }

                Rectangle dragRectangle = GetDragRectangle();
                SaveBitmap(graphics);

                using (SolidBrush brush = new SolidBrush(Color.FromKnownColor(KnownColor.MenuHighlight)))
                {
                    graphics.Graphics.FillRectangle(brush, dragRectangle);
                }
            }
        }

        private void _handler_DragStart(object sender, MouseEventArgs e)
        {
            switch (_direction)
            {
                case SplitterDragHandlerDirection.EastWest:
                    _setDragCursor = new SetCursor(Cursors.VSplit);
                    _variantDimension = _dragHandler.Control.PointToClient(Cursor.Position).X;
                    _invariantStartDimension = _dragHandler.Control.ClientRectangle.Top;
                    _invariantEndDimension = _dragHandler.Control.ClientRectangle.Bottom;
                    break;
                case SplitterDragHandlerDirection.NorthSouth:
                    _setDragCursor = new SetCursor(Cursors.HSplit);
                    _variantDimension = _dragHandler.Control.PointToClient(Cursor.Position).Y;
                    _invariantStartDimension = _dragHandler.Control.ClientRectangle.Left;
                    _invariantEndDimension = _dragHandler.Control.ClientRectangle.Right;
                    break;
                default:                    
                    break;
            }

            Rectangle bitmapRectangle = GetDragRectangle();
            _bitmap = new Bitmap(bitmapRectangle.Width, bitmapRectangle.Height);
            
            _offset = 0;
            
            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                SaveBitmap(graphics);
            }            
        }

        private Rectangle GetDragRectangle()
        {
            Rectangle bitmapRectangle = Rectangle.Empty;

            switch (_direction)
            {
                case SplitterDragHandlerDirection.NorthSouth:
                    bitmapRectangle = new Rectangle(_invariantStartDimension, _variantDimension + _offset, _invariantEndDimension - _invariantStartDimension, RectangleSize);
                    break;                    
                case SplitterDragHandlerDirection.EastWest:
                    bitmapRectangle = new Rectangle(_variantDimension + _offset, _invariantStartDimension, RectangleSize, _invariantEndDimension - _invariantStartDimension);
                    break;
                default:
                    break;
            }

            return _dragHandler.Control.RectangleToScreen(bitmapRectangle);
        }

        private void SaveBitmap(Graphics graphics)
        {
            Rectangle bitmapRectangle = GetDragRectangle();
            Darwen.Drawing.General.DrawingNativeMethods.CopyBitmapFromGraphics(graphics, bitmapRectangle.Location, _bitmap);
        }

        private void RestoreBitmap(Graphics graphics)
        {
            if (_bitmap != null)
            {
                Rectangle bitmapRectangle = GetDragRectangle();
                graphics.DrawImageUnscaled(_bitmap, bitmapRectangle.Location);
            }
        }

        private void _dragHandler_DragCancelled(object sender, EventArgs e)
        {
            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                RestoreBitmap(graphics);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }

        #endregion
    }
}
