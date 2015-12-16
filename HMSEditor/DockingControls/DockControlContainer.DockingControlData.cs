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
    partial class DockControlContainer
    {
        private class DockingControlData
        {
            private ToolStripButton _toolStripButton;
            private AutoHideChangedHandler _autoHideHandler;
            private CancelledChangedHandler _cancelledChangedHandler;

            public DockingControlData(ToolStripButton button, AutoHideChangedHandler autoHideHandler, CancelledChangedHandler cancelledChangedHandler)
            {
                _toolStripButton = button;
                _autoHideHandler = autoHideHandler;
                _cancelledChangedHandler = cancelledChangedHandler;
            }

            public ToolStripButton Button
            {
                get
                {
                    return _toolStripButton;
                }
            }

            public AutoHideChangedHandler AutoHideChangedHandler
            {
                get
                {
                    return _autoHideHandler;
                }
            }

            public CancelledChangedHandler CancelledChangedHandler
            {
                get
                {
                    return _cancelledChangedHandler;
                }
            }
        }
    }
}
