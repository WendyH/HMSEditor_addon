using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.Docking
{
    public interface IDockingControl
    {
        bool Cancelled { get; set; }
        void DockControl(int panelIndex, int index, DockingType direction);
        void FloatControl(Rectangle bounds);
        bool AutoHide { get; set; }
        int DockedDimension { get; set; }
        DockingType DockingType { get; }
        int DockIndex { get; set; }
        int PanelIndex { get; set; }
        string Title { get; set; }
    }

    public delegate void AutoHideChangedHandler(DockingControl control);
    public delegate void CancelledChangedHandler(DockingControl control);

    public partial class DockingControl : UserControl, IDockingControl
    {
        private ControlRectangleDrag _dragger;
        private bool _autoHide;
        private Control _child;
        private DockingManagerControl _manager;
        private FloatingForm _floatingForm;
        private const int DockingBarSize = 16;
        private DockControlContainer _container;
        private bool _cancelled;

        public DockingControl()
        {
            InitializeComponent();
        }

        public event AutoHideChangedHandler  AutoHideChanged;
        public event CancelledChangedHandler CancelledChanged;

        public DockingControl(Control child, DockingManagerControl manager)
        {
            manager.AddDockingControlToList(this);

            InitializeComponent();

            child.Parent = _transparentUserControl;
            child.Dock = DockStyle.Fill;
            _child = child;
            _manager = manager;
            _toolStripCaption.Renderer = manager.Renderer;
            _manager.ToolStripRendererChanged += new ToolStripRendererChangedHandler(_manager_ToolStripRendererChanged);

            SetupControlEvents(this);
        }

        public string Title
        {
            get
            {
                return _toolStripLabel.Text;
            }

            set
            {
                _toolStripLabel.Text = value;
            }
        }

        public bool DraggingEnabled
        {
            get
            {
                return _toolStripCaption.DraggingEnabled;
            }

            set
            {
                _toolStripCaption.DraggingEnabled = value;
            }
        }

        public Control Child
        {
            get
            {
                return _child;
            }
        }

        public bool AutoHide
        {
            get
            {
                return _autoHide;
            }

            set
            {
                if (_floatingForm == null)
                {
                    bool changed = _autoHide != value;
                    _autoHide = value;

                    if (changed)
                    {
                        OnAutoHideChanged();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Can't auto hide a floating form");
                }
            }
        }

        public bool Cancelled
        {
            get
            {
                return _cancelled;
            }

            set
            {
                bool changed = _cancelled != value;
                _cancelled = value;

                if (changed)
                {
                    this.Visible = !_cancelled;
                    OnCancelledChanged();
                }                
            }
        }        

        public Rectangle FloatingBounds
        {
            get
            {
                if (_floatingForm == null)
                {
                    throw new InvalidOperationException("Control is not floating");
                }
                else
                {
                    return _floatingForm.Bounds;
                }
            }
        }

        public DockingType DockingType
        {
            get
            {
                if (_floatingForm == null)
                {
                    switch (_container.Dock)
                    {
                        case DockStyle.Bottom:
                            return DockingType.Bottom;
                        case DockStyle.Fill:
                            throw new InvalidEnumArgumentException("Can't have a container with a dock style of fill");
                        case DockStyle.Left:
                            return DockingType.Left;
                        case DockStyle.None:
                            throw new InvalidEnumArgumentException("Can't have a container with a dock style of none");
                        case DockStyle.Right:
                            return DockingType.Right;
                        case DockStyle.Top:
                            return DockingType.Top;
                        default:
                            throw new InvalidEnumArgumentException("Unknown dock style");
                    }

                }
                else
                {
                    return DockingType.Floating;
                }
            }
        }

        public int DockIndex
        {
            get
            {
                if (_floatingForm == null)
                {
                    return _container.GetDockControlIndex(this);
                }
                else
                {
                    throw new InvalidOperationException("Can't get the dock index for a floating item");
                }
            }

            set
            {
                if (_floatingForm == null)
                {
                    _container.DockControl(this, value, DockControlHelpers.GetDockedDimension(this, _container.Dock));
                }
                else
                {
                    throw new InvalidOperationException("Can't set the dock index for a floating item");
                }
            }
        }

        public DockControlContainer DockingControlContainer
        {
            get
            {
                return _container;
            }

            set
            {
                _container = value;
            }
        }

        public int PanelIndex
        {
            get
            {
                if (_floatingForm == null)
                {
                    DockControlContainerCollection collection = DockControlContainerCollectionHelpers.GetCollection(_manager, _container.Dock);
                    return collection.IndexOf(_container);                                        
                }
                else
                {
                    throw new InvalidOperationException("Can't get the panel index for a floating item");
                }
            }

            set
            {
                if (_floatingForm == null)
                {
                    DockControlContainerCollection collection = DockControlContainerCollectionHelpers.GetCollection(_manager, _container.Dock);

                    if (collection.Count > value)
                    {
                        collection.InsertPanel(collection.Count);
                    }
                    else
                    {
                        throw new ArgumentException("Panel at index doesn't exist");
                    }

                    _container.DockControl(this, _container.DockedControlList.Count, DockControlHelpers.GetDockedDimension(this, _container.Dock));
                }
                else
                {
                    throw new InvalidOperationException("Can't set the dock index for a floating item");
                }
            }
        }

        #region IDockingControl Members

        public void DockControl(int panelIndex, int index, DockingType direction)
        {
            if (direction == DockingType.Floating)
            {
                throw new ArgumentException("Can't dock to a docking type of floating");
            }
            else
            {
                bool autoHide = this.AutoHide;
                bool cancelled = this.Cancelled;

                if (this.AutoHide)
                {
                    this.AutoHide = false;
                }

                this.Cancelled = false;

                DockControlContainerCollection panels = DockControlContainerCollectionHelpers.GetCollection(_manager, direction);
                DockControlContainer container = panels[panelIndex] as DockControlContainer;

                container.DockControl(this, container.DockedControlList.Count, index);

                RemoveFloatingForm();

                this.AutoHide = autoHide;
                this.Cancelled = cancelled;
            }
        }

        public void FloatControl(Rectangle bounds)
        {
            using (RedrawStopper redrawStopper = new RedrawStopper(_manager.ParentForm, true))
            {
                if (this.AutoHide)
                {
                    this.AutoHide = false;
                }

                DockControlContainer oldContainer = _container;
                Form parentForm = this.ParentForm;                

                if (oldContainer != null)
                {
                    oldContainer.RemoveControl(this);
                }

                if (_floatingForm == null || parentForm != _floatingForm)
                {                    
                    _floatingForm = new FloatingForm(this, _manager);
                    _autoHidetoolStripButton.Visible = false;
                    _tabsToolStripButton.Visible = false;                    
                }

                _floatingForm.Bounds = bounds;

                if (_floatingForm.Visible)
                {
                    _floatingForm.Invalidate();
                }
                else if (_floatingForm.Loaded)
                {
                    _floatingForm.Visible = !this.Cancelled;
                }
                else if (!this.Cancelled)
                {                    
                    _floatingForm.Show(parentForm);
                }                                
            }            
        }

        public int DockedDimension
        {
            get
            {
                if (_container == null)
                {
                    throw new InvalidOperationException("Control is not docked");
                }
                else
                {
                    AutoResizeControl autoResizeControl = this.Parent as AutoResizeControl;

                    if (autoResizeControl == null)
                    {
                        switch (_container.Dock)
                        {
                            case DockStyle.Left:
                            case DockStyle.Right:
                                return this.Height;

                            case DockStyle.Top:
                            case DockStyle.Bottom:
                                return this.Width;

                            default:
                                throw new InvalidOperationException("Control is not docked");
                        }
                    }
                    else
                    {
                        return autoResizeControl.TargetSize;
                    }
                }
            }
            set
            {
                if (_container == null)
                {
                    throw new InvalidOperationException("Control is not docked");
                }
                else
                {
                    AutoResizeControl autoResizeControl = this.Parent as AutoResizeControl;

                    if (autoResizeControl == null)
                    {
                        switch (_container.Dock)
                        {
                            case DockStyle.Left:
                            case DockStyle.Right:
                                this.Height = value;
                                break;

                            case DockStyle.Top:
                            case DockStyle.Bottom:
                                this.Width = value;
                                break;

                            default:
                                throw new InvalidOperationException("Control is not docked");
                        }
                    }
                    else
                    {
                        autoResizeControl.TargetSize = value;
                    }
                }
            }
        }        

        #endregion

        protected virtual void OnAutoHideChanged()
        {
            if (_autoHide)
            {
                _autoHidetoolStripButton.Image = global::HMSEditorNS.Properties.Resources.AutoHideOn;
                _tabsToolStripButton.Visible = false;
            }
            else
            {
                _autoHidetoolStripButton.Image = global::HMSEditorNS.Properties.Resources.AutoHideOff;
                _tabsToolStripButton.Visible = true;
            }

            if (AutoHideChanged != null)
            {
                AutoHideChanged(this);
            }
        }

        protected virtual void OnCancelledChanged()
        {
            if (CancelledChanged != null)
            {
                CancelledChanged(this);
            }
        }

        private DockControlContainer GetDropContainer(Point dropPoint)
        {
            return DockControlHelpers.GetDropContainer(_manager, PointToScreen(dropPoint), DockingBarSize);
        }

        protected virtual void OnEndCaptionDrag(Point dropPoint, Rectangle endDragBounds)
        {
            using (RedrawStopper stopRedraws = new RedrawStopper(_manager, true))
            {
                DockControlContainer container = GetDropContainer(dropPoint);
                
                if (container == null)
                {
                    endDragBounds.X -= FloatingForm.PaddingSize;
                    endDragBounds.Y -= FloatingForm.PaddingSize;
                    endDragBounds.Width += 2 * FloatingForm.PaddingSize;
                    endDragBounds.Height += 2 * FloatingForm.PaddingSize;
                    FloatControl(endDragBounds);
                }
                else
                {
                    int dimension = DockControlHelpers.GetDockedDimension(this, container.Dock);
                    container = DockControlHelpers.CreateNewContainerIfNecessary(_manager, container, PointToScreen(dropPoint), DockingBarSize);
                    DockControl(dropPoint, container, dimension);
                    RemoveFloatingForm();                    
                }
            }
        }

        private void DockControl(Point dropPoint, DockControlContainer container, int dimension)
        {
            Point containerLocalDropPoint = container.PointToClient(PointToScreen(dropPoint));

            int index = 0;

            DockedControlCollection controls = container.DockedControls as DockedControlCollection;

            foreach (DockingControl child in controls)
            {
                Rectangle bounds = child.Bounds;

                switch (container.Dock)
                {
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        if (bounds.Right >= containerLocalDropPoint.X)
                        {
                            if (containerLocalDropPoint.X < (bounds.Left + bounds.Right) / 2)
                            {
                                container.DockControl(this, index, dimension);
                                return;
                            }
                        }

                        break;

                    case DockStyle.Left:
                    case DockStyle.Right:
                        if (bounds.Bottom >= containerLocalDropPoint.Y)
                        {
                            if (containerLocalDropPoint.Y < (bounds.Top + bounds.Bottom) / 2)
                            {
                                container.DockControl(this, index, dimension);
                                return;
                            }
                        }

                        break;
                }

                index += 1;
            }

            container.DockControl(this, container.DockedControlList.Count, dimension);            
        }

        private void _toolStripCaption_StartCaptionDrag(object sender, MouseEventArgs e)
        {
            _dragger = new ControlRectangleDrag(this, Cursors.SizeAll);
            _dragger.Start(e.Location);
            _toolStripLabel.Enabled = true;
        }

        private void _toolStripCaption_CanelCaptionDrag(object sender, EventArgs e)
        {
            if (_dragger != null)
            {
                _dragger.End();
                _dragger.Dispose();
                _dragger = null;
            }
        }        

        private void _toolStripCaption_EndCaptionDrag(object sender, MouseEventArgs e)
        {
            Rectangle endDragBounds = Rectangle.Empty;

            if (_dragger != null)
            {
                _dragger.MoveTo(e.Location);
                endDragBounds = _dragger.End();
                _dragger.Dispose();
                _dragger = null;
            }

            OnEndCaptionDrag(e.Location, endDragBounds);
        }

        private void _toolStripCaption_CaptionDrag(object sender, MouseEventArgs e)
        {
            Point pointInScreen = this.PointToScreen(e.Location);                
            DockControlContainer container = _manager.GetContainerAtPoint(_manager.PointToClient(pointInScreen));
            
            if (container == null)
            {
                Rectangle dragRectangle = DockControlHelpers.GetDropXorDragRect(_manager, pointInScreen, DockingBarSize);

                if (!dragRectangle.IsEmpty)
                {
                    _dragger.SetRectangle(_manager.RectangleToScreen(dragRectangle));
                    return;
                }
            }
            else
            {
                Rectangle rectangleToDraw = GetContainerDragDrawRectangle(container, pointInScreen);

                if (!rectangleToDraw.IsEmpty)
                {
                    _dragger.SetRectangle(rectangleToDraw);
                    return;
                }

                if (container.Tabbed)
                {
                    _dragger.SetRectangle(container.RectangleToScreen(container.ClientRectangle));
                    return;
                }
                else
                {                    
                    DockingControl control = container.GetDockingControlAtPoint(container.PointToClient(pointInScreen));

                    if (control != null)
                    {
                        Point pointInControl = control.PointToClient(pointInScreen);
                        rectangleToDraw = GetContolDragDrawRectangle(container, control, pointInControl);
                        _dragger.SetRectangle(control.RectangleToScreen(rectangleToDraw));
                        return;                        
                    }
                }
            }
            
            _dragger.MoveTo(e.Location);            
        }

        private Rectangle GetContainerDragDrawRectangle(DockControlContainer container, Point pointInScreen)
        {            
            Point pointInControl = container.PointToClient(pointInScreen);
            Rectangle clientRectangle = container.ClientRectangle;
            Rectangle rectangleToDraw = Rectangle.Empty;
            Rectangle containerRectangle = container.RectangleToScreen(container.ClientRectangle);

            switch (container.Dock)
            {
                case DockStyle.Top:
                case DockStyle.Bottom:
                    if (pointInControl.Y > clientRectangle.Y && pointInControl.Y < clientRectangle.Y + DockingBarSize)
                    {
                        rectangleToDraw = containerRectangle;
                        rectangleToDraw.Height = DockingBarSize;
                    }
                    else if (pointInControl.Y < clientRectangle.Bottom && pointInControl.Y > clientRectangle.Bottom - DockingBarSize)
                    {
                        rectangleToDraw = containerRectangle;
                        rectangleToDraw.Y = containerRectangle.Bottom - DockingBarSize;
                        rectangleToDraw.Height = DockingBarSize;
                    }

                    break;

                case DockStyle.Left:
                case DockStyle.Right:
                    if (pointInControl.X > clientRectangle.X && pointInControl.X < clientRectangle.X + DockingBarSize)
                    {
                        rectangleToDraw = containerRectangle;
                        rectangleToDraw.Width = DockingBarSize;
                    }
                    else if (pointInControl.X < clientRectangle.Right && pointInControl.X > clientRectangle.Right - DockingBarSize)
                    {
                        rectangleToDraw = containerRectangle;
                        rectangleToDraw.X = containerRectangle.Right - DockingBarSize;
                        rectangleToDraw.Width = DockingBarSize;
                    }

                    break;

                default:
                    break;
            }

            return rectangleToDraw;
        }

        private Rectangle GetContolDragDrawRectangle(DockControlContainer container, DockingControl control, Point pointInControl)
        {
            Rectangle rectangleToDraw = Rectangle.Empty;

            Rectangle containerRectangle = control.RectangleToClient(container.RectangleToScreen(container.ClientRectangle));
            Rectangle clientRectangle = control.ClientRectangle;

            int index = container.DockedControlList.IndexOf(control);

            DockingControl controlBefore = GetControlBefore(container, index);
            DockingControl controlAfter = GetControlAfter(container, index);

            Rectangle bounds = control.Bounds;
            Rectangle beforeBounds = (controlBefore == null) ? Rectangle.Empty : controlBefore.Bounds;
            Rectangle afterBounds = (controlAfter == null) ? Rectangle.Empty : controlAfter.Bounds;

            switch (container.Dock)
            {
                case DockStyle.Left:
                case DockStyle.Right:
                    if (container.Tabbed)
                    {
                        rectangleToDraw = clientRectangle;
                    }
                    else if (pointInControl.Y < clientRectangle.Y + (clientRectangle.Height / 2))
                    {
                        if (controlBefore == null)
                        {
                            rectangleToDraw = clientRectangle;
                            rectangleToDraw.Height /= 2;
                        }
                        else
                        {
                            rectangleToDraw = new Rectangle(
                                beforeBounds.Left, beforeBounds.Top + (beforeBounds.Height / 2),
                                beforeBounds.Right, (bounds.Top + (bounds.Height / 2)) - (beforeBounds.Top + (beforeBounds.Height / 2)));

                            rectangleToDraw = control.RectangleToClient(container.RectangleToScreen(rectangleToDraw));
                        }
                    }
                    else
                    {
                        if (controlAfter == null)
                        {
                            rectangleToDraw = clientRectangle;
                            rectangleToDraw.Y = clientRectangle.Bottom - (clientRectangle.Height / 2);
                            rectangleToDraw.Height /= 2;
                        }
                        else
                        {
                            rectangleToDraw = new Rectangle(
                                bounds.Left, bounds.Top + (bounds.Height / 2),
                                bounds.Right, (afterBounds.Top + (afterBounds.Height / 2)) - (bounds.Top + (bounds.Height / 2)));

                            rectangleToDraw = control.RectangleToClient(container.RectangleToScreen(rectangleToDraw));
                        }
                    }

                    break;

                case DockStyle.Bottom:
                case DockStyle.Top:
                    if (container.Tabbed)
                    {
                        rectangleToDraw = clientRectangle;
                    }
                    else if (pointInControl.X < clientRectangle.X + (clientRectangle.Width / 2))
                    {
                        if (controlBefore == null)
                        {
                            rectangleToDraw = clientRectangle;
                            rectangleToDraw.Width /= 2;
                        }
                        else
                        {
                            rectangleToDraw = new Rectangle(
                                beforeBounds.Left + (beforeBounds.Width / 2), beforeBounds.Top,
                                (bounds.Left + (bounds.Width / 2)) - (beforeBounds.Left + (beforeBounds.Width / 2)), beforeBounds.Height);

                            rectangleToDraw = control.RectangleToClient(container.RectangleToScreen(rectangleToDraw));
                        }
                    }
                    else
                    {
                        if (controlAfter == null)
                        {
                            rectangleToDraw = clientRectangle;
                            rectangleToDraw.X = clientRectangle.Right - (clientRectangle.Width / 2);
                            rectangleToDraw.Width /= 2;
                        }
                        else
                        {
                            rectangleToDraw = new Rectangle(
                                bounds.Left + (bounds.Width / 2), bounds.Top,
                                (afterBounds.Left + (afterBounds.Width / 2)) - (bounds.Left + (bounds.Width / 2)), bounds.Height);

                            rectangleToDraw = control.RectangleToClient(container.RectangleToScreen(rectangleToDraw));
                        }
                    }

                    break;

                default:
                    break;
            }

            return rectangleToDraw;
        }

        private static DockingControl GetControlBefore(DockControlContainer container, int index)
        {
            DockingControl controlBefore = null;

            for (int indexBefore = index - 1; controlBefore == null && indexBefore >= 0; indexBefore -= 1)
            {
                controlBefore = container.DockedControlList[indexBefore];

                if (controlBefore.AutoHide || controlBefore.Cancelled)
                {
                    controlBefore = null;
                }
            }

            return controlBefore;
        }

        private static DockingControl GetControlAfter(DockControlContainer container, int index)
        {
            DockingControl controlAfter = null;

            for (int indexAfter = index + 1; controlAfter == null && indexAfter < container.DockedControlList.Count; indexAfter += 1)
            {
                controlAfter = container.DockedControlList[indexAfter];

                if (controlAfter.AutoHide || controlAfter.Cancelled)
                {
                    controlAfter = null;
                }
            }

            return controlAfter;
        }

        private void _toolStripButtonClose_Click(object sender, EventArgs e)
        {
            this.Cancelled = true;
        }

        private void _autoHidetoolStripButton_Click(object sender, EventArgs e)
        {
            this.AutoHide = !this.AutoHide;
        }

        private void SetupControlEvents(Control control)
        {
            if (control == null)
            {
                return;
            }
            else
            {
                // this is to track enabled state
                control.GotFocus += new EventHandler(control_GotFocus);
                control.LostFocus += new EventHandler(control_LostFocus);

                foreach (Control child in control.Controls)
                {
                    SetupControlEvents(child);
                }
            }
        }

        private void control_LostFocus(object sender, EventArgs e)
        {
            _toolStripLabel.Enabled = false;
            _toolStripCaption.Invalidate();
        }

        private void control_GotFocus(object sender, EventArgs e)
        {
            _toolStripLabel.Enabled = true;
            _toolStripCaption.Invalidate();
        }

        private void _manager_ToolStripRendererChanged(DockingManagerControl control, ToolStripRenderer newRenderer)
        {
            _toolStripCaption.Renderer = newRenderer;
        }

        private void _tabsToolStripButton_Click(object sender, EventArgs e)
        {
            if (_container != null)
            {
                _container.Tabbed = !_container.Tabbed;
            }
        }

        private void RemoveFloatingForm()
        {
            if (_floatingForm != null)
            {
                _autoHidetoolStripButton.Visible = true;
                _tabsToolStripButton.Visible = true;
                _floatingForm.Close();
                _floatingForm = null;                
            }
        }
    }
}
