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
    internal class SplitterDockControlContainerHandler : IDockControlContainerHandler
    {
        private List<Splitter> _splitters;
        
        private DockControlContainer _container;

        public SplitterDockControlContainerHandler(DockControlContainer container)
        {
            _container = container;

            _splitters = new List<Splitter>();
        }

        public void OnDockingControlAdded(DockingControl control, int index)
        {
            if (!_container.Controls.Contains(control))
            {
                _container.Controls.Add(control);
            }

            RepositionControls();
        }

        public void OnDockingControlRemoved(DockingControl control)
        {
            _container.Controls.Remove(control);
            RepositionControls();
        }

        public void OnDockingControlAutoHide(DockingControl control)
        {
            if (control.AutoHide)
            {
                _container.Controls.Remove(control);                
            }
            else
            {
                OnDockingControlAdded(control, 0);
            }

            RepositionControls();
        }

        public void OnDockingControlCancelled(DockingControl control)
        {
            if (control.Cancelled)
            {
                _container.Controls.Remove(control);                
            }
            else 
            {
                OnDockingControlAdded(control, 0);
            }

            RepositionControls();
        }

        public void OnDockingControlMoved(DockingControl control, int oldIndex, int newIndex)
        {
            RepositionControls();
        }

        public void RemoveAllControls()
        {            
            List<Control> controlsToRemove = new List<Control>();
            foreach (Control control in _container.Controls)
            {
                if (control is DockingControl || control is Splitter)
                {
                    controlsToRemove.Add(control);
                }
            }

            foreach (Control control in controlsToRemove)
            {
                _container.Controls.Remove(control);
            }

            _splitters.Clear();
        }

        public void LayoutControls()
        {
            RepositionControls();
        }

        private void RepositionControls()
        {
            List<DockingControl> nonAutoHideDockingControls = _container.DockedControlList.GetVisibleDockedControls();

            if (nonAutoHideDockingControls.Count > 0)
            {
                RemoveUnusedSplitters(nonAutoHideDockingControls);
                AddRequiredSplitters(nonAutoHideDockingControls);
                SetupZOrder(nonAutoHideDockingControls);
                AssignDockStyles(nonAutoHideDockingControls);
                ResizeControls(nonAutoHideDockingControls);
                _container.PerformLayout();
            }
        }

        private void RemoveUnusedSplitters(List<DockingControl> dockingControls)
        {
            while (_splitters.Count > dockingControls.Count - 1)
            {
                Splitter splitter = _splitters[_splitters.Count - 1];
                _splitters.Remove(splitter);
                _container.Controls.Remove(splitter);
                splitter.Parent = null;
                splitter.Dispose();
            }
        }

        private void ResizeControls(List<DockingControl> dockingControls)
        {
            Rectangle clientRectangle = _container.Bounds;

            switch (_container.Dock)
            {
                case DockStyle.Left:
                case DockStyle.Right:
                    int dockingControlHeight = clientRectangle.Height / dockingControls.Count;
                    int yPosition = clientRectangle.Y;

                    foreach (DockingControl control in dockingControls)
                    {
                        control.Height = dockingControlHeight;
                        yPosition += dockingControlHeight;
                    }
                    break;

                case DockStyle.Top:
                case DockStyle.Bottom:
                    int dockingControlWidth = clientRectangle.Width / dockingControls.Count;
                    int xPosition = clientRectangle.X;

                    foreach (DockingControl control in dockingControls)
                    {
                        control.Width = dockingControlWidth;
                        xPosition += dockingControlWidth;
                    }
                    break;
            }
        }

        private void SetupZOrder(List<DockingControl> dockingControls)
        {
            System.Diagnostics.Debug.Assert(dockingControls.Count == _splitters.Count + 1);

            for (int dockingControlIndex = 0; dockingControlIndex < dockingControls.Count; dockingControlIndex += 1)
            {
                DockingControl control = dockingControls[dockingControlIndex];
                control.BringToFront();

                if (dockingControlIndex < _splitters.Count)
                {
                    Splitter splitter = _splitters[dockingControlIndex];
                    splitter.BringToFront();
                }
            }
        }

        private void AssignDockStyles(List<DockingControl> dockingControls)
        {
            DockStyle dockingControlDockStyle = DockStyle.None;

            int dockingControlIndex = 0;

            switch (_container.Dock)
            {
                case DockStyle.Left:
                case DockStyle.Right:
                    dockingControlDockStyle = DockStyle.Top;
                    break;

                case DockStyle.Top:
                case DockStyle.Bottom:
                    dockingControlDockStyle = DockStyle.Left;
                    break;
            }

            foreach (DockingControl dockingControl in dockingControls)
            {
                if (dockingControlIndex == dockingControls.Count - 1)
                {
                    dockingControl.Dock = DockStyle.Fill;
                }
                else
                {
                    dockingControl.Dock = dockingControlDockStyle;
                }

                dockingControlIndex += 1;
            }
        }

        private void AddRequiredSplitters(List<DockingControl> dockingControls)
        {
            while (_splitters.Count < dockingControls.Count - 1)
            {
                Splitter splitter = new Splitter();

                switch (_container.Dock)
                {
                    case DockStyle.Top:
                    case DockStyle.Bottom:
                        splitter.Dock = DockStyle.Left;
                        splitter.Width = DockControlContainer.SplitterWidth;
                        break;

                    case DockStyle.Left:
                    case DockStyle.Right:
                        splitter.Dock = DockStyle.Top;
                        splitter.Height = DockControlContainer.SplitterWidth;
                        break;
                }

                splitter.Visible = true;
                _container.Controls.Add(splitter);
                _splitters.Add(splitter);
            }
        }
    }    
}
