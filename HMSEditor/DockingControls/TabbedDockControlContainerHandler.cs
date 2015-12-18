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
    internal class TabbedDockControlContainerHandler : IDockControlContainerHandler, IDisposable
    {
        private DockControlContainer _container;
        private TabControl _tabControl;
        private Dictionary<DockingControl, TabPage> _dockingControlToPageMap;
        
        public TabbedDockControlContainerHandler(DockControlContainer container)
        {
            _container = container;

            _dockingControlToPageMap = new Dictionary<DockingControl, TabPage>();

            _tabControl = new TabControl();
            _tabControl.Dock = DockStyle.Fill;
            _tabControl.Alignment = TabAlignment.Bottom;
            _tabControl.Parent = _container;

            _tabControl.SelectedIndexChanged += new EventHandler(_tabControl_SelectedIndexChanged);
        }

        public void OnDockingControlAdded(DockingControl control, int index)
        {
            AddTabControl(control, index);
        }

        public void OnDockingControlRemoved(DockingControl control)
        {
            RemoveTabControl(control);            
        }

        public void OnDockingControlAutoHide(DockingControl control)
        {
            if (control.AutoHide)
            {
                RemoveTabControl(control);                
            }
            else 
            {
                AddTabControl(control, -1);
            }           
        }

        public void OnDockingControlCancelled(DockingControl control)
        {
            if (control.Cancelled)
            {
                RemoveTabControl(control);                
            }
            else 
            {
                AddTabControl(control, -1);
            }
        }

        public void OnDockingControlMoved(DockingControl control, int oldIndex, int newIndex)
        {
            RemoveTabControl(control);
            AddTabControl(control, (oldIndex <= newIndex) ? newIndex : newIndex - 1);            
        }

        public void RemoveAllControls()
        {
            _container.Controls.Remove(_tabControl);
            _tabControl.TabPages.Clear();
            _dockingControlToPageMap.Clear();
        }

        private void AddTabControl(DockingControl control, int index)
        {
            System.Diagnostics.Debug.Assert(!control.AutoHide, "Can't dock auto hidden controls");

            TabPage page = null;

            if (_dockingControlToPageMap.ContainsKey(control))
            {
                System.Diagnostics.Debug.Fail("Can't add a tab page multiple times");
            }
            else
            {
                page = new TabPage(control.Title);
                _dockingControlToPageMap.Add(control, page);
            }

            if (index < 0 || index >= _tabControl.TabPages.Count)
            {
                _tabControl.TabPages.Add(page);
            }
            else
            {
                _tabControl.TabPages.Insert(index, page);
            }

            _tabControl.SelectedTab = page;

            control.Parent = page;
            control.Dock = DockStyle.Fill;            
        }

        private void RemoveTabControl(DockingControl control)
        {
            TabPage page = _dockingControlToPageMap[control];
            _tabControl.TabPages.Remove(page);
            _dockingControlToPageMap.Remove(control);
        }

        private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (DockingControl control in _dockingControlToPageMap.Keys)
            {
                if (_dockingControlToPageMap[control] == _tabControl.SelectedTab)
                {
                    control.Focus();
                    break;
                }
            }
        }

        public void LayoutControls()
        {
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _tabControl.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion
    }    
}
