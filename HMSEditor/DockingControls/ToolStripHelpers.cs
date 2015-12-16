using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;

namespace Darwen.Windows.Forms.General
{
    public static class ToolStripHelpers
    {
        static public ICollection<ToolStripButton> GetVisibleButtons(ToolStrip toolStrip)
        {
            List<ToolStripButton> visibleButtons = new List<ToolStripButton>();

            foreach (ToolStripButton button in toolStrip.Items)
            {
                if (button.Visible)
                {
                    visibleButtons.Add(button);
                }
            }

            return visibleButtons;
        }

        static public int GetVisibleButtonCount(ToolStrip toolStrip)
        {
            int count = 0;

            foreach (ToolStripButton button in toolStrip.Items)
            {
                if (button.Visible)
                {
                    count += 1;
                }
            }

            return count;
        }
    }
}
