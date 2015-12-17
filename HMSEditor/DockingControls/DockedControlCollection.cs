using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.Controls.Docking
{
    public interface IDockedControlCollection : IEnumerable<IDockingControl>
    {
        IDockingControl Add(string title, Control control);
        int Count { get; }
    }

    public class DockedControlCollection : IDockedControlCollection
    {
        private List<DockingControl> _dockedControls;
        private List<IDockingControl> _iDockedControls;

        private DockingManagerControl _manager;
        private DockControlContainer _container;

        public delegate void ControlAddedHandler(DockedControlCollection list, DockingControl control);
        public delegate void ControlRemoveHandler(DockedControlCollection list, IDockingControl control);        

        public event ControlAddedHandler ControlAdded;
        public event ControlRemoveHandler ControlRemoved;

        public DockedControlCollection(DockControlContainer container)
        {
            _container = container;
            _dockedControls = new List<DockingControl>();
            _iDockedControls = new List<IDockingControl>();
        }

        public DockingManagerControl Manager
        {
            get
            {
                return _manager;
            }

            set
            {
                _manager = value;
            }
        }

        public DockControlContainer DockControlContainer
        {
            get
            {
                return _container;
            }
        }

        public int GetVisibleDockedControlCount()
        {
            int count = 0;

            foreach (DockingControl control in _dockedControls)
            {
                if (!control.Cancelled && !control.AutoHide)
                {
                    count += 1;
                }
            }

            return count;
        }

        public List<DockingControl> GetVisibleDockedControls()
        {
            List<DockingControl> dockingControls = new List<DockingControl>();

            foreach (DockingControl control in _dockedControls)
            {
                if (!control.Cancelled && !control.AutoHide)
                {
                    dockingControls.Add(control);
                }
            }

            return dockingControls;
        }        

        #region IDockedControlList Members

        public IDockingControl Add(string title, Control control)
        {
            System.Diagnostics.Debug.Assert(_manager != null, "Manager has not been set");

            DockingControl dockingControl = new DockingControl(control, _manager);
            Add(dockingControl);
            
            dockingControl.Title = title;
            OnAddControl(dockingControl);
            return dockingControl;
        }

        public void Add(DockingControl dockingControl)
        {
            _dockedControls.Add(dockingControl);
            _iDockedControls.Add(dockingControl);
            dockingControl.DockingControlContainer = _container;
        }

        public void Remove(DockingControl dockingControl)
        {
            _dockedControls.Remove(dockingControl);
            _iDockedControls.Remove(dockingControl);
            dockingControl.DockingControlContainer = null;
        }

        public void RemoveAt(int index)
        {
            _dockedControls[index].DockingControlContainer = null;
            _dockedControls.RemoveAt(index);
            _iDockedControls.RemoveAt(index);
        }

        public void Insert(int index, DockingControl control)
        {
            _dockedControls.Insert(index, control);
            _iDockedControls.Insert(index, control);
            control.DockingControlContainer = _container;
        }

        public DockingControl this[int index]
        {
            get
            {
                return _dockedControls[index];
            }
        }

        public int IndexOf(DockingControl control)
        {
            return _dockedControls.IndexOf(control);
        }

        public int Count
        {
            get
            {
                return _dockedControls.Count;
            }
        }

        #endregion

        private IDockingControl Convert(DockingControl control)
        {
            return control;
        }

        #region IEnumerable<IDockingControl> Members

        public IEnumerator<IDockingControl> GetEnumerator()
        {
            return _iDockedControls.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dockedControls.GetEnumerator();
        }

        #endregion

        protected virtual void OnAddControl(DockingControl control)
        {
            if (ControlAdded != null)
            {
                ControlAdded(this, control);
            }
        }

        protected virtual void OnRemoveControl(IDockingControl control)
        {
            if (ControlRemoved != null)
            {
                ControlRemoved(this, control);
            }
        }
    }
}
