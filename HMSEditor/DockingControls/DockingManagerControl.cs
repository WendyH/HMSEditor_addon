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
    public delegate void ToolStripRendererChangedHandler(DockingManagerControl sender, ToolStripRenderer newRenderer);

    public interface IDockingPanels
    {
        IDockingPanelCollection this[DockingType type] { get; }
    }

    public partial class DockingManagerControl : UserControl
    {
        private DockingPanels _panels;
        private ToolStripRenderer _toolStripRenderer;
        private List<IDockingControl> _dockingControls;
        private DockControlContainerCollection _leftDockControlContainers;
        private DockControlContainerCollection _rightDockControlContainers;
        private DockControlContainerCollection _topDockControlContainers;
        private DockControlContainerCollection _bottomDockControlContainers;
        private bool _initialised;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event ToolStripRendererChangedHandler ToolStripRendererChanged;

        public DockingManagerControl()
        {
            _toolStripRenderer = new ToolStripProfessionalRenderer();
            _dockingControls = new List<IDockingControl>();
            _panels = new DockingPanels(this);

            InitializeComponent();
            InitializeControls();
            
            _initialised = true;
        }

        public IDockingPanels Panels
        {
            get
            {
                return _panels;
            }
        }

        public IEnumerable<IDockingControl> DockingControls
        {
            get
            {
                return _dockingControls;
            }
        }

        internal Rectangle DockingBounds
        {
            get
            {
                Rectangle bounds = this.ClientRectangle;

                if (_leftMenuStrip.Visible)
                {
                    bounds.X = _leftMenuStrip.Right;
                    bounds.Width = this.ClientRectangle.Right - bounds.X;
                }

                if (_topMenuStrip.Visible)
                {
                    bounds.Y = _topMenuStrip.Bottom;
                    bounds.Height = this.ClientRectangle.Bottom - bounds.Y;
                }

                if (_rightMenuStrip.Visible)
                {
                    bounds.Width = _rightMenuStrip.Left - bounds.X;
                }

                if (_bottomMenuStrip.Visible)
                {
                    bounds.Height = _bottomMenuStrip.Top - bounds.Y;
                }

                return bounds;
            }
        }

        public ToolStripRenderer Renderer
        {
            get
            {
                return _toolStripRenderer;
            }

            set
            {
                _toolStripRenderer = value;
                OnToolStripRendererChanged(value);
            }
        }

        internal DockControlContainer GetContainerAtPoint(Point point)
        {
            DockControlContainer container = null;

            container = GetContainerAtPoint(_leftDockControlContainers, point);
            container = (container == null) ? GetContainerAtPoint(_rightDockControlContainers, point) : container;
            container = (container == null) ? GetContainerAtPoint(_topDockControlContainers, point) : container;
            container = (container == null) ? GetContainerAtPoint(_bottomDockControlContainers, point) : container;

            return container;
        }

        internal void AddDockingControlToList(DockingControl dockingControl)
        {
            _dockingControls.Add(dockingControl);
        }

        protected virtual void OnToolStripRendererChanged(ToolStripRenderer renderer)
        {
            SetMenuRenderers(renderer);

            if (ToolStripRendererChanged != null)
            {
                ToolStripRendererChanged(this, renderer);
            }

            if (this.Visible)
            {
                this.Invalidate(true);
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            if (_initialised)
            {
                LayoutControls();
            }

            base.OnLayout(e);
        }

        private void InitializeControls()
        {
            _leftDockControlContainers = new DockControlContainerCollection(this, DockStyle.Left, _leftMenuStrip, _toolStripRenderer);
            _rightDockControlContainers = new DockControlContainerCollection(this, DockStyle.Right, _rightMenuStrip, _toolStripRenderer);
            _topDockControlContainers = new DockControlContainerCollection(this, DockStyle.Top, _topMenuStrip, _toolStripRenderer);
            _bottomDockControlContainers = new DockControlContainerCollection(this, DockStyle.Bottom, _bottomMenuStrip, _toolStripRenderer);

            LayoutControls();
        }

        private void SetMenuRenderers(ToolStripRenderer renderer)
        {
            _leftMenuStrip.Renderer = renderer;
            _rightMenuStrip.Renderer = renderer;
            _bottomMenuStrip.Renderer = renderer;
            _topMenuStrip.Renderer = renderer;
        }

        private void LayoutControls(DockControlContainerCollection container)
        {
            for (int index = 0; index < container.Count; index += 1)
            {
                DockControlContainer control = container[index] as DockControlContainer;
                control.SendToBack();                
            }
        }

        private void LayoutControls()
        {
            LayoutControls(_leftDockControlContainers);
            LayoutControls(_rightDockControlContainers);
            LayoutControls(_topDockControlContainers);
            LayoutControls(_bottomDockControlContainers);

            _leftMenuStrip.SendToBack();
            _rightMenuStrip.SendToBack();
            _topMenuStrip.SendToBack();
            _bottomMenuStrip.SendToBack();
        }

        private DockControlContainer GetContainerAtPoint(DockControlContainerCollection containers, Point point)
        {
            foreach (DockControlContainer container in containers)
            {
                if (container.Visible && container.Bounds.Contains(point))
                {
                    return container;
                }
            }

            return null;
        }

        private class DockingPanels : IDockingPanels
        {
            private DockingManagerControl _manager;

            public DockingPanels(DockingManagerControl manager)
            {
                _manager = manager;
            }

            #region IDockingPanels Members

            public IDockingPanelCollection this[DockingType type]
            {
                get
                {
                    switch (type)
                    {
                        case DockingType.Left:
                            return _manager._leftDockControlContainers;

                        case DockingType.Right:
                            return _manager._rightDockControlContainers;

                        case DockingType.Top:
                            return _manager._topDockControlContainers;

                        case DockingType.Bottom:
                            return _manager._bottomDockControlContainers;

                        default:
                            throw new ArgumentException("Invalid docking type");
                    }
                }
            }

            #endregion
        }
    }
}
