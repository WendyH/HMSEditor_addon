using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.Controls.TabbedDocuments
{
    internal sealed class TabbedDocumentControlActiveFilesContextMenuHandler
    {
        private Dictionary<ToolStripMenuItem, Control> _mapContextMenuToControl;
        private TabbedDocumentControl _tabbedDocumentControl;

        public TabbedDocumentControlActiveFilesContextMenuHandler(TabbedDocumentControl tabbedDocumentControl)
        {
            _tabbedDocumentControl = tabbedDocumentControl;
            _mapContextMenuToControl = new Dictionary<ToolStripMenuItem, Control>();            
        }

        public void Show(Rectangle screenRectOfButton)
        {
            ContextMenuStrip menuStrip = new ContextMenuStrip();
            _mapContextMenuToControl.Clear();

            foreach (Control control in _tabbedDocumentControl.Items)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(control.Name);
                menuStrip.Items.Add(menuItem);
                menuItem.Click += new EventHandler(ContextMenuItem_Click);
                _mapContextMenuToControl.Add(menuItem, control);
            }

            menuStrip.Show(screenRectOfButton.Left, screenRectOfButton.Bottom);            
        }

        private void ContextMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            Control control = _mapContextMenuToControl[menuItem];
            _tabbedDocumentControl.SelectControl(control, true);                        
        }
    }
}
