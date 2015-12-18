using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Darwen.Windows.Forms.General;

namespace Darwen.Windows.Forms.Controls.Docking
{
    public interface IDockingPanel
    {
        IDockedControlCollection DockedControls { get; }
        int Dimension { get; set; }
        bool Tabbed { get; set; }
        void LayoutControls();
    }

    public partial class DockControlContainer : UserControl, IDockingPanel
    {
        private DockedControlCollection _dockingControls;
        private Dictionary<ToolStripButton, AutoResizeControl> _mapButtonToControl;
        private Dictionary<DockingControl, DockingControlData> _mapDockingControlToData;
        private MenuStrip _menuStrip;
        private bool _inExternalAddControl;
        private IDockControlContainerHandler _dockControlContainerHandler;
        private bool _tabbed;
        private DragHandler _dragHandler;
        private SplitterDragHandler _splitterDragHandler;
        private SetCursor _setDragCursor;
        private PaddingDrawHandler _paddingDrawHandler;
        private const int MaxInitialDockSize = 500;
        public const int SplitterWidth = 5;
        private const int PaddingWidth = 5;
        
        public DockControlContainer()
        {
            InitializeComponent();
            InitializeControlList();

            _dockControlContainerHandler = new SplitterDockControlContainerHandler(this);
            _dragHandler = new DragHandler(this, new DragHandlerHitTester(HitTestSizeArea));
            _splitterDragHandler = new SplitterDragHandler(_dragHandler, SplitterDragHandlerDirection.EastWest);
            _splitterDragHandler.EndSplitterDrag += new SplitterDragEndHandler(_splitterDragHandler_EndSplitterDrag);
            _paddingDrawHandler = new PaddingDrawHandler(this);
        }

        public IDockedControlCollection DockedControls
        {
            get
            {
                return _dockingControls;
            }
        }

        public MenuStrip MenuStrip
        {
            get
            {
                return _menuStrip;
            }

            set
            {
                _menuStrip = value;
            }
        }

        public bool Tabbed
        {
            get
            {
                return _tabbed;
            }

            set
            {
                if (_tabbed != value)
                {
                    _tabbed = value;
                    SwitchTabbedStatus();
                }
            }
        }

        public int Dimension
        {
            get
            {
                int size = 0;

                switch (this.Dock)
                {
                    case DockStyle.Bottom:
                        size = this.Height;
                        break;
                    case DockStyle.Fill:
                        break;
                    case DockStyle.Left:
                        size = this.Width;
                        break;
                    case DockStyle.None:
                        break;
                    case DockStyle.Right:
                        size = this.Width;
                        break;
                    case DockStyle.Top:
                        size = this.Height;
                        break;
                    default:
                        break;
                }

                return size;
            }

            set
            {
                switch (this.Dock)
                {
                    case DockStyle.Bottom:
                        this.Height = value;
                        break;
                    case DockStyle.Fill:
                        break;
                    case DockStyle.Left:
                        this.Width = value;
                        break;
                    case DockStyle.None:
                        break;
                    case DockStyle.Right:
                        this.Width = value;
                        break;
                    case DockStyle.Top:
                        this.Height = value;
                        break;
                    default:
                        break;
                }
            }
        }

        public DockedControlCollection DockedControlList
        {
            get
            {
                return _dockingControls;
            }
        }    

        public void DockControl(DockingControl control, int dockAtIndex, int dimension)
        {
            using (RedrawStopper stopRedraws = new RedrawStopper(this))
            {
                DockControlContainer oldContainer = ControlHelpers.FindParentControl<DockControlContainer>(control);
                
                if (oldContainer != null && oldContainer != this)
                {
                    oldContainer.RemoveControl(control);
                }

                AddControl(control, dockAtIndex, dimension);
            }

            Invalidate(true);
        }

        public int GetDockControlIndex(DockingControl control)
        {
            return _dockingControls.IndexOf(control);
        }

