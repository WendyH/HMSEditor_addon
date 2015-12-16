using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Darwen.Windows.Forms.Controls
{
    /// <summary>
    /// This is needed because normal MenuStrips don't pass through WM_MOUSEACTIVATE messages.
    /// This has the effect of not passing on a MouseDown event when the control is clicked if its parent form isn't active.
    /// I.e. if you have a floating window you have to click once to enable it and then click again for the control to receive
    /// a mouse down message. The same is true for anything derived from ToolStrip.
    /// In the docking control instance this means that you have to click first to activate, then click & drag again to drag floating windows.
    /// </summary>
    public class MenuStrip : System.Windows.Forms.MenuStrip
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Darwen.Windows.Forms.General.NativeMethods.Constants.WM_MOUSEACTIVATE)
            {
                base.DefWndProc(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}
