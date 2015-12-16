using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Darwen.Windows.Forms.General
{
    public delegate bool DragHandlerHitTester(Point point);

    /// <summary>
    /// Allows you to override the default Control mouse events and provide your own, by implementing
    /// this on the control you pass into the drag handler
    /// </summary>
    public interface IDragMouseEvents
    {
        event MouseEventHandler MouseDown;
        event MouseEventHandler MouseUp;
        event MouseEventHandler MouseMove;
        event EventHandler Click;
        event EventHandler DoubleClick;
    }

    public class DragHandler
    {
        private DragHandlerHitTester _hitTester;
        private Control _control;
        private bool _dragging;
        private Point _startDragPoint;
        private Point _offset;
        private bool _mouseDown;
        private const int Tolerance = 3;
        private bool _enabled;
        private CancelKeyMessageHandler _cancelKeyMessageHandler;

        public event MouseEventHandler DragStart;
        public event MouseEventHandler Drag;
        public event MouseEventHandler DragEnd;
        public event EventHandler DragCancelled;

        public DragHandler(Control control)
            : this(control, null)
        {
        }

        public DragHandler(Control control, DragHandlerHitTester hitTester)
        {
            _control = control;
            _hitTester = hitTester;
            _enabled = true;
            _cancelKeyMessageHandler = new CancelKeyMessageHandler(this);

            InitialiseEvents();
        }

        public Control Control
        {
            get
            {
                return _control;
            }
        }

        public Point Offset
        {
            get
            {
                return _offset;
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
                if (_dragging)
                {
                    throw new InvalidOperationException("Can't set enabled when dragging");
                }
                else
                {
                    _enabled = value;
                }
            }
        }

        public bool Dragging
        {
            get
            {
                return _dragging;
            }
        }

        private void CancelDrag()
        {
            if (_mouseDown)
            {
                _mouseDown = false;
                _control.Capture = false;

                if (_dragging)
                {
                    _dragging = false;
                    RemoveCancelKeyHandler();
                    OnDragCancelled();
                }
            }
        }


        protected virtual void OnDragStart(MouseEventArgs args)
        {
            InitialiseCancelKeyHandler();

            _startDragPoint = args.Location;

            if (DragStart != null)
            {
                DragStart(this, args);
            }
        }

        protected virtual void OnDragEnd(MouseEventArgs args)
        {
            if (DragEnd != null)
            {
                DragEnd(this, args);
            }
        }

        protected virtual void OnDrag(MouseEventArgs args)
        {
            _offset.X = args.X - _startDragPoint.X;
            _offset.Y = args.Y - _startDragPoint.Y;

            if (Drag != null)
            {
                Drag(this, args);
            }
        }

        protected virtual void OnDragCancelled()
        {
            if (DragCancelled != null)
            {
                DragCancelled(this, null);
            }
        }

        protected virtual bool OnHitTest(Point point)
        {
            bool hit = false;

            if (_hitTester == null)
            {
                hit = _control.ClientRectangle.Contains(point);
            }
            else
            {
                hit = _hitTester(point);
            }

            return hit;
        }

        private void InitialiseEvents()
        {
            IDragMouseEvents dragMouseEvents = _control as IDragMouseEvents;

            if (dragMouseEvents == null)
            {
                _control.MouseMove += new MouseEventHandler(_control_MouseMove);
                _control.MouseDown += new MouseEventHandler(_control_MouseDown);
                _control.MouseUp += new MouseEventHandler(_control_MouseUp);
                _control.Click += new EventHandler(_control_Click);
                _control.DoubleClick += new EventHandler(_control_DoubleClick);
            }
            else
            {
                dragMouseEvents.MouseMove += new MouseEventHandler(_control_MouseMove);
                dragMouseEvents.MouseDown += new MouseEventHandler(_control_MouseDown);
                dragMouseEvents.MouseUp += new MouseEventHandler(_control_MouseUp);
                dragMouseEvents.Click += new EventHandler(_control_Click);
                dragMouseEvents.DoubleClick += new EventHandler(_control_DoubleClick);
            }
        }

        private void _control_DoubleClick(object sender, EventArgs e)
        {
            CancelDrag();
        }

        private void _control_Click(object sender, EventArgs e)
        {
            CancelDrag();
        }

        private void _control_MouseUp(object sender, MouseEventArgs e)
        {
            if (_enabled)
            {
                _mouseDown = false;
                _control.Capture = false;

                if (_dragging)
                {
                    _dragging = false;
                    RemoveCancelKeyHandler();
                    OnDragEnd(e);
                }
            }
        }

        private void _control_MouseDown(object sender, MouseEventArgs e)
        {
            if (_enabled)
            {
                if (e.Button == MouseButtons.Left)
                {
                    bool hit = OnHitTest(e.Location);

                    if (hit && !_dragging)
                    {
                        _startDragPoint = e.Location;
                        _control.Capture = true;
                        _mouseDown = true;
                    }
                }
            }
        }

        private void _control_MouseMove(object sender, MouseEventArgs e)
        {
            if (_enabled)
            {
                if (_mouseDown && !_dragging)
                {
                    if (Math.Abs(e.X - _startDragPoint.X) >= Tolerance ||
                        Math.Abs(e.Y - _startDragPoint.Y) >= Tolerance)
                    {
                        _dragging = true;

                        OnDragStart(new MouseEventArgs(e.Button, 1, _startDragPoint.X, _startDragPoint.Y, 0));
                    }
                }

                if (_dragging)
                {
                    OnDrag(e);
                }
            }
        }

        private void InitialiseCancelKeyHandler()
        {
            Application.AddMessageFilter(_cancelKeyMessageHandler);
        }

        private void RemoveCancelKeyHandler()
        {
            Application.RemoveMessageFilter(_cancelKeyMessageHandler);            
        }

        private class CancelKeyMessageHandler : IMessageFilter
        {
            private DragHandler _handler;

            public CancelKeyMessageHandler(DragHandler handler)
            {
                _handler = handler;
            }

            #region IMessageFilter Members

            public bool PreFilterMessage(ref Message m)
            {
                bool result = false;

                if (m.Msg == NativeMethods.Constants.WM_KEYDOWN)
                {
                    if (m.WParam == (IntPtr)Keys.Escape)
                    {
                        _handler.CancelDrag();
                        result = true;
                    }
                }

                return result;
            }

            #endregion
        }
    }
}