        public void RemoveControl(DockingControl control)
        {
            Manager.SuspendLayout();

            DockingControlData data = _mapDockingControlToData[control];
            control.AutoHideChanged -= data.AutoHideChangedHandler;
            control.CancelledChanged -= data.CancelledChangedHandler;

            _mapDockingControlToData.Remove(control);

            if (data.Button != null)
            {
                _mapButtonToControl.Remove(data.Button);
            }

            _dockingControls.Remove(control);
            OnDockingControlRemoved(control);

            CheckVisibility();

            Manager.ResumeLayout(true);
        }

        public void LayoutControls()
        {
            _dockControlContainerHandler.LayoutControls();
        }

        public DockingControl GetDockingControlAtPoint(Point point)
        {
            foreach (DockingControl control in _dockingControls)
            {
                if (!control.AutoHide && !control.Cancelled)
                {
                    Rectangle bounds = control.Bounds;

                    switch (this.Dock)
                    {
                        case DockStyle.Bottom:
                            bounds.Width += DockControlContainer.SplitterWidth;
                            break;
                        case DockStyle.Left:
                            bounds.Height += DockControlContainer.SplitterWidth;
                            break;
                        case DockStyle.Right:
                            bounds.Height += DockControlContainer.SplitterWidth;
                            break;
                        case DockStyle.Top:
                            bounds.Width += DockControlContainer.SplitterWidth;
                            break;
                        default:
                            break;
                    }

                    if (bounds.Contains(point))
                    {
                        return control;
                    }
                }
            }

            return null;         
        }

        protected DockingManagerControl Manager
        {
            get
            {
                return _dockingControls.Manager;
            }
        }

        protected override void OnDockChanged(EventArgs e)
        {
            base.OnDockChanged(e);

            SetSplitterDirection();
            SetPadding();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            _dockingControls.Manager = ControlHelpers.FindParentControl<DockingManagerControl>(this);;
        }

        private void DockingControlMovedHelper(DockingControl control, int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex)
            {
                _dockingControls.RemoveAt(oldIndex);
                _dockingControls.Insert((newIndex > oldIndex) ? newIndex - 1 : newIndex, control);
                
                if (!control.AutoHide && !control.Cancelled)
                {
                    OnDockingControlMoved(control, oldIndex, newIndex);
                }
            }
        }

        protected virtual void OnDockingControlRemoved(DockingControl control)
        {
            if (!control.Cancelled)
            {
                _dockControlContainerHandler.OnDockingControlRemoved(control);
            }
        }

        protected virtual void OnDockingControlAdded(DockingControl control, int index)
        {
            if (!control.Cancelled && !control.AutoHide)
            {
                _dockControlContainerHandler.OnDockingControlAdded(control, index);
            }
        }

        protected virtual void OnDockingControlMoved(DockingControl control, int oldIndex, int newIndex)
        {
            if (!control.Cancelled && !control.AutoHide)
            {
                _dockControlContainerHandler.OnDockingControlMoved(control, oldIndex, newIndex);
            }
        }

        protected virtual void OnDockingControlAutoHide(DockingControl control)
        {
            if (!control.Cancelled)
            {
                _dockControlContainerHandler.OnDockingControlAutoHide(control);
            }
        }

        protected virtual void OnDockingControlCancelled(DockingControl control)
        {
            if (!control.AutoHide)
            {
                _dockControlContainerHandler.OnDockingControlCancelled(control);
            }
        }
        
        private void InitializeControlList()
        {
            _dockingControls = new DockedControlCollection(this);
            _mapButtonToControl = new Dictionary<ToolStripButton, AutoResizeControl>();
            _mapDockingControlToData = new Dictionary<DockingControl, DockingControlData>();

            _dockingControls.ControlAdded += new DockedControlCollection.ControlAddedHandler(_controlList_ControlAdded);
        }

        private void AddControlHelper(DockingControl control, int index)
        {
            int controlIndex = _dockingControls.IndexOf(control);

            if (!_inExternalAddControl && controlIndex >= 0)
            {
                if (controlIndex == index)
                {
                    control.Child.Focus();
                    control.Child.Select();
                    return;
                }
                else
                {
                    DockingControlMovedHelper(control, controlIndex, index);
                }
            }
            else
            {
                if (controlIndex >= 0)
                {
                    if (index >= controlIndex)
                    {
                        index -= 1;
                    }
                }

                _dockingControls.Remove(control);
                _dockingControls.Insert(index, control);
                OnDockingControlAdded(control, index);
            }

            control.Child.Select();
            Manager.PerformLayout();
        }        

