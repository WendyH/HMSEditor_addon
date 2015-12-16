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
    public interface IDockingPanelCollection : IEnumerable<IDockingPanel>
    {
        IDockingPanel InsertPanel(int index);
        int Count { get; }
        IDockingPanel this[int index] { get; }
    }

    internal class DockControlContainerCollection : IDockingPanelCollection
    {
        private List<IDockingPanel> _panels;
        private DockingManagerControl _manager;
        private DockStyle _dockStyle;
        private MenuStrip _menuStrip;
        private ToolStripRenderer _renderer;

        public DockControlContainerCollection(DockingManagerControl manager, DockStyle dockStyle, MenuStrip menuStrip, ToolStripRenderer renderer)
        {
            _manager = manager;
            _dockStyle = dockStyle;
            _menuStrip = menuStrip;
            _renderer = renderer;

            _panels = new List<IDockingPanel>();
            InsertPanel(0);
        }

        public int GetContainerCount()
        {
            return _panels.Count;
        }

        public DockStyle DockStyle
        {
            get
            {
                return _dockStyle;
            }
        }

        public int IndexOf(DockControlContainer panel)
        {
            return _panels.IndexOf(panel);
        }

        public bool Contains(DockControlContainer panel)
        {
            return _panels.Contains(panel);
        }

        public void Remove(DockControlContainer container)
        {
            if (_panels.Count == 1)
            {
                throw new InvalidOperationException("You must have at least one panel in each direction");
            }
            else
            {
                if (container.DockedControlList.Count > 0)
                {
                    throw new InvalidOperationException("Can only remove empty panels");
                }
                else
                {
                    _panels.Remove(container);
                    container.Dispose();
                }
            }
        }

        #region IDockingPanelCollection Members

        public IDockingPanel InsertPanel(int index)
        {
            if (_panels.Count > index)
            {
                DockControlContainer existingContainer = _panels[index] as DockControlContainer;

                if (existingContainer.DockedControlList.Count == 0)
                {
                    return existingContainer;
                }
            }

            DockControlContainer container = new DockControlContainer();

            container.MenuStrip = _menuStrip;
            container.Dock = _dockStyle;
            container.Parent = _manager;

            _panels.Insert(index, container);

            container.Visible = true;
            _manager.PerformLayout();

            container.Visible = false;
            _manager.PerformLayout();

            return container;
        }

        public int Count
        {
            get
            {
                return _panels.Count;
            }
        }

        public IDockingPanel this[int index]
        {
            get
            {                
                return _panels[index];
            }
        }

        #endregion

        #region IEnumerable<IDockingPanel> Members

        public IEnumerator<IDockingPanel> GetEnumerator()
        {
            return _panels.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            IEnumerable enumerable = _panels as IEnumerable;
            return enumerable.GetEnumerator();
        }

        #endregion                
    }
}
