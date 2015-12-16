using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.Docking
{
    internal partial class AutoResizeControl : UserControl
    {
        private DockingType _dock;
        private ControlResizeAnimator _animator;
        private Control _inner;
        private EventHandler _parentSizeChangedHandler;
        private Control _oldParent;        
        private SetCursor _setDragCursor;
        private SplitterDragHandler _dragHandler;
        private AutoHideControlHandler _autoHideHandler;
        private DockControlContainer _container;
        private PaddingDrawHandler _paddingDrawHandler;
        
        protected AutoResizeControl()
        {
            InitializeComponent();            
        }

        public AutoResizeControl(DockingControl control, DockControlContainer container)
        {
            InitializeComponent();

            this.Width = control.Width;
            this.Height = control.Height;

            _animator = new ControlResizeAnimator(this, control.Width);
            
            _inner = control;
            _container = container;

            _inner.Parent = this;            
            _inner.Dock = DockStyle.Fill;
            
            DragHandler dragHandler = new DragHandler(this, new DragHandlerHitTester(HitTestSizeArea));
            _dragHandler = new SplitterDragHandler(dragHandler, SplitterDragHandlerDirection.EastWest);
                                
            _dragHandler.EndSplitterDrag += new SplitterDragEndHandler(_dragHandler_EndSplitterDrag);
            
            _autoHideHandler = new AutoHideControlHandler(this);
            _autoHideHandler.Hide += new Darwen.Windows.Forms.General.AutoHideHandler(_autoHideHandler_Hide);

            _paddingDrawHandler = new PaddingDrawHandler(this);
        }

        public DockControlContainer DockControlContainer
        {
            get
            {
                return _container;
            }
        }

        public DockingType DockDirection
        {
            get
            {
                return _dock;
            }

            set
            {
                _dock = value;
                _animator.Direction = DockingTypeConverter.ToDirection(value);

                switch (_dock)
                {
                    case DockingType.Floating:
                        break;
                    case DockingType.Left:
                        _dragHandler.Direction = SplitterDragHandlerDirection.EastWest;
                        _animator.TargetSize = _inner.Width;
                        break;
                    case DockingType.Right:
                        _dragHandler.Direction = SplitterDragHandlerDirection.EastWest;
                        _animator.TargetSize = _inner.Width;
                        break;
                    case DockingType.Top:
                        _dragHandler.Direction = SplitterDragHandlerDirection.NorthSouth;
                        _animator.TargetSize = _inner.Height;
                        break;
                    case DockingType.Bottom:
                        _dragHandler.Direction = SplitterDragHandlerDirection.NorthSouth;
                        _animator.TargetSize = _inner.Height;
                        break;
                    default:
                        break;
                }

                if (Parent != null)
                {
                    PositionControl();
                }
            }
        }

        public int TargetSize
        {
            get
            {
                return _animator.TargetSize;
            }

            set
            {
                _animator.TargetSize = value;
            }
        }


        public void Show(bool animated)
        {
            PositionControl();

            if (animated && !_animator.Showing)
            {
                _animator.Show();
            }
            else
            {
                Rectangle bounds = Rectangle.Empty;

                switch (_dock)
                {
                    case DockingType.Floating:
                        break;
                    case DockingType.Left:
                        this.Width = this.TargetSize;
                        break;
                    case DockingType.Right:
                        bounds = this.Bounds;
                        bounds.X = bounds.Right - TargetSize;
                        bounds.Width = TargetSize;
                        this.Bounds = bounds;
                        break;
                    case DockingType.Top:
                        this.Height = this.TargetSize;
                        break;
                    case DockingType.Bottom:
                        bounds = this.Bounds;
                        bounds.Y = bounds.Bottom - TargetSize;
                        bounds.Height = TargetSize;
                        this.Bounds = bounds;
                        break;
                    default:
                        break;
                }

                this.Visible = true;
            }
        }

        public void Hide(bool animated)
        {
            if (animated)
            {
                _animator.Hide();
            }
            else
            {
                this.Visible = false;
            }
        }

        public bool Showing
        {
            get
            {
                return _animator.Showing;
            }
        }

        public void StartTrackMouse()
        {
            _autoHideHandler.Start();
        }

        public void StopTrackMouse()
        {
            _autoHideHandler.Stop();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (_oldParent != null)
            {
                _oldParent.SizeChanged -= _parentSizeChangedHandler;
            }

            if (Parent != null)
            {
                _parentSizeChangedHandler = new EventHandler(Parent_SizeChanged);
                Parent.SizeChanged += _parentSizeChangedHandler;
                _oldParent = Parent;
                PositionControl();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_dragHandler.Dragging)
            {
                TestCursor(e.Location);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_setDragCursor != null && !_dragHandler.Dragging)
            {
                _setDragCursor.Dispose();
                _setDragCursor = null;
            }            
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            TestCursor(e.Location);
        }

        private void TestCursor(Point cursorPosition)
        {
            if (_setDragCursor != null)
            {
                _setDragCursor.Dispose();
                _setDragCursor = null;
            }

            if (HitTestSizeArea(cursorPosition))
            {
                switch (_dock)
                {
                    case DockingType.Left:
                    case DockingType.Right:
                        _setDragCursor = new SetCursor(this, Cursors.VSplit);
                        break;

                    case DockingType.Top:
                    case DockingType.Bottom:
                        _setDragCursor = new SetCursor(this, Cursors.HSplit);
                        break;
                }
            }            
        }

        private bool HitTestSizeArea(Point point)
        {
            if (Padding.Right > 0)
            {
                Rectangle bounds = this.ClientRectangle;
                Rectangle area = new Rectangle(bounds.Right - Padding.Right, bounds.Top, Padding.Right, bounds.Bottom);

                if (area.Contains(point))
                {
                    return true;
                }
            }

            if (Padding.Left > 0)
            {
                Rectangle bounds = this.ClientRectangle;
                Rectangle area = new Rectangle(bounds.Left, bounds.Top, Padding.Left, bounds.Bottom);

                if (area.Contains(point))
                {
                    return true;
                }
            }

            if (Padding.Top > 0)
            {
                Rectangle bounds = this.ClientRectangle;
                Rectangle area = new Rectangle(bounds.Left, bounds.Top, bounds.Right, bounds.Top + Padding.Top);

                if (area.Contains(point))
                {
                    return true;
                }
            }

            if (Padding.Bottom > 0)
            {
                Rectangle bounds = this.ClientRectangle;
                Rectangle area = new Rectangle(bounds.Left, bounds.Bottom - Padding.Bottom, bounds.Right, bounds.Bottom);

                if (area.Contains(point))
                {
                    return true;
                }
            }
            
            return false;            
        }

        private void Parent_SizeChanged(object sender, EventArgs e)
        {
            PositionControl();
        }

        protected virtual Rectangle GetDockingBounds()
        {
            DockingManagerControl manager = ControlHelpers.FindParentControl<DockingManagerControl>(this);

            if (manager == null)
            {
                throw new InvalidOperationException("Auto resize control must have a DockingManagerControl as one of its parents");
            }
            else
            {
                Rectangle bounds = manager.DockingBounds;

                switch (_container.Dock)
                {
                    case DockStyle.Left:
                        bounds.Width = _container.Width;
                        break;

                    case DockStyle.Top:
                        bounds.Height = _container.Height;
                        break;

                    case DockStyle.Right:
                        bounds.X = bounds.Right - _container.Width;
                        bounds.Width = _container.Width;
                        break;

                    case DockStyle.Bottom:
                        bounds.Y = bounds.Bottom - _container.Height;
                        bounds.Height = _container.Height;
                        break;

                    default:
                        System.Diagnostics.Debug.Assert(false, "Invalid dock style");
                        break;
                }

                return bounds;
            }
        }

        protected virtual void PositionControl()
        {
            this.Bounds = GetDockingBounds();
        }

        private void _autoHideHandler_Hide(AutoHideControlHandler sender)
        {
            _animator.Hide();
        }

        private void _dragHandler_EndSplitterDrag(SplitterDragHandler sender, int size)
        {
            switch (_dock)
            {
                case DockingType.Floating:
                    break;
                case DockingType.Left:
                    this.Width += size;
                    _animator.TargetSize = this.Width;
                    break;
                case DockingType.Right:
                    this.Left += size;
                    this.Width -= size;
                    _animator.TargetSize = this.Width;
                    break;
                case DockingType.Top:
                    this.Height += size;
                    _animator.TargetSize = this.Height;
                    break;
                case DockingType.Bottom:
                    this.Top += size;
                    this.Height -= size;
                    _animator.TargetSize = this.Height;
                    break;
                default:
                    break;
            }

            if (_setDragCursor != null)
            {
                _setDragCursor.Dispose();
                _setDragCursor = null;
            }            
        }        
    }
}