        private void AddControl(object sender, int dockIndex, int dimension)
        {
            DockingControl control = sender as DockingControl;
            if (control == null) return;
            Manager.SuspendLayout();

            if (!_mapDockingControlToData.ContainsKey(control))
            {
                DockingControlData dockingControlData = new DockingControlData(null,
                    new AutoHideChangedHandler(DockingControl_AutoHideChanged),
                    new CancelledChangedHandler(DockingControl_CancelledChanged));
            
                _mapDockingControlToData.Add(control, dockingControlData);

                control.AutoHideChanged += dockingControlData.AutoHideChangedHandler;
                control.CancelledChanged += dockingControlData.CancelledChangedHandler;            
            }

            switch (Dock)
            {
                case DockStyle.Left:
                case DockStyle.Right:
                    if (_dockingControls.Count == 0)
                    {
                        this.Width = Math.Min(MaxInitialDockSize, PaddingWidth + dimension);
                    }                    
                    
                    break;

                case DockStyle.Top:
                case DockStyle.Bottom:
                    if (_dockingControls.Count == 0)
                    {
                        this.Height = Math.Min(MaxInitialDockSize, PaddingWidth + dimension);
                    }
                    
                    break;
            }

            if (dockIndex < 0 || dockIndex >= _dockingControls.Count)
            {
                AddControlHelper(control, _dockingControls.Count);
            }
            else
            {
                AddControlHelper(control, dockIndex);
            }

            Manager.ResumeLayout(true);

            CheckVisibility();            
        }

        private void _controlList_ControlAdded(DockedControlCollection list, DockingControl control)
        {
            _inExternalAddControl = true;
            AddControl(control, -1, DockControlHelpers.GetDockedDimension(control, this.Dock));
            _inExternalAddControl = false;
        }

        private void DockingControl_AutoHideChanged(object sender, EventArgs e)
        {
            DockingControl control = sender as DockingControl;
            Manager.SuspendLayout();
                
            if (control.AutoHide)
            {
                AutoHideControl(control);
            }
            else
            {
                ShowControl(control);
            }

            Manager.ResumeLayout(true); 
        }

        private void ShowControl(DockingControl control)
        {
            Form parentForm = this.ParentForm;
            
            using (RedrawStopper stopRedraws = new RedrawStopper(parentForm))
            {
                Manager.SuspendLayout();

                switch (this.Dock)
                {
                    case DockStyle.Left:
                    case DockStyle.Right:
                        this.Width = Math.Max(this.Width, PaddingWidth + control.Width);
                        break;

                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        this.Height = Math.Max(this.Height, PaddingWidth + control.Height);
                        break;
                }

                this.Visible = true;

                control.Parent = this;

                ToolStripButton button = _mapDockingControlToData[control].Button;

                if (button != null)
                {
                    _menuStrip.Items.Remove(button);

                    AutoResizeControl autoResizeControl = _mapButtonToControl[button];
                    this.Controls.Remove(autoResizeControl);
                    autoResizeControl.Dispose();

                    _mapButtonToControl.Remove(button);
                }

                DockingControlData oldData = _mapDockingControlToData[control];
                _mapDockingControlToData[control] = new DockingControlData(null, oldData.AutoHideChangedHandler, oldData.CancelledChangedHandler);

                if (ToolStripHelpers.GetVisibleButtonCount(_menuStrip) == 0)
                {
                    _menuStrip.Visible = false;
                }

                _dockingControls.Remove(control);
                _dockingControls.Add(control);
                OnDockingControlAutoHide(control);

                if (control.Cancelled)
                {
                    CancelControl(control);
                }
                
                Manager.ResumeLayout(true);

                control.DraggingEnabled = true;
            }

            control.Focus();
            control.Select();

            if (parentForm != null)
            {
                parentForm.Invalidate(true);
            }            
        }

