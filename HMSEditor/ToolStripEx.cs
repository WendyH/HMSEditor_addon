using System;
using System.Windows.Forms;

namespace HMSEditorNS {
	/// <summary>
	/// This class adds on to the functionality provided in System.Windows.Forms.ToolStrip.
	/// </summary>
	public class ToolStripEx : ToolStrip {
		const uint WM_MOUSEACTIVATE = 0x21;
		const uint MA_ACTIVATE         = 1;
		const uint MA_ACTIVATEANDEAT   = 2;

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);

			if (m.Msg == WM_MOUSEACTIVATE && m.Result == (IntPtr)MA_ACTIVATEANDEAT)
				m.Result = (IntPtr)MA_ACTIVATE;
		}

	}
}
