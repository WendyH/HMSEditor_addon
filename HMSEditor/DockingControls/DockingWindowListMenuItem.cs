using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.Controls.Docking
{
    internal class DockingWindowMenuItem : ToolStripMenuItem
    {
        private IDockingControl _control;

        public DockingWindowMenuItem(IDockingControl control)
            : base(control.Title)
        {
            _control = control;
        }

        public IDockingControl Control
        {
            get
            {
                return _control;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            _control.Cancelled = !_control.Cancelled;
        }
    }

    public class DockingWindowListMenuItem : ToolStripMenuItem
    {
        public DockingWindowListMenuItem()
        {
            //Text = "Toolbars";            
        }

        public void Initialise(DockingManagerControl manager)
        {
            foreach (IDockingControl control in manager.DockingControls)
            {
                ToolStripMenuItem item = new DockingWindowMenuItem(control);
                this.DropDownItems.Add(item);
            }
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            base.OnDropDownOpened(e);

            foreach (DockingWindowMenuItem item in this.DropDownItems)
            {
                item.Checked = !item.Control.Cancelled;
            }
        }
    }
}