        private void AutoHideControl(DockingControl control)
        {
            DockingControlData oldData = _mapDockingControlToData[control];

            if (oldData.Button == null)
            {
                Manager.SuspendLayout();

                _menuStrip.Visible = true;

                MyToolStripButton button = new MyToolStripButton(control.Title);
                // < By WendyH ----------------------------------------------
                button.Font = new Font("Serif", 9.00f, FontStyle.Regular, GraphicsUnit.Point);
                button.Padding = new Padding(5);
                // < By WendyH ----------------------------------------------
                button.MouseHover += new EventHandler(button_MouseHover);
                button.Click += new EventHandler(button_Click);
                button.MouseEnter += new EventHandler(button_MouseEnter);
                button.MouseLeave += new EventHandler(button_MouseLeave);
                button.Visible = !control.Cancelled;
                _menuStrip.Items.Add(button);

                if (_dockingControls.GetVisibleDockedControlCount() == 0)
                {
                    this.Visible = false;
                }

                AutoResizeControl autoResizeControl = new AutoResizeControl(control, this);
                autoResizeControl.Visible = false;
                autoResizeControl.Parent = this.Parent;

                switch (this.Dock)
                {
                    case DockStyle.Bottom:
                        autoResizeControl.DockDirection = DockingType.Bottom;
                        autoResizeControl.Padding = new Padding(0, PaddingWidth, 0, 0);
                        break;
                    case DockStyle.Fill:
                        break;
                    case DockStyle.Left:
                        autoResizeControl.DockDirection = DockingType.Left;
                        autoResizeControl.Padding = new Padding(0, 0, PaddingWidth, 0);
                        break;
                    case DockStyle.None:
                        break;
                    case DockStyle.Right:
                        autoResizeControl.DockDirection = DockingType.Right;
                        autoResizeControl.Padding = new Padding(PaddingWidth, 0, 0, 0);
                        break;
                    case DockStyle.Top:
                        autoResizeControl.DockDirection = DockingType.Top;
                        autoResizeControl.Padding = new Padding(0, 0, 0, PaddingWidth);
                        break;
                    default:
                        break;
                }

                _mapButtonToControl.Add(button, autoResizeControl);
                _mapDockingControlToData[control] = new DockingControlData(button, oldData.AutoHideChangedHandler, oldData.CancelledChangedHandler);

                control.DraggingEnabled = false;
                control.Visible = true;

                OnDockingControlAutoHide(control);

                Manager.ResumeLayout(true);                
            }            
        }

        private void button_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            AutoResizeControl control = _mapButtonToControl[button];

