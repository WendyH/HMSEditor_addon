using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace HMSEditor_addon {
	class HmsEditBox: RichTextBox {

		// constructor
		public HmsEditBox(IntPtr parentHandle) {

		}

		public int GetCurrentLine() {
			return GetLineFromCharIndex(SelectionStart);
		}

		public void GetCaretPos(ref int aLine, ref int aChar) {
			Point p = GetPositionFromCharIndex(SelectionStart);
			aLine = p.X;
			aChar = p.Y;
        }

		public void SetCaretPos(int aLine, int aChar) {
			int iLen = 0;
			int iRow = 0;
			foreach (string str in Lines) {
				iRow++;
				iLen += str.Length;
				if (iRow == aLine)
					break;
				iLen += aChar;
				SelectionStart = iLen;
			}
		}

	}
}
