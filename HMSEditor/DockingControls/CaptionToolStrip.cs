using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.Docking
{
    internal partial class CaptionToolStrip : ToolStrip, IDragMouseEvents
    {
        private SetCursor _setCursor;
        private Timer _timer;
        private DragHandler _dragHandler;

        public event MouseEventHandler StartCaptionDrag;
        public event MouseEventHandler EndCaptionDrag;
        public event MouseEventHandler CaptionDrag;
        public event EventHandler CancelCaptionDrag;

        protected CaptionToolStrip()
        {
            InitializeComponent();
            InitializeDragHandler();
            InitializeEvents();
        }

        public CaptionToolStrip(IContainer container)            
        {
            container.Add(this);

            InitializeComponent();
            InitializeDragHandler();                        
            InitializeEvents();            
        }

        public bool DraggingEnabled
        {
            get
            {
                return _dragHandler.Enabled;
            }

            set
            {
                _dragHandler.Enabled = value;
            }
        }


        protected virtual void OnStartCaptionDrag(MouseEventArgs args)
        {
            if (StartCaptionDrag != null)
            {
                StartCaptionDrag(this, args);
            }                   
        }

        protected virtual void OnEndCaptionDrag(MouseEventArgs args)
        {
            if (EndCaptionDrag != null)
            {
                EndCaptionDrag(this, args);
            }            
        }

        protected virtual void OnCaptionDrag(MouseEventArgs args)
        {
            if (CaptionDrag != null)
            {
                CaptionDrag(this, args);
            }
        }        

        protected virtual void OnCancelCaptionDrag()
        {
            if (CancelCaptionDrag != null)
            {
                CancelCaptionDrag(this, null);
            }
        }        

        protected override void WndProc(ref Message m)
        {
            // WM_MOUSEACTIVE messages have to be passed through transparently
            // Default behaviour for a tool strip is to eat them
            if (m.Msg == NativeMethods.Constants.WM_MOUSEACTIVATE)
            {
                base.DefWndProc(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }            
        }

        protected override void OnMouseDown(MouseEventArgs mea)
        {
            if (DragHitTest(mea.Location))
            {
                if (_mouseDownHandlers != null)
                {
                    _mouseDownHandlers(this, mea);
                }
            }
            else
            {
                base.OnMouseDown(mea);
            }
        }

        protected override void OnMouseUp(MouseEventArgs mea)
        {
            if (_dragHandler.Dragging)
            {
                if (_mouseUpHandlers != null)
                {
                    _mouseUpHandlers(this, mea);
                }
            }
            else
            {
                base.OnMouseUp(mea);
            }
        }

        protected override void OnMouseMove(MouseEventArgs mea)
        {
            base.OnMouseMove(mea);

            if (_mouseMoveHandlers != null)
            {
                _mouseMoveHandlers(this, mea);
            }
        }

        private bool IsDraggableItem(ToolStripItem item)
        {
            if (item is ToolStripButton)
            {
                ToolStripButton button = item as ToolStripButton;                
                return !button.CanSelect;
            }
            else
            {
                return true;
            }
        }

        private void InitializeEvents()
        {
            this.MouseEnter += new EventHandler(control_MouseEnter);
            this.MouseLeave += new EventHandler(control_MouseLeave);                        
        }

        private void InitializeDragHandler()
        {
            _dragHandler = new DragHandler(this, new DragHandlerHitTester(DragHitTest));
            _dragHandler.DragStart += new MouseEventHandler(_dragHandler_DragStart);
            _dragHandler.Drag += new MouseEventHandler(_dragHandler_Drag);
            _dragHandler.DragEnd += new MouseEventHandler(_dragHandler_DragEnd);
            _dragHandler.DragCancelled += new EventHandler(_dragHandler_DragCancelled);
        }

        private void _dragHandler_DragCancelled(object sender, EventArgs e)
        {
            OnCancelCaptionDrag();
        }

        private void _dragHandler_DragEnd(object sender, MouseEventArgs e)
        {
            OnEndCaptionDrag(e);
        }

        private void _dragHandler_Drag(object sender, MouseEventArgs e)
        {
            OnCaptionDrag(e);
        }

        private void _dragHandler_DragStart(object sender, MouseEventArgs e)
        {
            OnStartCaptionDrag(e);
        }

        private void DisposeCursor()
        {
            if (_setCursor != null)
            {
                _setCursor.Dispose();
                _setCursor = null;
            }
        }

        private void TestCursor(Point location)
        {
            foreach (ToolStripItem item in this.Items)
            {
                if (item is ToolStripButton && item.Bounds.Contains(location))
                {
                    DisposeCursor();
                    return;
                }
            }

            DockingControl parent = Parent as DockingControl;

            if (parent != null && !parent.AutoHide)
            {
                if (_setCursor == null)
                {
                    _setCursor = new SetCursor(this, Cursors.SizeAll);
                }
            }
        }

        private void control_MouseLeave(object sender, EventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            DisposeCursor();
        }

        private void control_MouseEnter(object sender, EventArgs e)
        {
            if (DraggingEnabled)
            {
                _timer = new Timer();
                _timer.Interval = 25;
                _timer.Tick += new EventHandler(_timer_Tick);
                _timer.Start();
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            Point cursorPoint = PointToClient(System.Windows.Forms.Cursor.Position);
            TestCursor(cursorPoint);
        }

        private bool DragHitTest(Point point)
        {
            foreach (ToolStripItem item in this.Items)
            {
                if (item.Bounds.Contains(point))
                {
                    return IsDraggableItem(item);
                }
            }

            return true;
        }

        #region IDragMouseEvents Members

        private event MouseEventHandler _mouseDownHandlers;
        private event MouseEventHandler _mouseUpHandlers;
        private event MouseEventHandler _mouseMoveHandlers;

        event MouseEventHandler IDragMouseEvents.MouseDown
        {
            add { _mouseDownHandlers = Delegate.Combine(_mouseDownHandlers, value) as MouseEventHandler; }
            remove { _mouseDownHandlers = Delegate.Remove(_mouseDownHandlers, value) as MouseEventHandler; }
        }

        event MouseEventHandler IDragMouseEvents.MouseUp
        {
            add { _mouseUpHandlers = Delegate.Combine(_mouseUpHandlers, value) as MouseEventHandler; }
            remove { _mouseUpHandlers = Delegate.Remove(_mouseUpHandlers, value) as MouseEventHandler; }
        }

        event MouseEventHandler IDragMouseEvents.MouseMove
        {
            add { _mouseMoveHandlers = Delegate.Combine(_mouseMoveHandlers, value) as MouseEventHandler; }
            remove { _mouseMoveHandlers = Delegate.Remove(_mouseMoveHandlers, value) as MouseEventHandler; }
        }       

        #endregion
    }
}