            if (!control.Showing)
            {
                control.Show(false);
                control.BringToFront();
                control.Focus();
                control.StartTrackMouse();
            }            
        }

        private void button_MouseHover(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            AutoResizeControl control = _mapButtonToControl[button];

            if (!control.Visible && !control.Showing)
            {
                control.BringToFront();

                foreach (Control child in Parent.Controls)
                {
                    if (child is MenuStrip)
                    {
                        control.BringToFront();
                    }
                }

                control.Show(true);
            }
        }

        private void button_MouseLeave(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            AutoResizeControl control = _mapButtonToControl[button];

            if (control.Visible)
            {
                control.StartTrackMouse();
            }
        }

        private void button_MouseEnter(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;
            AutoResizeControl control = _mapButtonToControl[button];

            if (control.Visible)
            {
                control.StopTrackMouse();
            }
        }

        private void CancelControl(DockingControl control)
        {
            DockingControlData data = _mapDockingControlToData[control];

            if (control.Cancelled)
            {
                if (data.Button == null)
                {
                    if (_dockingControls.GetVisibleDockedControlCount() == 0)
                    {
                        this.Visible = false;
                    }
                }
                else
                {
                    ToolStripButton button = _mapDockingControlToData[control].Button;
                    button.Visible = false;

                    AutoResizeControl autoResizeControl = _mapButtonToControl[button];
                    autoResizeControl.Visible = false;

                    if (ToolStripHelpers.GetVisibleButtonCount(_menuStrip) == 0)
                    {
                        _menuStrip.Visible = false;
                    }
                }        
            }
            else
            {
                ToolStripButton button = _mapDockingControlToData[control].Button;

                if (button == null)
                {
                    this.Visible = true;
                }
                else
                {
                    button.Visible = true;
                    _menuStrip.Visible = true;                    
                }
            }
            
        }

        private void DockingControl_CancelledChanged(object sender, EventArgs e)
        {
            DockingControl control = sender as DockingControl;
            if (control == null) return;
            using (RedrawStopper stopRedraws = new RedrawStopper(this.Parent, true))
            {
                Manager.SuspendLayout();

                CancelControl(control);

                OnDockingControlCancelled(control);

                Manager.ResumeLayout(true);
            }            
        }

        private void SwitchTabbedStatus()
        {
            using (RedrawStopper stopRedraws = new RedrawStopper(this, true))
            {
                IDockControlContainerHandler newHandler = null;

                if (this.Tabbed)
                {
                    newHandler = new TabbedDockControlContainerHandler(this);
                }
                else
                {
                    newHandler = new SplitterDockControlContainerHandler(this);
                }

                _dockControlContainerHandler.RemoveAllControls();

                foreach (DockingControl control in _dockingControls)
                {
                    if (!control.AutoHide)
                    {
                        newHandler.OnDockingControlAdded(control, -1);

                        if (control.Cancelled)
                        {
                            newHandler.OnDockingControlCancelled(control);
                        }
                    }                    
                }

                _dockControlContainerHandler = newHandler;
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
                switch (this.Dock)
                {
                    case DockStyle.Left:
                    case DockStyle.Right:
                        _setDragCursor = new SetCursor(this, Cursors.VSplit);
                        break;

                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        _setDragCursor = new SetCursor(this, Cursors.HSplit);
                        break;

                    default:
                        break;
                }
            }
        }        

        private void CheckVisibility()
        {
            if (_dockingControls.GetVisibleDockedControlCount() > 0)
            {
                this.Visible = true;
            }
            else
            {                
                this.Visible = false;
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

        private void _splitterDragHandler_EndSplitterDrag(SplitterDragHandler sender, int size)
        {
            using (RedrawStopper stopRedraws = new RedrawStopper(this.ParentForm, true))
            {
                switch (this.Dock)
                {
                    case DockStyle.Bottom:                        
                        this.Height -= size;
                        this.Height = Math.Max(this.MinimumSize.Height, this.Height);
                        break;
                    case DockStyle.Left:
                        this.Width += size;
                        this.Width = Math.Max(this.MinimumSize.Width, this.Width);
                        break;
                    case DockStyle.Right:
                        this.Width -= size;
                        this.Width = Math.Max(this.MinimumSize.Width, this.Width);
                        break;
                    case DockStyle.Top:
                        this.Height += size;
                        this.Height = Math.Max(this.MinimumSize.Height, this.Height);
                        break;
                    default:
                        break;
                }
            }
        }

        private void SetPadding()
        {
            switch (this.Dock)
            {
                case DockStyle.Bottom:
                    this.Padding = new Padding(0, 5, 0, 0);
                    break;
                case DockStyle.Left:
                    this.Padding = new Padding(0, 0, 5, 0);
                    break;
                case DockStyle.Right:
                    this.Padding = new Padding(5, 0, 0, 0);
                    break;
                case DockStyle.Top:
                    this.Padding = new Padding(0, 0, 0, 5);
                    break;
                default:
                    break;
            }
        }

        private void SetSplitterDirection()
        {
            switch (this.Dock)
            {
                case DockStyle.Top:
                case DockStyle.Bottom:
                    _splitterDragHandler.Direction = SplitterDragHandlerDirection.NorthSouth;
                    break;
                case DockStyle.Right:
                case DockStyle.Left:
                    _splitterDragHandler.Direction = SplitterDragHandlerDirection.EastWest;
                    break;
                default:
                    break;
            }
        }        
    }    

    public class MyToolStripButton : ToolStripButton {
        public MyToolStripButton(string title) {
            base.Text = title;
        }

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            base.OnPaint(e);
        }
    }

}