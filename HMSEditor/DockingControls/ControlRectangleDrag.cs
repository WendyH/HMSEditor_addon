using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Darwen.Drawing.General;

namespace Darwen.Windows.Forms.General
{
    public class ControlRectangleDrag : IDisposable
    {
        private RectangleDrag _drag;
        private Control _control;
        private Point _startPoint;
        private Cursor _dragCursor;
        private SetCursor _setCursor;
        private bool _enabled;
        
        public ControlRectangleDrag(Control control)
            : this(control, null)
        {
            
        }

        public ControlRectangleDrag(Control control, Cursor cursor)
        {
            _control = control;
            _dragCursor = cursor;
            _enabled = true;
        }

        public Cursor DragCursor
        {
            get
            {
                return _dragCursor;
            }

            set
            {
                _dragCursor = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                _enabled = value;
            }
        }

        public bool Dragging
        {
            get
            {
                return _drag != null;
            }
        }

        public void Start(Point startPoint)
        {
            if (_enabled)
            {
                _drag = new RectangleDrag();
                _startPoint = startPoint;

                if (_dragCursor != null)
                {
                    if (_setCursor != null)
                    {
                        _setCursor.Dispose();
                    }

                    _setCursor = new SetCursor(_dragCursor);
                }

                using (DesktopGraphics graphics = new DesktopGraphics())
                {
                    _drag.Start(_control.Parent.RectangleToScreen(_control.Bounds), graphics);
                }
            }
        }

        public Rectangle End()
        {
            Rectangle result;

            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                result = _drag.End(graphics);
            }

            _drag.Dispose();
            _drag = null;

            _setCursor.Dispose();
            _setCursor = null;

            return result;
        }

        public void Offset(Point offset)
        {
            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                _drag.Offset(offset, graphics);
            }

            _startPoint.X += offset.X;
            _startPoint.Y += offset.Y;
        }

        public void MoveTo(Point point)
        {
            Offset(new Point(point.X - _startPoint.X, point.Y - _startPoint.Y));            
        }

        public void SetRectangle(Rectangle rectangle)
        {
            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                _drag.Size(rectangle, graphics);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_drag != null)
            {
                _drag.Dispose();
                _drag = null;
            }
        }

        #endregion
    }
}
