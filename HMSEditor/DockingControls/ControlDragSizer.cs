using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using Darwen.Drawing.General;

namespace Darwen.Windows.Forms.General
{
    public class ControlDragSizer : DragHandler
    {
        private SetCursor _setCursor;
        private RectangleDrag _rectangleDrag;
        private DragDirectionFlags _flags = DragDirectionFlags.None;
        private Rectangle _bounds;
        private bool _dragging;

        private enum DragDirection
        {
            None, NorthWest, North, NorthEast, East, SouthEast, South, SouthWest, West
        }

        [Flags]
        private enum DragDirectionFlags
        {
            None = 0, North = 1, South = 2, East = 4, West = 8
        }

        public ControlDragSizer(Control control)
            : base(control)
        {
            InitialiseEvents();
        }

        protected override void OnDragStart(MouseEventArgs args)
        {
            base.OnDragStart(args);

            _dragging = true;

            _rectangleDrag = new RectangleDrag();
            _bounds = Control.Bounds;

            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                _rectangleDrag.Start(GetScreenRectangle(_bounds), graphics);
            }

            _flags = GetDirectionFlags(GetDragDirection(args.Location));            
        }

        protected override void OnDrag(MouseEventArgs args)
        {
            base.OnDrag(args);

            Rectangle bounds = GetSizedRectangle();

            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                _rectangleDrag.Size(bounds, graphics);
            }
        }

        protected override void OnDragCancelled()
        {
            _dragging = false;

            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                if (_rectangleDrag != null)
                {
                    _rectangleDrag.End(graphics);
                    _rectangleDrag.Dispose();
                    _rectangleDrag = null;
                }
            }

            SetCursor(null);

            base.OnDragCancelled();            
        }

        protected override void OnDragEnd(MouseEventArgs args)
        {
            _dragging = false;

            Rectangle bounds = GetSizedRectangle();            

            using (DesktopGraphics graphics = new DesktopGraphics())
            {
                if (_rectangleDrag != null)
                {
                    _rectangleDrag.End(graphics);
                    _rectangleDrag.Dispose();
                    _rectangleDrag = null;
                }
            }

            SetCursor(null);
            Control.Bounds = bounds;
            Control.Invalidate();

            base.OnDragEnd(args);
        }

        private Rectangle GetSizedRectangle()
        {
            int left = _bounds.Left;
            int top = _bounds.Top;
            int width = _bounds.Width;
            int height = _bounds.Height;

            if ((_flags & DragDirectionFlags.East) != 0)
            {
                width += Offset.X;
            }

            if ((_flags & DragDirectionFlags.West) != 0)
            {
                left += Offset.X;
                width -= Offset.X;
            }

            if ((_flags & DragDirectionFlags.North) != 0)
            {
                top += Offset.Y;
                height -= Offset.Y;
            }

            if ((_flags & DragDirectionFlags.South) != 0)
            {
                height += Offset.Y;
            }

            width = Math.Max(width, Control.MinimumSize.Width);
            height = Math.Max(height, Control.MinimumSize.Height);

            return new Rectangle(left, top, width, height);
        }

        private Rectangle GetScreenRectangle(Rectangle rectangle)
        {
            if (Control.Parent == null)
            {
                return rectangle;
            }
            else
            {
                return Control.Parent.RectangleToScreen(rectangle);
            }
        }

        private void InitialiseEvents()
        {
            Control.MouseMove += new MouseEventHandler(Control_MouseMove);
            Control.MouseLeave += new EventHandler(Control_MouseLeave);
        }

        private void SetCursor(Cursor cursor)
        {
            if (cursor == null)
            {
                if (_setCursor != null)
                {
                    _setCursor.Dispose();
                    _setCursor = null;
                }
            }
            else
            {
                if (_setCursor != null && cursor != _setCursor.Cursor)
                {
                    _setCursor.Dispose();
                    _setCursor = null;
                }

                if (_setCursor == null)
                {
                    _setCursor = new SetCursor(Control, cursor);
                }
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging)
            {
                SetCursor(GetCursorFromPoint(e.Location));
            }
        }

        private DragDirectionFlags GetDirectionFlags(DragDirection direction)
        {
            DragDirectionFlags flags = DragDirectionFlags.None;

            switch (direction)
            {
                case DragDirection.None:
                    break;
                case DragDirection.NorthWest:
                    flags = DragDirectionFlags.North | DragDirectionFlags.West;
                    break;
                case DragDirection.North:
                    flags = DragDirectionFlags.North;
                    break;
                case DragDirection.NorthEast:
                    flags = DragDirectionFlags.North | DragDirectionFlags.East;
                    break;
                case DragDirection.East:
                    flags = DragDirectionFlags.East;
                    break;
                case DragDirection.SouthEast:
                    flags = DragDirectionFlags.South | DragDirectionFlags.East;
                    break;
                case DragDirection.South:
                    flags = DragDirectionFlags.South;
                    break;
                case DragDirection.SouthWest:
                    flags = DragDirectionFlags.South | DragDirectionFlags.West;
                    break;
                case DragDirection.West:
                    flags = DragDirectionFlags.West;
                    break;
                default:
                    break;
            }

            return flags;
        }

        private Cursor GetCursorFromPoint(Point point)
        {
            DragDirection direction = GetDragDirection(point);
            Cursor cursor = null;

            switch (direction)
            {
                case DragDirection.NorthWest:
                    cursor = Cursors.SizeNWSE;
                    break;
                case DragDirection.North:
                    cursor = Cursors.SizeNS;
                    break;
                case DragDirection.NorthEast:
                    cursor = Cursors.SizeNESW;
                    break;
                case DragDirection.East:
                    cursor = Cursors.SizeWE;
                    break;
                case DragDirection.SouthEast:
                    cursor = Cursors.SizeNWSE;
                    break;
                case DragDirection.South:
                    cursor = Cursors.SizeNS;
                    break;
                case DragDirection.SouthWest:
                    cursor = Cursors.SizeNESW;
                    break;
                case DragDirection.West:
                    cursor = Cursors.SizeWE;
                    break;
                default:
                    cursor = Cursors.Arrow;
                    break;
            }

            return cursor;
        }

        private DragDirection GetDragDirection(Point point)
        {
            DragDirection direction = DragDirection.None;

            if (point.Y > Control.ClientRectangle.Top + Control.Padding.Top &&
                    point.Y < Control.ClientRectangle.Bottom - Control.Padding.Bottom)
            {
                if (point.X < Control.ClientRectangle.Left + Control.Padding.Left &&
                    point.X >= Control.ClientRectangle.Left)
                {
                    direction = DragDirection.West;
                }

                if (point.X > Control.ClientRectangle.Right - Control.Padding.Right &&
                        point.X <= Control.ClientRectangle.Right)
                {
                    direction = DragDirection.East;
                }
            }
            else if (point.X > Control.ClientRectangle.Left + Control.Padding.Left &&
                    point.X < Control.ClientRectangle.Right - Control.Padding.Right)
            {
                if (point.Y < Control.ClientRectangle.Top + Control.Padding.Top &&
                    point.Y >= Control.ClientRectangle.Top)
                {
                    direction = DragDirection.North;
                }

                if (point.Y > Control.ClientRectangle.Bottom - Control.Padding.Bottom &&
                    point.Y <= Control.ClientRectangle.Bottom)
                {
                    direction = DragDirection.South;
                }
            }

            if (point.X >= Control.ClientRectangle.Left &&
                     point.X < Control.ClientRectangle.Left + Control.Padding.Left &&
                     point.Y >= Control.ClientRectangle.Top &&
                     point.Y < Control.ClientRectangle.Top + Control.Padding.Top)
            {
                direction = DragDirection.NorthWest;
            }
            else if (point.X >= Control.ClientRectangle.Right - Control.Padding.Right &&
                     point.X < Control.ClientRectangle.Right &&
                     point.Y >= Control.ClientRectangle.Top &&
                     point.Y < Control.ClientRectangle.Top + Control.Padding.Top)
            {
                direction = DragDirection.NorthEast;
            }
            else if (point.X >= Control.ClientRectangle.Left &&
                     point.X < Control.ClientRectangle.Left + Control.Padding.Left &&
                     point.Y < Control.ClientRectangle.Bottom &&
                     point.Y >= Control.ClientRectangle.Bottom - Control.Padding.Bottom)
            {
                direction = DragDirection.SouthWest;
            }
            else if (point.X >= Control.ClientRectangle.Right - Control.Padding.Right &&
                     point.X < Control.ClientRectangle.Right &&
                     point.Y < Control.ClientRectangle.Bottom &&
                     point.Y >= Control.ClientRectangle.Bottom - Control.Padding.Bottom)
            {
                direction = DragDirection.SouthEast;
            }

            return direction;
        }

        private void Control_MouseLeave(object sender, EventArgs e)
        {
            SetCursor(null);
        }        
    }
}
