using System;
using System.Collections.Generic;
using System.Text;

namespace Darwen.Windows.Forms.Controls.Docking
{
    internal interface IDockControlContainerHandler
    {
        void OnDockingControlAdded(DockingControl control, int index);
        void OnDockingControlRemoved(DockingControl control);
        void OnDockingControlAutoHide(DockingControl control);
        void OnDockingControlCancelled(DockingControl control);
        void OnDockingControlMoved(DockingControl control, int oldIndex, int newIndex);

        void RemoveAllControls();
        void LayoutControls();
    }
}
