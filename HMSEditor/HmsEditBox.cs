using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace HmsAddons {
    class HmsEditBox: RichTextBox {
        public string ScriptLanguage = "ThisLang";

        [DllImport("User32")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        public static extern IntPtr SetParent(IntPtr hWndEditor, IntPtr hWndParent);

        // constructor
        public HmsEditBox(IntPtr parentHandle) {

            SetParent(Handle, parentHandle);

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

        public void OnRunningStateChange(bool val) {
        }

        public void HotKeysDialog() {
        }

        public void SaveSettings() {
        }

        public void RestorePosition() {
        }

    }
}
